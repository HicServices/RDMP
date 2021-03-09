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
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Providers.Nodes.UsedByNodes;
using Rdmp.Core.Providers.Nodes.UsedByProject;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.Repositories.Managers.HighPerformance;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Providers
{
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

        public SelectedDataSets[] SelectedDataSets { get; private set; }

        public ExtractableDataSetPackage[] AllPackages { get; set; }
        
        public Project[] Projects { get; set; }

        private Dictionary<int,HashSet<ExtractableCohort>> _cohortsByOriginId;
        public ExtractableCohort[] Cohorts { get; private set; }

        public ExtractionConfiguration[] ExtractionConfigurations { get; private set; }
        public Dictionary<int, List<ExtractionConfiguration>> ExtractionConfigurationsByProject { get; set; }

        private Dictionary<ExtractionConfiguration, List<SelectedDataSets>> _configurationToDatasetMapping;
        private IFilterManager _dataExportFilterManager;

        public List<ExternalCohortTable> BlackListedSources { get; private set; }
        
        public List<IObjectUsedByOtherObjectNode<Project,IMapsDirectlyToDatabaseTable>> DuplicatesByProject = new List<IObjectUsedByOtherObjectNode<Project,IMapsDirectlyToDatabaseTable>>();
        public List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>> DuplicatesByCohortSourceUsedByProjectNode = new List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>>();
        
        private readonly object _oProjectNumberToCohortsDictionary = new object();
        public Dictionary<int,List<ExtractableCohort>> ProjectNumberToCohortsDictionary = new Dictionary<int, List<ExtractableCohort>>();

        public ProjectCohortIdentificationConfigurationAssociation[] AllProjectAssociatedCics;

        public GlobalExtractionFilterParameter[] AllGlobalExtractionFilterParameters;
        
        /// <summary>
        /// ID of all CohortIdentificationConfiguration which have an ProjectCohortIdentificationConfigurationAssociation declared on them (i.e. the CIC is used with one or more Projects)
        /// </summary>
        private HashSet<int> _cicAssociations;

        public AllFreeCohortIdentificationConfigurationsNode AllFreeCohortIdentificationConfigurationsNode = new AllFreeCohortIdentificationConfigurationsNode();
        public AllProjectCohortIdentificationConfigurationsNode AllProjectCohortIdentificationConfigurationsNode = new AllProjectCohortIdentificationConfigurationsNode();
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

        public DataExportChildProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IChildProvider[] pluginChildProviders,ICheckNotifier errorsCheckNotifier, DataExportChildProvider previousStateIfKnown) : base(repositoryLocator.CatalogueRepository, pluginChildProviders,errorsCheckNotifier,previousStateIfKnown)
        {
            BlackListedSources = previousStateIfKnown?.BlackListedSources ?? new List<ExternalCohortTable>();
            _errorsCheckNotifier = errorsCheckNotifier;
            dataExportRepository = repositoryLocator.DataExportRepository;

            AllProjectAssociatedCics = GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>(dataExportRepository);

            _cicAssociations = new HashSet<int>(AllProjectAssociatedCics.Select(a => a.CohortIdentificationConfiguration_ID));

            CohortSources = GetAllObjects<ExternalCohortTable>(dataExportRepository);
            ExtractableDataSets = GetAllObjects<ExtractableDataSet>(dataExportRepository);
            
            AddToDictionaries(new HashSet<object>(AllCohortIdentificationConfigurations.Where(cic => _cicAssociations.Contains(cic.ID))), new DescendancyList(AllProjectCohortIdentificationConfigurationsNode));
            AddToDictionaries(new HashSet<object>(AllCohortIdentificationConfigurations.Where(cic => !_cicAssociations.Contains(cic.ID))), new DescendancyList(AllFreeCohortIdentificationConfigurationsNode));

            _selectedDataSetsWithNoIsExtractionIdentifier = new HashSet<ISelectedDataSets>(dataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers());

            SelectedDataSets = GetAllObjects<SelectedDataSets>(dataExportRepository);
            ReportProgress("Fetching data export objects");

            var dsDictionary = ExtractableDataSets.ToDictionary(ds => ds.ID, d => d);
            foreach (SelectedDataSets s in SelectedDataSets)
                s.InjectKnown(dsDictionary[s.ExtractableDataSet_ID]);
            
            ReportProgress("Injecting SelectedDataSets");

            //This means that the ToString method in ExtractableDataSet doesn't need to go lookup catalogue info
            var catalogueIdDict = AllCatalogues.ToDictionary(c => c.ID, c2 => c2);
            foreach (ExtractableDataSet ds in ExtractableDataSets)
                if(catalogueIdDict.TryGetValue(ds.Catalogue_ID, out Catalogue cata))
                    ds.InjectKnown(cata);
             
            ReportProgress("Injecting ExtractableDataSet");
            
            AllPackages = GetAllObjects<ExtractableDataSetPackage>(dataExportRepository);
            
            Projects = GetAllObjects<Project>(dataExportRepository);
            ExtractionConfigurations = GetAllObjects<ExtractionConfiguration>(dataExportRepository);
                        
            ReportProgress("Get Projects and Configurations");

            ExtractionConfigurationsByProject = ExtractionConfigurations.GroupBy(k => k.Project_ID).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            ReportProgress("Grouping Extractions by Project");

            AllGlobalExtractionFilterParameters = GetAllObjects<GlobalExtractionFilterParameter>(dataExportRepository);

            AllContainers = GetAllObjects<FilterContainer>(dataExportRepository).ToDictionary(o => o.ID, o => o);
            AllDeployedExtractionFilters = GetAllObjects<DeployedExtractionFilter>(dataExportRepository);
            _allParameters = GetAllObjects<DeployedExtractionFilterParameter>(dataExportRepository);
            
            ReportProgress("Getting Filters");

            //if we are using a database repository then we can make use of the caching class DataExportFilterManagerFromChildProvider to speed up
            //filter contents
            var dbRepo = dataExportRepository as DataExportRepository;
            _dataExportFilterManager = dbRepo == null ? dataExportRepository.FilterManager : new DataExportFilterManagerFromChildProvider(dbRepo, this);
                        
            ReportProgress("Building FilterManager");

            Cohorts = GetAllObjects<ExtractableCohort>(dataExportRepository);
            _cohortsByOriginId = new Dictionary<int,HashSet<ExtractableCohort>>();

            foreach (ExtractableCohort c in Cohorts)
            {
                if(!_cohortsByOriginId.ContainsKey(c.OriginID))
                    _cohortsByOriginId.Add(c.OriginID,new HashSet<ExtractableCohort>());

                _cohortsByOriginId[c.OriginID].Add(c);
            }

            _configurationToDatasetMapping = new Dictionary<ExtractionConfiguration, List<SelectedDataSets>>();
            
            ReportProgress("Fetching Cohorts");

            GetCohortAvailability();

            ReportProgress("GetCohortAvailability");
            
            var configToSds = SelectedDataSets.GroupBy(k => k.ExtractionConfiguration_ID).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            
            foreach (ExtractionConfiguration configuration in ExtractionConfigurations)
                if(configToSds.TryGetValue(configuration.ID, out List<SelectedDataSets> result))
                    _configurationToDatasetMapping.Add(configuration,result);
            
            ReportProgress("Mapping configurations to datasets");

            RootCohortsNode = new AllCohortsNode();
            AddChildren(RootCohortsNode,new DescendancyList(RootCohortsNode));

            foreach (ExtractableDataSetPackage package in AllPackages)
                AddChildren(package, new DescendancyList(package));
            
            ReportProgress("Packages and Cohorts");

            foreach (Project p in Projects)
                AddChildren(p, new DescendancyList(p));
            
            ReportProgress("Projects");

            //work out all the Catalogues that are extractable (Catalogues are extractable if there is an ExtractableDataSet with the Catalogue_ID that matches them)
            var cataToEds = new Dictionary<int,ExtractableDataSet>(ExtractableDataSets.ToDictionary(k => k.Catalogue_ID));

            //inject extractability into Catalogues
            foreach (Catalogue catalogue in AllCatalogues)
                if (cataToEds.TryGetValue(catalogue.ID, out ExtractableDataSet result))
                    catalogue.InjectKnown(result.GetCatalogueExtractabilityStatus());
                else
                    catalogue.InjectKnown(new CatalogueExtractabilityStatus(false,false));
            
            ReportProgress("Catalogue extractability injection");

            try
            {
                AddPipelineUseCases(new Dictionary<string, PipelineUseCase>
                {
                    {"File Import", UploadFileUseCase.DesignTime()},
                    {"Extraction",ExtractionPipelineUseCase.DesignTime()},
                    {"Release",ReleaseUseCase.DesignTime()},
                    {"Cohort Creation",CohortCreationRequest.DesignTime()},
                    {"Caching",CachingPipelineUseCase.DesignTime()},
                    {"Aggregate Committing",CreateTableFromAggregateUseCase.DesignTime(repositoryLocator.CatalogueRepository)}
                });
            }
            catch (Exception ex)
            {
                _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to build DesignTime PipelineUseCases",CheckResult.Fail,ex));
            }

            ReportProgress("Pipeline adding");
        }

        private void AddChildren(IExtractableDataSetPackage package, DescendancyList descendancy)
        {
            var children = new HashSet<object>(dataExportRepository.PackageManager.GetAllDataSets(package, ExtractableDataSets)
                .Select(ds => new PackageContentNode(package, ds, dataExportRepository.PackageManager)));

            AddToDictionaries(children, descendancy);

        }
        
        private void AddChildren(Project project, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            var projectCohortNode = new ProjectCohortsNode(project);
            children.Add(projectCohortNode);
            AddChildren(projectCohortNode,descendancy.Add(projectCohortNode));

            var projectCataloguesNode = new ProjectCataloguesNode(project);
            children.Add(projectCataloguesNode);
            AddChildren(projectCataloguesNode, descendancy.Add(projectCataloguesNode).SetNewBestRoute());

            var extractionConfigurationsNode = new ExtractionConfigurationsNode(project);
            children.Add(extractionConfigurationsNode);

            AddChildren(extractionConfigurationsNode,descendancy.Add(extractionConfigurationsNode));
            
            var folder = new ExtractionDirectoryNode(project);
            children.Add(folder);
            AddToDictionaries(children,descendancy);
        }

        private void AddChildren(ProjectCataloguesNode projectCataloguesNode, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            foreach (ExtractableDataSet projectSpecificEds in ExtractableDataSets.Where(eds=>eds.Project_ID == projectCataloguesNode.Project.ID))
            {
                
                var cata = (Catalogue)projectSpecificEds.Catalogue;
                
                // cata will be null if it has been deleted from the database
                if(cata != null)
                {
                    children.Add(cata);
                    AddChildren(cata, descendancy.Add(projectSpecificEds.Catalogue));
                }
                    
            }
            
            AddToDictionaries(children, descendancy);
        }

        private void AddChildren(ProjectCohortsNode projectCohortsNode, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();
            var projectCiCsNode = new ProjectCohortIdentificationConfigurationAssociationsNode(projectCohortsNode.Project);
            children.Add(projectCiCsNode);
            AddChildren(projectCiCsNode, descendancy.Add(projectCiCsNode));

            var savedCohortsNode = new ProjectSavedCohortsNode(projectCohortsNode.Project);
            children.Add(savedCohortsNode);
            AddChildren(savedCohortsNode, descendancy.Add(savedCohortsNode));

            AddToDictionaries(children, descendancy);
        }

        private void AddChildren(ProjectSavedCohortsNode savedCohortsNode, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            var cohortGroups = GetAllCohortProjectUsageNodesFor(savedCohortsNode.Project);

            foreach (CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode in cohortGroups)
            {
                AddChildren(cohortSourceUsedByProjectNode, descendancy.Add(cohortSourceUsedByProjectNode));
                children.Add(cohortSourceUsedByProjectNode);
            }

            AddToDictionaries(children, descendancy);
        }

        private void AddChildren(ProjectCohortIdentificationConfigurationAssociationsNode projectCiCsNode, DescendancyList descendancy)
        {
            //add the associations
            HashSet<object> children = new HashSet<object>();
            foreach (ProjectCohortIdentificationConfigurationAssociation association in AllProjectAssociatedCics.Where(assoc => assoc.Project_ID == projectCiCsNode.Project.ID))
            {

                var matchingCic = AllCohortIdentificationConfigurations.SingleOrDefault(cic => cic.ID == association.CohortIdentificationConfiguration_ID);

                if (matchingCic == null)
                    _errorsCheckNotifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Failed to find Associated Cohort Identification Configuration with ID " +
                            association.CohortIdentificationConfiguration_ID +
                            " which was supposed to be associated with " + association.Project, CheckResult.Fail));//inject knowledge of what the cic is so it doesn't have to be fetched during ToString
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
            HashSet<object> children = new HashSet<object>();

            //Create a frozen extraction configurations folder as a subfolder of each ExtractionConfigurationsNode
            var frozenConfigurationsNode = new FrozenExtractionConfigurationsNode(extractionConfigurationsNode.Project);

            //Make the frozen folder appear under the extractionConfigurationsNode
            children.Add(frozenConfigurationsNode);

            //Add children to the frozen folder
            AddChildren(frozenConfigurationsNode,descendancy.Add(frozenConfigurationsNode));

            //Add ExtractionConfigurations which are not released (frozen)
            if(ExtractionConfigurationsByProject.TryGetValue(extractionConfigurationsNode.Project.ID, out List<ExtractionConfiguration> result))
                foreach (ExtractionConfiguration config in result.Where(c=>!c.IsReleased))
                {
                    AddChildren(config, descendancy.Add(config));
                    children.Add(config);
                }

            AddToDictionaries(children, descendancy);
        }

        private void AddChildren(FrozenExtractionConfigurationsNode frozenExtractionConfigurationsNode, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            //Add ExtractionConfigurations which are not released (frozen)
            if(ExtractionConfigurationsByProject.TryGetValue(frozenExtractionConfigurationsNode.Project.ID, out List<ExtractionConfiguration> result))
                foreach (ExtractionConfiguration config in result.Where(c => c.IsReleased))
                {
                    AddChildren(config, descendancy.Add(config));
                    children.Add(config);
                }

            AddToDictionaries(children,descendancy);
        }

        private void AddChildren(ExtractionConfiguration extractionConfiguration, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            var parameters = AllGlobalExtractionFilterParameters.Where(p => p.ExtractionConfiguration_ID == extractionConfiguration.ID)
                .ToArray();

            if (parameters.Any())
            {
                var parameterNode = new ParametersNode(extractionConfiguration, parameters);
                children.Add(parameterNode);
            }

            //if it has a cohort
            if (extractionConfiguration.Cohort_ID != null)
            {
                var cohort = Cohorts.Single(c => c.ID == extractionConfiguration.Cohort_ID);
                children.Add(new LinkedCohortNode(extractionConfiguration, cohort));
            }

            //if it has extractable datasets add those
            foreach (SelectedDataSets ds in GetDatasets(extractionConfiguration))
            {
                children.Add(ds);
                AddChildren(ds,descendancy.Add(ds));
            }
            
            AddToDictionaries(children,descendancy);
            
        }

        private void AddChildren(SelectedDataSets selectedDataSets, DescendancyList descendancy)
        {
            if (selectedDataSets.RootFilterContainer_ID != null)
            {
                var rootContainer = AllContainers[selectedDataSets.RootFilterContainer_ID.Value];
                AddChildren(rootContainer,descendancy.Add(rootContainer));
                AddToDictionaries(new HashSet<object>(new object[]{rootContainer}),descendancy);
            }
        }
        

        private void AddChildren(FilterContainer filterContainer, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            foreach (FilterContainer subcontainer in _dataExportFilterManager.GetSubContainers(filterContainer))
            {
                AddChildren(subcontainer,descendancy.Add(subcontainer));
                children.Add(subcontainer);
            }

            foreach (var filter in _dataExportFilterManager.GetFilters(filterContainer))
                children.Add(filter);

            AddToDictionaries(children,descendancy);
        }


        private void AddChildren(CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode, DescendancyList descendancy)
        {
            AddToDictionaries(new HashSet<object>(cohortSourceUsedByProjectNode.CohortsUsed),descendancy);
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
            
            foreach (ExtractableCohort cohort in cohorts)
                cohort.InjectKnown(externalCohortTable);

            AddToDictionaries(new HashSet<object>(cohorts), descendancy);
        }

        private void GetCohortAvailability()
        {
            Parallel.ForEach(CohortSources.Except(BlackListedSources), GetCohortAvailability);
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
                    server = DataAccessPortal.GetInstance().ExpectDatabase(source, DataAccessContext.DataExport).Server;
                }
                catch (Exception exception)
                {
                    ex = exception;
                }

                if (server == null || !server.RespondsWithinTime(3, out ex) || !source.IsFullyPopulated())
                {
                    Blacklist(source,ex);
                    return;
                }

                try
                {
                    using (var con = server.GetConnection())
                    {
                        con.Open();
                    
                        //Get all of the project numbers and remote origin ids etc from the source in one query
                        using (var cmd = server.GetCommand(source.GetExternalDataSql(), con))
                        {
                            cmd.CommandTimeout = 120;

                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    //really should be only one here but still they might for some reason have 2 references to the same external cohort
                            
                                    if(_cohortsByOriginId.TryGetValue(Convert.ToInt32(r["OriginID"]),out HashSet<ExtractableCohort> result))
                                        //Tell the cohorts what their external data values are so they don't have to fetch them themselves individually
                                        foreach (ExtractableCohort c in result.Where(c=> c.ExternalCohortTable_ID == source.ID))
                                        {
                                            //load external data from the result set
                                            var externalData = new ExternalCohortDefinitionData(r, source.Name);

                                            //tell the cohort about the data
                                            c.InjectKnown(externalData);
                                        
                                            lock (_oProjectNumberToCohortsDictionary)
                                            {
                                                //for performance also keep a dictionary of project number => compatible cohorts
                                                if (!ProjectNumberToCohortsDictionary.ContainsKey(externalData.ExternalProjectNumber))
                                                    ProjectNumberToCohortsDictionary.Add(externalData.ExternalProjectNumber, new List<ExtractableCohort>());

                                                ProjectNumberToCohortsDictionary[externalData.ExternalProjectNumber].Add(c);
                                            }
                                        }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Blacklist(source,e);
                }
        }

        private void Blacklist(ExternalCohortTable source,Exception ex)
        {
            BlackListedSources.Add(source);

            _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs("Could not reach cohort '" + source + "' (it may be slow responding or inaccessible due to user permissions)", CheckResult.Warning, ex));

            //tell them not to bother looking for the cohort data because its inaccessible
            foreach (ExtractableCohort cohort in Cohorts.Where(c => c.ExternalCohortTable_ID == source.ID).ToArray())
                cohort.InjectKnown((ExternalCohortDefinitionData)null);
        }

        private List<CohortSourceUsedByProjectNode> GetAllCohortProjectUsageNodesFor(Project project)
        {
            //if the current project does not have a number or there are no cohorts associated with it
            if (project.ProjectNumber == null || !ProjectNumberToCohortsDictionary.ContainsKey(project.ProjectNumber.Value))
                return new List<CohortSourceUsedByProjectNode>();

            List<CohortSourceUsedByProjectNode> toReturn = new List<CohortSourceUsedByProjectNode>();
            
            
            foreach (var cohort in ProjectNumberToCohortsDictionary[project.ProjectNumber.Value])
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
                var cohortUsedByProject = new ObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode,ExtractableCohort>(existing,cohort);
                existing.CohortsUsed.Add(cohortUsedByProject);

                DuplicatesByCohortSourceUsedByProjectNode.Add(cohortUsedByProject);
            }

            DuplicatesByProject.AddRange(toReturn);

            //if the project has no cohorts then add a ??? node 
            if(!toReturn.Any())
                toReturn.Add(new CohortSourceUsedByProjectNode(project,null));

            return toReturn;
        }

        public IEnumerable<ExtractionConfiguration> GetActiveConfigurationsOnly(Project project)
        {
            lock(WriteLock)
            {
                return GetConfigurations(project).Where(ec => !ec.IsReleased);
            }
        }

        public IEnumerable<SelectedDataSets> GetDatasets(ExtractionConfiguration extractionConfiguration)
        {
            lock(WriteLock)
            {
                return _configurationToDatasetMapping.TryGetValue(extractionConfiguration,out List<SelectedDataSets> result)?
                (IEnumerable<SelectedDataSets>) result :new SelectedDataSets[0];
            }
        }

        public IEnumerable<ExtractionConfiguration> GetConfigurations(Project project)
        {
            lock(WriteLock)
            {
                //Get the extraction configurations node of the project
                var configurationsNode = GetChildren(project).OfType<ExtractionConfigurationsNode>().Single();

                var frozenConfigurationsNode = GetChildren(configurationsNode).OfType<FrozenExtractionConfigurationsNode>().Single();

                //then add all the children extraction configurations
                return GetChildren(configurationsNode).OfType<ExtractionConfiguration>().Union(GetChildren(frozenConfigurationsNode).OfType<ExtractionConfiguration>());
            }
        }

        public IEnumerable<IExtractableDataSet> GetDatasets(ExtractableDataSetPackage package)
        {
            lock(WriteLock)
            {
                return dataExportRepository.PackageManager.GetAllDataSets(package, ExtractableDataSets);
            }
        }
        
        public bool ProjectHasNoSavedCohorts(Project project)
        {
            lock(WriteLock)
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
            lock(WriteLock)
            {
                var toReturn = base.GetAllSearchables();
                AddToReturnSearchablesWithNoDecendancy(toReturn,Projects);
                AddToReturnSearchablesWithNoDecendancy(toReturn, AllPackages);
                return toReturn;
            }
            
        }

        public bool IsMissingExtractionIdentifier(SelectedDataSets selectedDataSets)
        {
            lock(WriteLock)
            {
                return _selectedDataSetsWithNoIsExtractionIdentifier.Contains(selectedDataSets);
            }
        }

        /// <summary>
        /// Returns all <see cref="ExtractableColumn"/> Injected with thier corresponding <see cref="ExtractionInformation"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public ExtractableColumn[] GetAllExtractableColumns(IDataExportRepository repository)
        {
            lock(WriteLock)
            {
                var toReturn = repository.GetAllObjects<ExtractableColumn>();
                foreach (var c in toReturn)
                {
                    if (c.CatalogueExtractionInformation_ID == null)
                        c.InjectKnown((ExtractionInformation)null);
                    else
                    {
                        if (AllExtractionInformationsDictionary.TryGetValue(c.CatalogueExtractionInformation_ID.Value,out ExtractionInformation ei))
                            c.InjectKnown(ei);
                    }
                }

                return toReturn;
            }
        }

        public override void UpdateTo(ICoreChildProvider other)
        {
            lock(WriteLock)
            {
                base.UpdateTo(other);

                if(!(other is DataExportChildProvider dxOther))
                {
                    throw new NotSupportedException("Did not know how to UpdateTo ICoreChildProvider of type " + other.GetType().Name);
                }

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
                BlackListedSources = dxOther.BlackListedSources;
                DuplicatesByProject = dxOther.DuplicatesByProject;
                DuplicatesByCohortSourceUsedByProjectNode = dxOther.DuplicatesByCohortSourceUsedByProjectNode;
                ProjectNumberToCohortsDictionary = dxOther.ProjectNumberToCohortsDictionary;
                AllProjectAssociatedCics = dxOther.AllProjectAssociatedCics;
                AllGlobalExtractionFilterParameters = dxOther.AllGlobalExtractionFilterParameters;
                _cicAssociations = dxOther._cicAssociations;
                AllFreeCohortIdentificationConfigurationsNode = dxOther.AllFreeCohortIdentificationConfigurationsNode;
                AllProjectCohortIdentificationConfigurationsNode = dxOther.AllProjectCohortIdentificationConfigurationsNode;
                _selectedDataSetsWithNoIsExtractionIdentifier = dxOther._selectedDataSetsWithNoIsExtractionIdentifier;
                AllContainers = dxOther.AllContainers;
                AllDeployedExtractionFilters = dxOther.AllDeployedExtractionFilters;
                _allParameters = dxOther._allParameters;
                dataExportRepository = dxOther.dataExportRepository;
                _extractionInformationsByCatalogueItem = dxOther._extractionInformationsByCatalogueItem;
            }
            
        }
    }
}
