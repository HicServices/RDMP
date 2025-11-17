// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAnsi.Discovery;
using NPOI.OpenXmlFormats.Dml;
using Org.BouncyCastle.Crypto.Signers;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Providers.Nodes.UsedByNodes;
using Rdmp.Core.Providers.Nodes.UsedByProject;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.Repositories.Managers.HighPerformance;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Providers;

/// <summary>
/// Finds the all the objects required for data export tree rendering including which objects are children of others
/// and the descendancy for each object etc.  This class inherits from CatalogueChildProvider because you cannot have
/// one without the other and one Data Export database always maps to one (and only one) Catalogue database.
/// </summary>
public class DataExportChildProvider : CatalogueChildProvider
{
    //TODO - there are still some issues here with speed, they are noted with a TODO flag
    // there is an issue with folder stuctures not updating in the UI when you change the name 
    // might need to have the UI watch for updastes?
    //there is a "double tap" when saving objects...

    //root objects
    LazyWithReset<AllCohortsNode> _lazyRootCohortsNode = new(() => new AllCohortsNode());
    public AllCohortsNode RootCohortsNode { get => _lazyRootCohortsNode.Value; }

    private readonly ICheckNotifier _errorsCheckNotifier;


    LazyWithReset<ExternalCohortTable[]> _lazyCohortSources = new(() => []);
    public ExternalCohortTable[] CohortSources { get => _lazyCohortSources.Value; }

    LazyWithReset<ExtractableDataSet[]> _lazyExtractableDataSets = new(() => []);
    public ExtractableDataSet[] ExtractableDataSets { get => _lazyExtractableDataSets.Value; }

    LazyWithReset<ExtractableDataSetProject[]> _lazyExtractableDataSetProjects = new(() => []);
    public ExtractableDataSetProject[] ExtractableDataSetProjects { get => _lazyExtractableDataSetProjects.Value; }

    LazyWithReset<SelectedDataSets[]> _lazySelectedDataSets = new(() => []);
    public SelectedDataSets[] SelectedDataSets { get => _lazySelectedDataSets.Value; }

    LazyWithReset<Dictionary<int, ExtractionProgress>> _lazy_extractionProgressesBySelectedDataSetID = new(() => []);
    public Dictionary<int, ExtractionProgress> _extractionProgressesBySelectedDataSetID { get => _lazy_extractionProgressesBySelectedDataSetID.Value; }

    LazyWithReset<ExtractableDataSetPackage[]> _lazyAllPackages { get; set; }
    public ExtractableDataSetPackage[] AllPackages { get => _lazyAllPackages.Value; }


    LazyWithReset<FolderNode<Project>> _lazyProjectRootFolder;
    public FolderNode<Project> ProjectRootFolder { get => _lazyProjectRootFolder.Value; }

    LazyWithReset<Project[]> _lazyProjects = new(() => []);
    public Project[] Projects { get => _lazyProjects.Value; }

    LazyWithReset<Dictionary<int, HashSet<ExtractableCohort>>> _lazy_cohortsByOriginId;

    private Dictionary<int, HashSet<ExtractableCohort>> _cohortsByOriginId { get => _lazy_cohortsByOriginId.Value; }

    LazyWithReset<ExtractableCohort[]> _lazyCohorts = new(() => []);
    public ExtractableCohort[] Cohorts { get => _lazyCohorts.Value; }


    LazyWithReset<ExtractionConfiguration[]> _lazyExtractionConfigurations = new(() => []);
    public ExtractionConfiguration[] ExtractionConfigurations { get => _lazyExtractionConfigurations.Value; }

    LazyWithReset<Dictionary<int, List<ExtractionConfiguration>>> _lazyExtractionConfigurationsByProject;
    public Dictionary<int, List<ExtractionConfiguration>> ExtractionConfigurationsByProject { get => _lazyExtractionConfigurationsByProject.Value; }

    LazyWithReset<Dictionary<IExtractionConfiguration, List<SelectedDataSets>>> _lazy_configurationToDatasetMapping;

    private Dictionary<IExtractionConfiguration, List<SelectedDataSets>> _configurationToDatasetMapping { get => _lazy_configurationToDatasetMapping.Value; }
    private IFilterManager _dataExportFilterManager;


    LazyWithReset<List<ExternalCohortTable>> _lazyForbidListedSources = new(() => []);
    public List<ExternalCohortTable> ForbidListedSources { get => _lazyForbidListedSources.Value; }

    LazyWithReset<List<IObjectUsedByOtherObjectNode<Project, IMapsDirectlyToDatabaseTable>>> _lazyDuplicatesByProject = new(() => new List<IObjectUsedByOtherObjectNode<Project, IMapsDirectlyToDatabaseTable>>());
    public List<IObjectUsedByOtherObjectNode<Project, IMapsDirectlyToDatabaseTable>> DuplicatesByProject { get => _lazyDuplicatesByProject.Value; }

    LazyWithReset<List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>>> _lazyDuplicatesByCohortSourceUsedByProjectNode = new(() => []);

    public List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>> DuplicatesByCohortSourceUsedByProjectNode { get => _lazyDuplicatesByCohortSourceUsedByProjectNode.Value; }


    private readonly object _oProjectNumberToCohortsDictionary = new();

    LazyWithReset<Dictionary<int, List<ExtractableCohort>>> _lazyProjectNumberToCohortsDictionary = new(() => new Dictionary<int, List<ExtractableCohort>>());
    public Dictionary<int, List<ExtractableCohort>> ProjectNumberToCohortsDictionary { get => _lazyProjectNumberToCohortsDictionary.Value; }

    LazyWithReset<ProjectCohortIdentificationConfigurationAssociation[]> _lazyAllProjectAssociatedCics = new(() => []);

    public ProjectCohortIdentificationConfigurationAssociation[] AllProjectAssociatedCics { get => _lazyAllProjectAssociatedCics.Value; }

    LazyWithReset<GlobalExtractionFilterParameter[]> _lazyAllGlobalExtractionFilterParameters = new(() => []);
    public GlobalExtractionFilterParameter[] AllGlobalExtractionFilterParameters { get => _lazyAllGlobalExtractionFilterParameters.Value; }

    /// <summary>
    /// ID of all CohortIdentificationConfiguration which have an ProjectCohortIdentificationConfigurationAssociation declared on them (i.e. the CIC is used with one or more Projects)
    /// </summary>
    /// 
    LazyWithReset<HashSet<int>> _lazy_cicAssociations = new(() => new HashSet<int>());
    private HashSet<int> _cicAssociations { get => _lazy_cicAssociations.Value; }


    LazyWithReset<HashSet<ISelectedDataSets>> _lazy_selectedDataSetsWithNoIsExtractionIdentifier = new(() => new HashSet<ISelectedDataSets>());
    private HashSet<ISelectedDataSets> _selectedDataSetsWithNoIsExtractionIdentifier { get => _lazy_selectedDataSetsWithNoIsExtractionIdentifier.Value; }

    /// <summary>
    /// All AND/OR containers found during construction (in the data export database).  The Key is the ID of the container (for rapid random access)
    /// </summary>
    LazyWithReset<Dictionary<int, FilterContainer>> _lazyAllContainers;
    public Dictionary<int, FilterContainer> AllContainers { get => _lazyAllContainers.Value; }

    /// <summary>
    /// All data export filters that existed when this child provider was constructed
    /// </summary>
    /// 
    LazyWithReset<DeployedExtractionFilter[]> _lazyAllDeployedExtractionFilters = new(() => []);
    public DeployedExtractionFilter[] AllDeployedExtractionFilters { get => _lazyAllDeployedExtractionFilters.Value; }

    LazyWithReset<DeployedExtractionFilterParameter[]> _lazy_allParameters = new(() => []);

    private DeployedExtractionFilterParameter[] _allParameters { get => _lazy_allParameters.Value; }

    private IDataExportRepository dataExportRepository;

    public DataExportChildProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier,
        DataExportChildProvider previousStateIfKnown) : base(repositoryLocator.CatalogueRepository,
        pluginChildProviders, errorsCheckNotifier, previousStateIfKnown)
    {
        _lazyForbidListedSources = new LazyWithReset<List<ExternalCohortTable>>(() => {
            return previousStateIfKnown?.ForbidListedSources ?? new List<ExternalCohortTable>();
        });
        _errorsCheckNotifier = errorsCheckNotifier;
        dataExportRepository = repositoryLocator.DataExportRepository;

        _lazyAllProjectAssociatedCics = new LazyWithReset<ProjectCohortIdentificationConfigurationAssociation[]>(() =>
            GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>(dataExportRepository));

        _lazy_cicAssociations = new LazyWithReset<HashSet<int>>(() =>
            new HashSet<int>(AllProjectAssociatedCics.Select(a => a.CohortIdentificationConfiguration_ID)));

        _lazyCohortSources = new LazyWithReset<ExternalCohortTable[]>(() => GetAllObjects<ExternalCohortTable>(dataExportRepository));
        _lazyExtractableDataSets = new LazyWithReset<ExtractableDataSet[]>(() =>
        {
            var x = GetAllObjects<ExtractableDataSet>(dataExportRepository);
            var catalogueIdDict = AllCatalogues.ToDictionaryEx(c => c.ID, c2 => c2);
            foreach (var ds in x)
                if (catalogueIdDict.TryGetValue(ds.Catalogue_ID, out var cata))
                    ds.InjectKnown(cata);
            return x;
        });
        _lazyExtractableDataSetProjects = new LazyWithReset<ExtractableDataSetProject[]>(() => GetAllObjects<ExtractableDataSetProject>(dataExportRepository));
        //This means that the ToString method in ExtractableDataSet doesn't need to go lookup catalogue info

        ReportProgress("Injecting ExtractableDataSet");

        _lazyAllPackages = new LazyWithReset<ExtractableDataSetPackage[]>(() =>
        {
            var x = GetAllObjects<ExtractableDataSetPackage>(dataExportRepository);
            foreach (var package in x)
                AddChildren(package, new DescendancyList(package));
            return x;
        });

        _lazyProjects = new LazyWithReset<Project[]>(() => GetAllObjects<Project>(dataExportRepository));
        _lazyExtractionConfigurations = new LazyWithReset<ExtractionConfiguration[]>(() => GetAllObjects<ExtractionConfiguration>(dataExportRepository));

        ReportProgress("Get Projects and Configurations");

        _lazyExtractionConfigurationsByProject = new LazyWithReset<Dictionary<int, List<ExtractionConfiguration>>>(() => ExtractionConfigurations.GroupBy(k => k.Project_ID)
            .ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList()));

        ReportProgress("Grouping Extractions by Project");

        BuildSelectedDatasets();

        _lazyAllGlobalExtractionFilterParameters = new LazyWithReset<GlobalExtractionFilterParameter[]>(() => GetAllObjects<GlobalExtractionFilterParameter>(dataExportRepository));

        BuildExtractionFilters();

        ReportProgress("Building FilterManager");

        _lazyCohorts = new LazyWithReset<ExtractableCohort[]>(() =>
        {
            var x = GetAllObjects<ExtractableCohort>(dataExportRepository);
            return x;
        }
        );
        _lazy_cohortsByOriginId = new LazyWithReset<Dictionary<int, HashSet<ExtractableCohort>>>(() =>
        {

            var x = new Dictionary<int, HashSet<ExtractableCohort>>();
            foreach (var c in Cohorts)
            {
                if (!x.ContainsKey(c.OriginID))
                    x.Add(c.OriginID, new HashSet<ExtractableCohort>());

                x[c.OriginID].Add(c);
            }
            return x;
        });



        ReportProgress("Fetching Cohorts");


        _lazyProjectNumberToCohortsDictionary = new LazyWithReset<Dictionary<int, List<ExtractableCohort>>>(() =>
        {
            var x = new Dictionary<int, List<ExtractableCohort>>();
            foreach (var cohort in CohortSources.Except(ForbidListedSources))
            {
                GetCohortAvailability(cohort, x);
            }
            return x;
        });

        ReportProgress("GetCohortAvailability");


        ReportProgress("Mapping configurations to datasets");

        _lazyRootCohortsNode = new LazyWithReset<AllCohortsNode>(() =>
        {
            var x = new AllCohortsNode();
            AddChildren(x, new DescendancyList(x));
            return x;
        });



        ReportProgress("Packages and Cohorts");

        _lazyProjectRootFolder = new LazyWithReset<FolderNode<Project>>(() =>
        {
            var x = FolderHelper.BuildFolderTree(Projects);
            AddChildren(x, new DescendancyList(x));
            return x;
        });

        ReportProgress("Projects");
        //inject extractability into Catalogues
        foreach (var catalogue in AllCatalogues)
        {
            var eds = ExtractableDataSets.Where(e => e.Catalogue_ID == catalogue.ID).ToList();
            if (!eds.Any())
            {
                catalogue.InjectKnown(new CatalogueExtractabilityStatus(false, false));
            }
            else
            {
                catalogue.InjectKnown(new CatalogueExtractabilityStatus(true, eds.First().Projects.Any()));
            }
        }
        ReportProgress("Catalogue extractability injection");

        //try//TODO
        //{
        //    AddPipelineUseCases(new Dictionary<string, PipelineUseCase>
        //    {
        //        { "File Import", UploadFileUseCase.DesignTime() },
        //        { "Extraction", ExtractionPipelineUseCase.DesignTime() },
        //        { "Release", ReleaseUseCase.DesignTime() },
        //        { "Cohort Creation", CohortCreationRequest.DesignTime() },
        //        { "Caching", CachingPipelineUseCase.DesignTime() },
        //        {
        //            "Aggregate Committing",
        //            CreateTableFromAggregateUseCase.DesignTime(repositoryLocator.CatalogueRepository)
        //        }
        //    });
        //}
        //catch (Exception ex)
        //{
        //    _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to build DesignTime PipelineUseCases",
        //        CheckResult.Fail, ex));
        //}

        ReportProgress("Pipeline adding");

        GetPluginChildren();//TODO
    }


    private void AddChildren(FolderNode<Project> folder, DescendancyList descendancy)
    {
        foreach (var child in folder.ChildFolders)
            //add subfolder children
            AddChildren(child, descendancy.Add(child));

        //add catalogues in folder
        foreach (var project in folder.ChildObjects) AddChildren(project, descendancy.Add(project));

        // Children are the folders + objects
        //AddToDictionaries(new HashSet<object>(
        //        folder.ChildFolders.Cast<object>()
        //            .Union(folder.ChildObjects)), descendancy
        //);
    }

    private void BuildSelectedDatasets()
    {
        _lazy_selectedDataSetsWithNoIsExtractionIdentifier = new LazyWithReset<HashSet<ISelectedDataSets>>(() =>
            new HashSet<ISelectedDataSets>(dataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers()));

        _lazySelectedDataSets = new LazyWithReset<SelectedDataSets[]>(() =>
        {
            var x = GetAllObjects<SelectedDataSets>(dataExportRepository);
            var dsDictionary = ExtractableDataSets.ToDictionaryEx(ds => ds.ID, d => d);
            foreach (var s in x)
                s.InjectKnown(dsDictionary[s.ExtractableDataSet_ID]);
            return x;
        });
        ReportProgress("Fetching data export objects");

        _lazy_extractionProgressesBySelectedDataSetID = new LazyWithReset<Dictionary<int, ExtractionProgress>>(() => GetAllObjects<ExtractionProgress>(dataExportRepository)
            .ToDictionaryEx(ds => ds.SelectedDataSets_ID, d => d));

        ReportProgress("Injecting SelectedDataSets");

        _lazy_configurationToDatasetMapping = new LazyWithReset<Dictionary<IExtractionConfiguration, List<SelectedDataSets>>>(() =>
        {

            var x = new Dictionary<IExtractionConfiguration, List<SelectedDataSets>>();
            var configToSds = SelectedDataSets.GroupBy(k => k.ExtractionConfiguration_ID)
         .ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList());

            foreach (var configuration in ExtractionConfigurations)
                if (configToSds.TryGetValue(configuration.ID, out var result))
                    x.Add(configuration, result);
            return x;
        });


    }

    private void BuildExtractionFilters()
    {
        _lazyAllContainers = new LazyWithReset<Dictionary<int, FilterContainer>>(() => GetAllObjects<FilterContainer>(dataExportRepository).ToDictionaryEx(o => o.ID, o => o));
        _lazyAllDeployedExtractionFilters = new LazyWithReset<DeployedExtractionFilter[]>(() => GetAllObjects<DeployedExtractionFilter>(dataExportRepository));
        _lazy_allParameters = new LazyWithReset<DeployedExtractionFilterParameter[]>(() => GetAllObjects<DeployedExtractionFilterParameter>(dataExportRepository));

        ReportProgress("Getting Filters");

        //if we are using a database repository then we can make use of the caching class DataExportFilterManagerFromChildProvider to speed up
        //filter contents
        _dataExportFilterManager = dataExportRepository is not DataExportRepository dbRepo
            ? dataExportRepository.FilterManager
            : new DataExportFilterManagerFromChildProvider(dbRepo, this);
    }

    private void AddChildren(IExtractableDataSetPackage package, DescendancyList descendancy)
    {
        var children = new HashSet<object>(dataExportRepository.GetAllDataSets(package, ExtractableDataSets)
            .Select(ds => new PackageContentNode(package, ds, dataExportRepository)));

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(Project project, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        var projectCohortNode = new ProjectCohortsNode(project);
        children.Add(projectCohortNode);
        AddChildren(projectCohortNode, descendancy.Add(projectCohortNode));

        var projectCataloguesNode = new ProjectCataloguesNode(project);
        children.Add(projectCataloguesNode);
        AddChildren(projectCataloguesNode, descendancy.Add(projectCataloguesNode).SetNewBestRoute());

        var extractionConfigurationsNode = new ExtractionConfigurationsNode(project);
        children.Add(extractionConfigurationsNode);

        AddChildren(extractionConfigurationsNode, descendancy.Add(extractionConfigurationsNode));

        var folder = new ExtractionDirectoryNode(project);
        children.Add(folder);
        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(ProjectCataloguesNode projectCataloguesNode, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        if (ExtractableDataSetProjects.Any())
        {
            foreach (var projectSpecificEdsp in ExtractableDataSetProjects.Where(edsp => edsp.Project_ID == projectCataloguesNode.Project.ID))
            {
                var eds = ExtractableDataSets.FirstOrDefault(eds => eds.ID == projectSpecificEdsp.ExtractableDataSet_ID);
                if (eds != null)
                {
                    var cata = (Catalogue)eds.Catalogue;

                    // cata will be null if it has been deleted from the database
                    if (cata != null)
                    {
                        children.Add(cata);
                        //AddChildren(cata, descendancy.Add(eds.Catalogue));
                    }
                }
            }
        }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(ProjectCohortsNode projectCohortsNode, DescendancyList descendancy)
    {
        var children = new HashSet<object>();
        var projectCiCsNode = new ProjectCohortIdentificationConfigurationAssociationsNode(projectCohortsNode.Project);
        children.Add(projectCiCsNode);
        AddChildren(projectCiCsNode, descendancy.Add(projectCiCsNode));

        var savedCohortsNode = new ProjectSavedCohortsNode(projectCohortsNode.Project);
        children.Add(savedCohortsNode);
        AddChildren(savedCohortsNode, descendancy.Add(savedCohortsNode));

        var associatedCohortConfigurations = new CommittedCohortIdentificationNode(projectCohortsNode.Project);
        children.Add(associatedCohortConfigurations);
        AddChildren(associatedCohortConfigurations, descendancy.Add(associatedCohortConfigurations));

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(CommittedCohortIdentificationNode associatedCohortConfigurations, DescendancyList descendancy)
    {
        var children = new HashSet<object>();
        var associatedCohorts = associatedCohortConfigurations.Project.GetAssociatedCohortIdentificationConfigurations();
        foreach (var cohort in associatedCohorts)
        {
            children.Add(cohort);
        }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(ProjectSavedCohortsNode savedCohortsNode, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        var cohortGroups = GetAllCohortProjectUsageNodesFor(savedCohortsNode.Project);

        foreach (var cohortSourceUsedByProjectNode in cohortGroups)
        {
            AddChildren(cohortSourceUsedByProjectNode, descendancy.Add(cohortSourceUsedByProjectNode));
            children.Add(cohortSourceUsedByProjectNode);
        }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(ProjectCohortIdentificationConfigurationAssociationsNode projectCiCsNode,
        DescendancyList descendancy)
    {
        //add the associations
        var children = new HashSet<object>();
        foreach (var association in AllProjectAssociatedCics.Where(assoc =>
                     assoc.Project_ID == projectCiCsNode.Project.ID))
        {
            var matchingCic = AllCohortIdentificationConfigurations.SingleOrDefault(cic =>
                cic.ID == association.CohortIdentificationConfiguration_ID);

            if (matchingCic == null)
            {
                _errorsCheckNotifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Failed to find Associated Cohort Identification Configuration with ID {association.CohortIdentificationConfiguration_ID} which was supposed to be associated with {association.Project}",
                        CheckResult
                            .Fail)); //inject knowledge of what the cic is so it doesn't have to be fetched during ToString
            }
            else
            {
                association.InjectKnown(matchingCic);

                //document that it is a child of the project cics node
                children.Add(association);
            }
        }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(ExtractionConfigurationsNode extractionConfigurationsNode, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        //Create a frozen extraction configurations folder as a subfolder of each ExtractionConfigurationsNode
        var frozenConfigurationsNode = new FrozenExtractionConfigurationsNode(extractionConfigurationsNode.Project);

        //Make the frozen folder appear under the extractionConfigurationsNode
        children.Add(frozenConfigurationsNode);

        //Add children to the frozen folder
        AddChildren(frozenConfigurationsNode, descendancy.Add(frozenConfigurationsNode));

        //Add ExtractionConfigurations which are not released (frozen)
        if (ExtractionConfigurationsByProject.TryGetValue(extractionConfigurationsNode.Project.ID, out var result))
            foreach (var config in result.Where(c => !c.IsReleased))
            {
                AddChildren(config, descendancy.Add(config));
                children.Add(config);
            }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(FrozenExtractionConfigurationsNode frozenExtractionConfigurationsNode,
        DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        //Add ExtractionConfigurations which are not released (frozen)
        if (ExtractionConfigurationsByProject.TryGetValue(frozenExtractionConfigurationsNode.Project.ID,
                out var result))
            foreach (var config in result.Where(c => c.IsReleased))
            {
                AddChildren(config, descendancy.Add(config));
                children.Add(config);
            }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(IExtractionConfiguration extractionConfiguration, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        var parameters = AllGlobalExtractionFilterParameters
            .Where(p => p.ExtractionConfiguration_ID == extractionConfiguration.ID)
            .ToArray();

        foreach (var p in parameters)
            children.Add(p);

        //if it has a cohort
        if (extractionConfiguration.Cohort_ID != null)
        {
            var cohort = Cohorts.Single(c => c.ID == extractionConfiguration.Cohort_ID);
            children.Add(new LinkedCohortNode(extractionConfiguration, cohort));
        }

        //if it has extractable datasets add those
        foreach (var ds in GetDatasets(extractionConfiguration))
        {
            children.Add(ds);
            AddChildren(ds, descendancy.Add(ds));
        }

        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(SelectedDataSets selectedDataSets, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        if (_extractionProgressesBySelectedDataSetID.TryGetValue(selectedDataSets.ID, out var value))
            children.Add(value);

        if (selectedDataSets.RootFilterContainer_ID != null)
        {
            var rootContainer = AllContainers[selectedDataSets.RootFilterContainer_ID.Value];
            children.Add(rootContainer);

            AddChildren(rootContainer, descendancy.Add(rootContainer));
        }

        //AddToDictionaries(children, descendancy);
    }


    private void AddChildren(FilterContainer filterContainer, DescendancyList descendancy)
    {
        var children = new HashSet<object>();

        foreach (FilterContainer subcontainer in _dataExportFilterManager.GetSubContainers(filterContainer))
        {
            AddChildren(subcontainer, descendancy.Add(subcontainer));
            children.Add(subcontainer);
        }

        foreach (DeployedExtractionFilter filter in _dataExportFilterManager.GetFilters(filterContainer))
        {
            AddChildren(filter, descendancy.Add(filter));
            children.Add(filter);
        }


        //AddToDictionaries(children, descendancy);
    }

    private void AddChildren(DeployedExtractionFilter filter, DescendancyList descendancyList)
    {
        //AddToDictionaries(new HashSet<object>(_allParameters.Where(p => p.ExtractionFilter_ID == filter.ID)),
        //    descendancyList);
    }

    private void AddChildren(CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode, DescendancyList descendancy)
    {
        //AddToDictionaries(new HashSet<object>(cohortSourceUsedByProjectNode.CohortsUsed), descendancy);
    }

    private void AddChildren(AllCohortsNode cohortsNode, DescendancyList descendancy)
    {
        var validSources = CohortSources.ToArray();

        //AddToDictionaries(new HashSet<object>(validSources), descendancy);
        foreach (var s in validSources)
            AddChildren(s, descendancy.Add(s));
    }

    private void AddChildren(ExternalCohortTable externalCohortTable, DescendancyList descendancy)
    {
        var cohorts = Cohorts.Where(c => c.ExternalCohortTable_ID == externalCohortTable.ID).ToArray();

        foreach (var cohort in cohorts)
            cohort.InjectKnown(externalCohortTable);

        //AddToDictionaries(new HashSet<object>(cohorts), descendancy);
    }

    private void GetCohortAvailability(ExternalCohortTable source, Dictionary<int, List<ExtractableCohort>> cohortList)
    {
        DiscoveredServer server = null;

        Exception ex = null;

        //it obviously hasn't been initialised properly yet
        if (string.IsNullOrWhiteSpace(source.Server) || string.IsNullOrWhiteSpace(source.Database))
            return;

        try
        {
            server = DataAccessPortal.ExpectDatabase(source, DataAccessContext.DataExport).Server;
        }
        catch (Exception exception)
        {
            ex = exception;
        }

        if (server == null || !server.RespondsWithinTime(3, out ex) || !source.IsFullyPopulated())
        {
            ForbidList(source, ex);
            return;
        }

        try
        {
            using var con = server.GetConnection();
            con.Open();

            //Get all of the project numbers and remote origin ids etc from the source in one query
            using var cmd = server.GetCommand(source.GetExternalDataSql(), con);
            cmd.CommandTimeout = 120;

            using var r = cmd.ExecuteReader();
            while (r.Read())
                //really should be only one here but still they might for some reason have 2 references to the same external cohort
                if (_cohortsByOriginId.TryGetValue(Convert.ToInt32(r["OriginID"]), out var result))
                    //Tell the cohorts what their external data values are so they don't have to fetch them themselves individually
                    foreach (var c in result.Where(c => c.ExternalCohortTable_ID == source.ID))
                    {
                        //load external data from the result set
                        var externalData = new ExternalCohortDefinitionData(r, source.Name);

                        //tell the cohort about the data
                        c.InjectKnown(externalData);

                        lock (_oProjectNumberToCohortsDictionary)
                        {
                            //for performance also keep a dictionary of project number => compatible cohorts
                            if (!cohortList.ContainsKey(externalData.ExternalProjectNumber))
                                cohortList.Add(externalData.ExternalProjectNumber,
                                    new List<ExtractableCohort>());

                            cohortList[externalData.ExternalProjectNumber].Add(c);
                        }
                    }
        }
        catch (Exception e)
        {
            ForbidList(source, e);
        }
    }

    /// <summary>
    /// Marks the <paramref name="source"/> as unreachable and prevents future
    /// attempts to retrieve it.  This is important as it can take multiple seconds
    /// to determine that a server doesn't exist (e.g. network TCP timeout).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="ex"></param>
    private void ForbidList(ExternalCohortTable source, Exception ex)
    {
        ForbidListedSources.Add(source);

        // notify being unable to reach cohorts unless user has suppressed this
        if (UserSettings.GetErrorReportingLevelFor(ErrorCodes.CouldNotReachCohort) != CheckResult.Success)
            _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.CouldNotReachCohort, ex, source));

        //tell them not to bother looking for the cohort data because its inaccessible
        foreach (var cohort in Cohorts.Where(c => c.ExternalCohortTable_ID == source.ID).ToArray())
            cohort.InjectKnown((ExternalCohortDefinitionData)null);
    }

    /// <summary>
    /// Returns all cohort sources used by a <see cref="Project"/>.  Returned object
    /// contains references to the cohorts being used.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public List<CohortSourceUsedByProjectNode> GetAllCohortProjectUsageNodesFor(Project project)
    {
        //if the current project does not have a number or there are no cohorts associated with it
        if (project.ProjectNumber == null ||
            !ProjectNumberToCohortsDictionary.TryGetValue(project.ProjectNumber.Value, out var cohorts))
            return new List<CohortSourceUsedByProjectNode>();

        var toReturn = new List<CohortSourceUsedByProjectNode>();


        foreach (var cohort in cohorts)
        {
            //get the source of the cohort
            var source = CohortSources.Single(s => s.ID == cohort.ExternalCohortTable_ID);

            //numbers match
            var existing = toReturn.SingleOrDefault(s => s.ObjectBeingUsed.ID == cohort.ExternalCohortTable_ID);

            //make sure we have a record of the source
            if (existing == null)
            {
                existing = new CohortSourceUsedByProjectNode(project, source);
                toReturn.Add(existing);
            }

            //add the cohort to the list of known cohorts from this source (a project can have lots of cohorts and even cohorts from different sources)
            var cohortUsedByProject =
                new ObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode, ExtractableCohort>(existing, cohort);
            existing.CohortsUsed.Add(cohortUsedByProject);

            DuplicatesByCohortSourceUsedByProjectNode.Add(cohortUsedByProject);
        }

        DuplicatesByProject.AddRange(toReturn);

        //if the project has no cohorts then add a ??? node
        if (!toReturn.Any())
            toReturn.Add(new CohortSourceUsedByProjectNode(project, null));

        return toReturn;
    }

    public IEnumerable<ExtractionConfiguration> GetActiveConfigurationsOnly(Project project)
    {
        lock (WriteLock)
        {
            return GetConfigurations(project).Where(ec => !ec.IsReleased);
        }
    }

    public IEnumerable<SelectedDataSets> GetDatasets(IExtractionConfiguration extractionConfiguration)
    {
        lock (WriteLock)
        {
            return _configurationToDatasetMapping.TryGetValue(extractionConfiguration, out var result)
                ? (IEnumerable<SelectedDataSets>)result
                : Array.Empty<SelectedDataSets>();
        }
    }

    public IEnumerable<ExtractionConfiguration> GetConfigurations(IProject project)
    {
        lock (WriteLock)
        {
            var x = GetChildren(project);
            //Get the extraction configurations node of the project
            var configurationsNode = GetChildren(project).OfType<ExtractionConfigurationsNode>().Single();

            var frozenConfigurationsNode =
                GetChildren(configurationsNode).OfType<FrozenExtractionConfigurationsNode>().Single();

            //then add all the children extraction configurations
            return GetChildren(configurationsNode).OfType<ExtractionConfiguration>()
                .Union(GetChildren(frozenConfigurationsNode).OfType<ExtractionConfiguration>());
        }
    }

    public override object[] GetChildren(object model)
    {
        lock(WriteLock)
        {
            return model switch
            {
                Project p => GetChildren(p).ToArray(),
                ExtractionConfigurationsNode ecn => GetChildren(ecn).ToArray(),
                FrozenExtractionConfigurationsNode ecn => GetChildren(ecn).ToArray(),
                ProjectCohortsNode pcn => GetChildren(pcn).ToArray(),
                ProjectCohortIdentificationConfigurationAssociationsNode pcican => GetChildren(pcican).ToArray(),
                _ => base.GetChildren(model)
            };

        }
    }

    private HashSet<object> GetChildren(Project project)
    {
        return [.. new object[]{
            new ExtractionConfigurationsNode(project),
            new ProjectCohortsNode(project)
        }];
    }

    private HashSet<object> GetChildren(ProjectCohortIdentificationConfigurationAssociationsNode pcican)
    {
        return new HashSet<object>(pcican.Project.GetAssociatedCohortIdentificationConfigurations());
    }

    private HashSet<object> GetChildren(ProjectCohortsNode pcn)
    {
        var configs  = pcn.Project.GetAssociatedCohortIdentificationConfigurations();//todo I don't think this is correct
        var node = new ProjectCohortIdentificationConfigurationAssociationsNode(pcn.Project);
        var x = new List<object>();
        x.AddRange(configs);
        x.Add(node);
        return new HashSet<object>(x);
    }

    private HashSet<object> GetChildren(ExtractionConfigurationsNode node)
    {
        ExtractionConfigurationsByProject.TryGetValue(node.Project.ID, out List<ExtractionConfiguration> vals);
        var x = new List<object>() { new FrozenExtractionConfigurationsNode(node.Project) };
        x.AddRange(vals ?? new List<ExtractionConfiguration>());
        return new HashSet<object>(x.ToArray());
    }

    private HashSet<object> GetChildren(FrozenExtractionConfigurationsNode node)
    {
        ExtractionConfigurationsByProject.TryGetValue(node.Project.ID, out List<ExtractionConfiguration> vals);
        return new HashSet<object>(vals ?? new List<ExtractionConfiguration>());
    }

    public IEnumerable<IExtractableDataSet> GetDatasets(ExtractableDataSetPackage package)
    {
        lock (WriteLock)
        {
            return dataExportRepository.GetAllDataSets(package, ExtractableDataSets);
        }
    }

    public bool ProjectHasNoSavedCohorts(Project project)
    {
        lock (WriteLock)
        {
            //get the projects cohort umbrella folder
            var projectCohortsNode = GetChildren(project).OfType<ProjectCohortsNode>().Single();

            //get the saved cohorts folder under it
            var projectSavedCohortsNode = GetChildren(projectCohortsNode).OfType<ProjectSavedCohortsNode>().Single();

            //if ther are no children that are Cohort Sources (cohort databases) under this saved cohorts folder then the Project has no
            return GetChildren(projectSavedCohortsNode).OfType<CohortSourceUsedByProjectNode>().All(s => s.IsEmptyNode);
        }
    }

    public override Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables()
    {
        lock (WriteLock)
        {
            var toReturn = base.GetAllSearchables();
            foreach (var c in Projects) toReturn.Add(c, new DescendancyList());

            AddToReturnSearchablesWithNoDecendancy(toReturn, AllPackages);
            return toReturn;
        }
    }

    public bool IsMissingExtractionIdentifier(SelectedDataSets selectedDataSets)
    {
        lock (WriteLock)
        {
            return _selectedDataSetsWithNoIsExtractionIdentifier.Contains(selectedDataSets);
        }
    }

    /// <summary>
    /// Returns all <see cref="ExtractableColumn"/> Injected with their corresponding <see cref="ExtractionInformation"/>
    /// </summary>
    /// <param name="repository"></param>
    /// <returns></returns>
    public ExtractableColumn[] GetAllExtractableColumns(IDataExportRepository repository)
    {
        lock (WriteLock)
        {
            var toReturn = repository.GetAllObjects<ExtractableColumn>();
            foreach (var c in toReturn)
                if (c.CatalogueExtractionInformation_ID == null)
                {
                    c.InjectKnown((ExtractionInformation)null);
                }
                else
                {
                    if (AllExtractionInformationsDictionary.TryGetValue(c.CatalogueExtractionInformation_ID.Value,
                            out var ei))
                        c.InjectKnown(ei);
                }

            return toReturn;
        }
    }

    public override bool SelectiveRefresh(IMapsDirectlyToDatabaseTable databaseEntity)
    {
        ProgressStopwatch.Restart();

        return databaseEntity switch
        {
            ExternalCohortTable _ => SelectiveRefresh(typeof(ExternalCohortTable)),
            ExtractableDataSet _ => SelectiveRefresh(typeof(ExtractableDataSet)),
            DataExport.Data.SelectedDataSets _ => SelectiveRefresh(typeof(DataExport.Data.SelectedDataSets)),
            ExtractionProgress _ => SelectiveRefresh(typeof(ExtractionProgress)),
            ExtractableDataSetPackage _ => SelectiveRefresh(typeof(ExtractableDataSetPackage)),
            Project _ => SelectiveRefresh(typeof(Project)),
            ExtractableCohort _ => SelectiveRefresh(typeof(ExtractableCohort)),
            ExtractionConfiguration _ => SelectiveRefresh(typeof(ExtractionConfiguration)),
            ProjectCohortIdentificationConfigurationAssociation _ => SelectiveRefresh(typeof(ProjectCohortIdentificationConfigurationAssociation)),
            GlobalExtractionFilterParameter _ => SelectiveRefresh(typeof(GlobalExtractionFilterParameter)),
            FilterContainer _ => SelectiveRefresh(typeof(FilterContainer)),
            DeployedExtractionFilter _ => SelectiveRefresh(typeof(DeployedExtractionFilter)),
            DeployedExtractionFilterParameter _ => SelectiveRefresh(typeof(DeployedExtractionFilterParameter)),
            _ => base.SelectiveRefresh(databaseEntity)
        };
    }

    public override bool SelectiveRefresh(Type t)
    {
        if (t == typeof(DeployedExtractionFilterParameter))
        {
            _lazy_allParameters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExternalCohortTable))
        {
            _lazyCohortSources.Reset();
            _lazyRootCohortsNode.Reset();
            _lazyForbidListedSources.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractableDataSet))
        {
            _lazyExtractableDataSets.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(DataExport.Data.SelectedDataSets))
        {
            _lazySelectedDataSets.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionProgress))
        {
            _lazy_extractionProgressesBySelectedDataSetID.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractableDataSetPackage))
        {
            _lazyAllPackages.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(Project))
        {
            _lazyProjects.Reset();
            _lazyProjectRootFolder.Reset();
            _lazyAllPackages.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractableCohort))
        {
            _lazyCohorts.Reset();
            _lazyForbidListedSources.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionConfiguration))
        {
            _lazyExtractionConfigurations.Reset();
            _lazyExtractionConfigurationsByProject.Reset();
            _lazyProjects.Reset();
            _lazyProjectRootFolder.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ProjectCohortIdentificationConfigurationAssociation))
        {
            _lazyAllProjectAssociatedCics.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(GlobalExtractionFilterParameter))
        {
            _lazyAllGlobalExtractionFilterParameters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(FilterContainer))
        {
            _lazyAllContainers.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(DeployedExtractionFilter))
        {
            _lazyAllDeployedExtractionFilters.Reset();
            return SelectiveRefreshParents(t);
        }
        return base.SelectiveRefresh(t);
    }
}
