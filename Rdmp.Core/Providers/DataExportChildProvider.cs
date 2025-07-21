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
    //root objects
    public AllCohortsNode RootCohortsNode { get; private set; }

    private readonly ICheckNotifier _errorsCheckNotifier;

    public ExternalCohortTable[] CohortSources { get; private set; }
    public ExtractableDataSet[] ExtractableDataSets { get; private set; }
    public ExtractableDataSetProject[] ExtractableDataSetProjects { get; private set; }
    public SelectedDataSets[] SelectedDataSets { get; private set; }
    public Dictionary<int, ExtractionProgress> _extractionProgressesBySelectedDataSetID { get; private set; }

    public ExtractableDataSetPackage[] AllPackages { get; set; }

    public FolderNode<Project> ProjectRootFolder { get; private set; }
    public Project[] Projects { get; set; }

    private Dictionary<int, HashSet<ExtractableCohort>> _cohortsByOriginId;
    public ExtractableCohort[] Cohorts { get; private set; }

    public ExtractionConfiguration[] ExtractionConfigurations { get; private set; }
    public Dictionary<int, List<ExtractionConfiguration>> ExtractionConfigurationsByProject { get; set; }

    private Dictionary<IExtractionConfiguration, List<SelectedDataSets>> _configurationToDatasetMapping;
    private IFilterManager _dataExportFilterManager;

    public List<ExternalCohortTable> ForbidListedSources { get; private set; }

    public List<IObjectUsedByOtherObjectNode<Project, IMapsDirectlyToDatabaseTable>> DuplicatesByProject = new();

    public List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>> DuplicatesByCohortSourceUsedByProjectNode =
        new();


    private readonly object _oProjectNumberToCohortsDictionary = new();
    public Dictionary<int, List<ExtractableCohort>> ProjectNumberToCohortsDictionary = new();

    public ProjectCohortIdentificationConfigurationAssociation[] AllProjectAssociatedCics;

    public GlobalExtractionFilterParameter[] AllGlobalExtractionFilterParameters;

    /// <summary>
    /// ID of all CohortIdentificationConfiguration which have an ProjectCohortIdentificationConfigurationAssociation declared on them (i.e. the CIC is used with one or more Projects)
    /// </summary>
    private HashSet<int> _cicAssociations;

    private HashSet<ISelectedDataSets> _selectedDataSetsWithNoIsExtractionIdentifier;

    /// <summary>
    /// All AND/OR containers found during construction (in the data export database).  The Key is the ID of the container (for rapid random access)
    /// </summary>
    public Dictionary<int, FilterContainer> AllContainers;

    /// <summary>
    /// All data export filters that existed when this child provider was constructed
    /// </summary>
    public DeployedExtractionFilter[] AllDeployedExtractionFilters { get; private set; }

    private DeployedExtractionFilterParameter[] _allParameters;

    private IDataExportRepository dataExportRepository;

    public DataExportChildProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier,
        DataExportChildProvider previousStateIfKnown) : base(repositoryLocator.CatalogueRepository,
        pluginChildProviders, errorsCheckNotifier, previousStateIfKnown)
    {
        ForbidListedSources = previousStateIfKnown?.ForbidListedSources ?? new List<ExternalCohortTable>();
        _errorsCheckNotifier = errorsCheckNotifier;
        dataExportRepository = repositoryLocator.DataExportRepository;

        AllProjectAssociatedCics =
            GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>(dataExportRepository);

        _cicAssociations =
            new HashSet<int>(AllProjectAssociatedCics.Select(a => a.CohortIdentificationConfiguration_ID));

        CohortSources = GetAllObjects<ExternalCohortTable>(dataExportRepository);
        ExtractableDataSets = GetAllObjects<ExtractableDataSet>(dataExportRepository);
        ExtractableDataSetProjects = GetAllObjects<ExtractableDataSetProject>(dataExportRepository);
        //This means that the ToString method in ExtractableDataSet doesn't need to go lookup catalogue info
        var catalogueIdDict = AllCatalogues.ToDictionaryEx(c => c.ID, c2 => c2);
        foreach (var ds in ExtractableDataSets)
            if (catalogueIdDict.TryGetValue(ds.Catalogue_ID, out var cata))
                ds.InjectKnown(cata);

        ReportProgress("Injecting ExtractableDataSet");

        AllPackages = GetAllObjects<ExtractableDataSetPackage>(dataExportRepository);

        Projects = GetAllObjects<Project>(dataExportRepository);
        ExtractionConfigurations = GetAllObjects<ExtractionConfiguration>(dataExportRepository);

        ReportProgress("Get Projects and Configurations");

        ExtractionConfigurationsByProject = ExtractionConfigurations.GroupBy(k => k.Project_ID)
            .ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList());

        ReportProgress("Grouping Extractions by Project");

        BuildSelectedDatasets();

        AllGlobalExtractionFilterParameters = GetAllObjects<GlobalExtractionFilterParameter>(dataExportRepository);

        BuildExtractionFilters();

        ReportProgress("Building FilterManager");

        Cohorts = GetAllObjects<ExtractableCohort>(dataExportRepository);
        _cohortsByOriginId = new Dictionary<int, HashSet<ExtractableCohort>>();

        foreach (var c in Cohorts)
        {
            if (!_cohortsByOriginId.ContainsKey(c.OriginID))
                _cohortsByOriginId.Add(c.OriginID, new HashSet<ExtractableCohort>());

            _cohortsByOriginId[c.OriginID].Add(c);
        }


        ReportProgress("Fetching Cohorts");

        GetCohortAvailability();

        ReportProgress("GetCohortAvailability");


        ReportProgress("Mapping configurations to datasets");

        RootCohortsNode = new AllCohortsNode();
        AddChildren(RootCohortsNode, new DescendancyList(RootCohortsNode));

        foreach (var package in AllPackages)
            AddChildren(package, new DescendancyList(package));

        ReportProgress("Packages and Cohorts");

        ProjectRootFolder = FolderHelper.BuildFolderTree(Projects);
        AddChildren(ProjectRootFolder, new DescendancyList(ProjectRootFolder));

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

        try
        {
            AddPipelineUseCases(new Dictionary<string, PipelineUseCase>
            {
                { "File Import", UploadFileUseCase.DesignTime() },
                { "Extraction", ExtractionPipelineUseCase.DesignTime() },
                { "Release", ReleaseUseCase.DesignTime() },
                { "Cohort Creation", CohortCreationRequest.DesignTime() },
                { "Caching", CachingPipelineUseCase.DesignTime() },
                {
                    "Aggregate Committing",
                    CreateTableFromAggregateUseCase.DesignTime(repositoryLocator.CatalogueRepository)
                }
            });
        }
        catch (Exception ex)
        {
            _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to build DesignTime PipelineUseCases",
                CheckResult.Fail, ex));
        }

        ReportProgress("Pipeline adding");

        GetPluginChildren();
    }


    private void AddChildren(FolderNode<Project> folder, DescendancyList descendancy)
    {
        foreach (var child in folder.ChildFolders)
            //add subfolder children
            AddChildren(child, descendancy.Add(child));

        //add catalogues in folder
        foreach (var project in folder.ChildObjects) AddChildren(project, descendancy.Add(project));

        // Children are the folders + objects
        AddToDictionaries(new HashSet<object>(
                folder.ChildFolders.Cast<object>()
                    .Union(folder.ChildObjects)), descendancy
        );
    }

    private void BuildSelectedDatasets()
    {
        _selectedDataSetsWithNoIsExtractionIdentifier =
            new HashSet<ISelectedDataSets>(dataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers());

        SelectedDataSets = GetAllObjects<SelectedDataSets>(dataExportRepository);
        ReportProgress("Fetching data export objects");

        _extractionProgressesBySelectedDataSetID = GetAllObjects<ExtractionProgress>(dataExportRepository)
            .ToDictionaryEx(ds => ds.SelectedDataSets_ID, d => d);

        var dsDictionary = ExtractableDataSets.ToDictionaryEx(ds => ds.ID, d => d);
        foreach (var s in SelectedDataSets)
            s.InjectKnown(dsDictionary[s.ExtractableDataSet_ID]);

        ReportProgress("Injecting SelectedDataSets");

        _configurationToDatasetMapping = new Dictionary<IExtractionConfiguration, List<SelectedDataSets>>();

        var configToSds = SelectedDataSets.GroupBy(k => k.ExtractionConfiguration_ID)
            .ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList());

        foreach (var configuration in ExtractionConfigurations)
            if (configToSds.TryGetValue(configuration.ID, out var result))
                _configurationToDatasetMapping.Add(configuration, result);
    }

    private void BuildExtractionFilters()
    {
        AllContainers = GetAllObjects<FilterContainer>(dataExportRepository).ToDictionaryEx(o => o.ID, o => o);
        AllDeployedExtractionFilters = GetAllObjects<DeployedExtractionFilter>(dataExportRepository);
        _allParameters = GetAllObjects<DeployedExtractionFilterParameter>(dataExportRepository);

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

        AddToDictionaries(children, descendancy);
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
        AddToDictionaries(children, descendancy);
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
                        AddChildren(cata, descendancy.Add(eds.Catalogue));
                    }
                }
            }
        }

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
    }

    private void AddChildren(CommittedCohortIdentificationNode associatedCohortConfigurations, DescendancyList descendancy)
    {
        var children = new HashSet<object>();
        var associatedCohorts = associatedCohortConfigurations.Project.GetAssociatedCohortIdentificationConfigurations();
        foreach (var cohort in associatedCohorts)
        {
            children.Add(cohort);
        }

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
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

        AddToDictionaries(children, descendancy);
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


        AddToDictionaries(children, descendancy);
    }

    private void AddChildren(DeployedExtractionFilter filter, DescendancyList descendancyList)
    {
        AddToDictionaries(new HashSet<object>(_allParameters.Where(p => p.ExtractionFilter_ID == filter.ID)),
            descendancyList);
    }

    private void AddChildren(CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode, DescendancyList descendancy)
    {
        AddToDictionaries(new HashSet<object>(cohortSourceUsedByProjectNode.CohortsUsed), descendancy);
    }

    private void AddChildren(AllCohortsNode cohortsNode, DescendancyList descendancy)
    {
        var validSources = CohortSources.ToArray();

        AddToDictionaries(new HashSet<object>(validSources), descendancy);
        foreach (var s in validSources)
            AddChildren(s, descendancy.Add(s));
    }

    private void AddChildren(ExternalCohortTable externalCohortTable, DescendancyList descendancy)
    {
        var cohorts = Cohorts.Where(c => c.ExternalCohortTable_ID == externalCohortTable.ID).ToArray();

        foreach (var cohort in cohorts)
            cohort.InjectKnown(externalCohortTable);

        AddToDictionaries(new HashSet<object>(cohorts), descendancy);
    }

    private void GetCohortAvailability()
    {
        Parallel.ForEach(CohortSources.Except(ForbidListedSources), GetCohortAvailability);
    }

    private void GetCohortAvailability(ExternalCohortTable source)
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
                            if (!ProjectNumberToCohortsDictionary.ContainsKey(externalData.ExternalProjectNumber))
                                ProjectNumberToCohortsDictionary.Add(externalData.ExternalProjectNumber,
                                    new List<ExtractableCohort>());

                            ProjectNumberToCohortsDictionary[externalData.ExternalProjectNumber].Add(c);
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
            //Get the extraction configurations node of the project
            var configurationsNode = GetChildren(project).OfType<ExtractionConfigurationsNode>().Single();

            var frozenConfigurationsNode =
                GetChildren(configurationsNode).OfType<FrozenExtractionConfigurationsNode>().Single();

            //then add all the children extraction configurations
            return GetChildren(configurationsNode).OfType<ExtractionConfiguration>()
                .Union(GetChildren(frozenConfigurationsNode).OfType<ExtractionConfiguration>());
        }
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

    public override void UpdateTo(ICoreChildProvider other)
    {
        lock (WriteLock)
        {
            base.UpdateTo(other);

            if (other is not DataExportChildProvider dxOther)
                throw new NotSupportedException(
                    $"Did not know how to UpdateTo ICoreChildProvider of type {other.GetType().Name}");

            //That's one way to avoid memory leaks... anyone holding onto a stale one of these is going to have a bad day
            RootCohortsNode = dxOther.RootCohortsNode;
            CohortSources = dxOther.CohortSources;
            ExtractableDataSets = dxOther.ExtractableDataSets;
            SelectedDataSets = dxOther.SelectedDataSets;
            AllPackages = dxOther.AllPackages;
            Projects = dxOther.Projects;
            _cohortsByOriginId = dxOther._cohortsByOriginId;
            Cohorts = dxOther.Cohorts;
            ExtractionConfigurations = dxOther.ExtractionConfigurations;
            ExtractionConfigurationsByProject = dxOther.ExtractionConfigurationsByProject;
            _configurationToDatasetMapping = dxOther._configurationToDatasetMapping;
            _dataExportFilterManager = dxOther._dataExportFilterManager;
            ForbidListedSources = dxOther.ForbidListedSources;
            DuplicatesByProject = dxOther.DuplicatesByProject;
            DuplicatesByCohortSourceUsedByProjectNode = dxOther.DuplicatesByCohortSourceUsedByProjectNode;
            ProjectNumberToCohortsDictionary = dxOther.ProjectNumberToCohortsDictionary;
            AllProjectAssociatedCics = dxOther.AllProjectAssociatedCics;
            AllGlobalExtractionFilterParameters = dxOther.AllGlobalExtractionFilterParameters;
            _cicAssociations = dxOther._cicAssociations;
            _selectedDataSetsWithNoIsExtractionIdentifier = dxOther._selectedDataSetsWithNoIsExtractionIdentifier;
            AllContainers = dxOther.AllContainers;
            AllDeployedExtractionFilters = dxOther.AllDeployedExtractionFilters;
            _allParameters = dxOther._allParameters;
            dataExportRepository = dxOther.dataExportRepository;
            _extractionInformationsByCatalogueItem = dxOther._extractionInformationsByCatalogueItem;
            _extractionProgressesBySelectedDataSetID = dxOther._extractionProgressesBySelectedDataSetID;
            ProjectRootFolder = dxOther.ProjectRootFolder;
            DatasetRootFolder = dxOther.DatasetRootFolder;
            AllDatasetsNode = dxOther.AllDatasetsNode;
            AllRegexRedactionConfigurations = dxOther.AllRegexRedactionConfigurations;
            AllRegexRedactionConfigurationsNode = dxOther.AllRegexRedactionConfigurationsNode;
        }
    }

    public override bool SelectiveRefresh(IMapsDirectlyToDatabaseTable databaseEntity)
    {
        ProgressStopwatch.Restart();

        return databaseEntity switch
        {
            DeployedExtractionFilterParameter defp => SelectiveRefresh(defp.ExtractionFilter),
            DeployedExtractionFilter def => SelectiveRefresh(def),
            FilterContainer fc => SelectiveRefresh(fc),
            SelectedDataSets sds => SelectiveRefresh(sds),
            IExtractionConfiguration ec => SelectiveRefresh(ec),
            _ => base.SelectiveRefresh(databaseEntity)
        };
    }

    private bool SelectiveRefresh(DeployedExtractionFilter f)
    {
        var knownContainer = GetDescendancyListIfAnyFor(f.FilterContainer);
        if (knownContainer == null) return false;

        BuildExtractionFilters();
        AddChildren((FilterContainer)f.FilterContainer, knownContainer.Add(f.FilterContainer));
        return true;
    }

    public bool SelectiveRefresh(FilterContainer container)
    {
        var descendancy = GetDescendancyListIfAnyFor(container);

        if (descendancy == null)
            return false;

        var sds = descendancy.Parents.OfType<SelectedDataSets>().Last();

        descendancy = GetDescendancyListIfAnyFor(sds);

        if (descendancy != null)
        {
            // update it to the latest state (e.g. if a root filter container is being added)
            sds.RevertToDatabaseState();

            BuildExtractionFilters();

            // rebuild descendency from here
            AddChildren(sds, descendancy.Add(sds));
            return true;
        }


        return false;
    }

    public bool SelectiveRefresh(SelectedDataSets sds)
    {
        var ec = sds.ExtractionConfiguration;
        var descendancy = GetDescendancyListIfAnyFor(ec);

        if (descendancy != null)
        {
            // update it to the latest state (e.g. if a root filter container is being added)
            ec.RevertToDatabaseState();

            BuildExtractionFilters();

            BuildSelectedDatasets();

            // rebuild descendency from here
            AddChildren(ec, descendancy.Add(ec));
            return true;
        }

        return false;
    }

    public bool SelectiveRefresh(IExtractionConfiguration ec)
    {
        // don't try to selectively refresh when deleting
        if (!ec.Exists())
            return false;

        var project = ec.Project;
        // update it to the latest state
        project.RevertToDatabaseState();

        AllGlobalExtractionFilterParameters = GetAllObjects<GlobalExtractionFilterParameter>(dataExportRepository);

        BuildSelectedDatasets();

        // rebuild descendency from here
        AddChildren((Project)project, new DescendancyList(project));
        return true;
    }
}
