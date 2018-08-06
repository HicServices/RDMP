using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.CohortCreationPipeline.UseCases;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Data.Hierarchy;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.Providers.Nodes;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportLibrary.Providers.Nodes.UsedByNodes;
using DataExportLibrary.Providers.Nodes.UsedByProject;
using DataLoadEngine.PipelineUseCases;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataExportLibrary.Providers
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
        public ExtractableDataSetPackageContents PackageContents { get; set; }

        public Project[] Projects { get; set; }

        public ExtractableCohort[] Cohorts { get; private set; }

        public ExtractionConfiguration[] ExtractionConfigurations { get; private set; }
        
        private Dictionary<ExtractionConfiguration, SelectedDataSets[]> _configurationToDatasetMapping;
        private DataExportFilterHierarchy _dataExportFilterHierarchy;

        private static List<ExternalCohortTable> BlackListedSources = new List<ExternalCohortTable>();
        
        public List<IObjectUsedByOtherObjectNode<Project,IMapsDirectlyToDatabaseTable>> DuplicatesByProject = new List<IObjectUsedByOtherObjectNode<Project,IMapsDirectlyToDatabaseTable>>();
        public List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>> DuplicatesByCohortSourceUsedByProjectNode = new List<IObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode>>();
        

        public Dictionary<int,List<ExtractableCohort>> ProjectNumberToCohortsDictionary = new Dictionary<int, List<ExtractableCohort>>();

        public ProjectCohortIdentificationConfigurationAssociation[] AllProjectAssociatedCics;

        public GlobalExtractionFilterParameter[] AllGlobalExtractionFilterParameters;

        /// <summary>
        /// All columns extracted in any dataset extracted in any ExtractionConfiguration.  Use GetColumnsIn to interrogate this in a more manageable way
        /// </summary>
        public Dictionary<int,List<ExtractableColumn>> ExtractionConfigurationToExtractableColumnsDictionary = new Dictionary<int, List<ExtractableColumn>>();

        public IEnumerable<ExtractableColumn> AllExtractableColumns {get { return ExtractionConfigurationToExtractableColumnsDictionary.SelectMany(kvp => kvp.Value); }}

        public DataExportChildProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IChildProvider[] pluginChildProviders,ICheckNotifier errorsCheckNotifier) : base(repositoryLocator.CatalogueRepository, pluginChildProviders,errorsCheckNotifier)
        {
            _errorsCheckNotifier = errorsCheckNotifier;
            var dataExportRepository = repositoryLocator.DataExportRepository;

            AllProjectAssociatedCics = dataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>();
            CohortSources = dataExportRepository.GetAllObjects<ExternalCohortTable>();
            ExtractableDataSets = dataExportRepository.GetAllObjects<ExtractableDataSet>();


            //record all extractable columns in each ExtractionConfiguration for fast reference later
            foreach (var c in dataExportRepository.GetAllObjects<ExtractableColumn>())
            {
                if (c.CatalogueExtractionInformation_ID == null)
                    c.InjectKnown((ExtractionInformation) null);
                else
                {
                    if (AllExtractionInformationsDictionary.ContainsKey(c.CatalogueExtractionInformation_ID.Value))
                    {
                        var extractionInformation = AllExtractionInformationsDictionary[c.CatalogueExtractionInformation_ID.Value];

                        c.InjectKnown(extractionInformation);
                    }
                }

                if(!ExtractionConfigurationToExtractableColumnsDictionary.ContainsKey(c.ExtractionConfiguration_ID))
                    ExtractionConfigurationToExtractableColumnsDictionary.Add(c.ExtractionConfiguration_ID,new List<ExtractableColumn>());   
                
                ExtractionConfigurationToExtractableColumnsDictionary[c.ExtractionConfiguration_ID].Add(c);
            }

            SelectedDataSets = dataExportRepository.GetAllObjects<SelectedDataSets>();
            var dsDictionary = ExtractableDataSets.ToDictionary(ds => ds.ID, d => d);
            foreach (SelectedDataSets s in SelectedDataSets)
                s.InjectKnown(dsDictionary[s.ExtractableDataSet_ID]);
            
            //This means that the ToString method in ExtractableDataSet doesn't need to go lookup catalogue info
            var dictionary = AllCatalogues.ToDictionary(c => c.ID, c2 => c2);
            foreach (ExtractableDataSet ds in ExtractableDataSets)
                if(dictionary.ContainsKey(ds.Catalogue_ID))
                    ds.InjectKnown(dictionary[ds.Catalogue_ID]);
                
            AllPackages = dataExportRepository.GetAllObjects<ExtractableDataSetPackage>();
            PackageContents = new ExtractableDataSetPackageContents(dataExportRepository);

            Projects = dataExportRepository.GetAllObjects<Project>();
            ExtractionConfigurations = dataExportRepository.GetAllObjects<ExtractionConfiguration>();
            AllGlobalExtractionFilterParameters = dataExportRepository.GetAllObjects<GlobalExtractionFilterParameter>();

            _dataExportFilterHierarchy = new DataExportFilterHierarchy(dataExportRepository);

            Cohorts = dataExportRepository.GetAllObjects<ExtractableCohort>();
            
            _configurationToDatasetMapping = new Dictionary<ExtractionConfiguration, SelectedDataSets[]>();

            GetCohortAvailability();

            foreach (ExtractionConfiguration configuration in ExtractionConfigurations)
                _configurationToDatasetMapping.Add(configuration, SelectedDataSets.Where(c => c.ExtractionConfiguration_ID == configuration.ID).ToArray());

            RootCohortsNode = new AllCohortsNode();
            AddChildren(RootCohortsNode,new DescendancyList(RootCohortsNode));

            foreach (ExtractableDataSetPackage package in AllPackages)
                AddChildren(package, new DescendancyList(package));

            foreach (Project p in Projects)
                AddChildren(p, new DescendancyList(p));

            //work out all the Catalogues that are extractable (Catalogues are extractable if there is an ExtractableDataSet with the Catalogue_ID that matches them)
            var cataToEds = new Dictionary<int,ExtractableDataSet>(ExtractableDataSets.ToDictionary(k => k.Catalogue_ID));

            //inject extractability into Catalogues
            foreach (Catalogue catalogue in AllCatalogues)
                if (cataToEds.ContainsKey(catalogue.ID))
                    catalogue.InjectKnown(cataToEds[catalogue.ID].GetCatalogueExtractabilityStatus());
                else
                    catalogue.InjectKnown(new CatalogueExtractabilityStatus(false,false));

            try
            {
                AddPipelineUseCases(new Dictionary<string, PipelineUseCase>
                {
                    {"File Import", UploadFileUseCase.DesignTime()},
                    {"Extraction",new ExtractionPipelineUseCase(Project.Empty)},
                    {"Release",ReleaseUseCase.DesignTime(repositoryLocator)},
                    {"Cohort Creation",CohortCreationRequest.DesignTime(repositoryLocator)},
                    {"Caching",CachingPipelineUseCase.DesignTime(repositoryLocator.CatalogueRepository)},
                    {"Aggregate Committing",CreateTableFromAggregateUseCase.DesignTime(repositoryLocator.CatalogueRepository)}
                });
            }
            catch (Exception ex)
            {
                _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to build DesignTime PipelineUseCases",CheckResult.Fail,ex));
            }
        }

        private void AddChildren(ExtractableDataSetPackage package, DescendancyList descendancy)
        {
            var children = new HashSet<object>(PackageContents.GetAllDataSets(package, ExtractableDataSets)
                .Select(ds => new PackageContentNode(package, ds,PackageContents)));

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
                children.Add(projectSpecificEds.Catalogue);
                AddChildren((Catalogue)projectSpecificEds.Catalogue, descendancy.Add(projectSpecificEds.Catalogue));
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
                    association.InjectKnownCohortIdentificationConfiguration(matchingCic);

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
            var configs = ExtractionConfigurations.Where(c => c.Project_ID == extractionConfigurationsNode.Project.ID).ToArray();
            foreach (ExtractionConfiguration config in configs.Where(c=>!c.IsReleased))
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
            var configs = ExtractionConfigurations.Where(c => c.Project_ID == frozenExtractionConfigurationsNode.Project.ID).ToArray();
            foreach (ExtractionConfiguration config in configs.Where(c => c.IsReleased))
            {
                AddChildren(config, descendancy.Add(config));
                children.Add(config);
            }

            AddToDictionaries(children,descendancy);
        }

        private void AddChildren(ExtractionConfiguration extractionConfiguration, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            var parameterNode = new ParametersNode(extractionConfiguration, AllGlobalExtractionFilterParameters.Where(p=>p.ExtractionConfiguration_ID == extractionConfiguration.ID).ToArray());

            children.Add(parameterNode);
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
                var rootContainer = _dataExportFilterHierarchy.AllContainers[selectedDataSets.RootFilterContainer_ID.Value];
                AddChildren(rootContainer,descendancy.Add(rootContainer));
                AddToDictionaries(new HashSet<object>(new object[]{rootContainer}),descendancy);
            }
        }

        private void AddChildren(FilterContainer filterContainer, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            foreach (var subcontainer in _dataExportFilterHierarchy.GetSubcontainers(filterContainer))
            {
                AddChildren(subcontainer,descendancy.Add(subcontainer));
                children.Add(subcontainer);
            }

            foreach (var filter in _dataExportFilterHierarchy.GetFilters(filterContainer))
                children.Add(filter);

            AddToDictionaries(children,descendancy);
        }


        private void AddChildren(CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode, DescendancyList descendancy)
        {
            AddToDictionaries(new HashSet<object>(cohortSourceUsedByProjectNode.CohortsUsed),descendancy);
        }
        
        private void AddChildren(AllCohortsNode cohortsNode, DescendancyList descendancy)
        {
            var validSources = CohortSources.Except(BlackListedSources).ToArray();

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
            foreach (ExternalCohortTable source in CohortSources.Except(BlackListedSources))
            {
                DiscoveredServer server = null;

                Exception ex = null;

                //it obviously hasn't been initialised properly yet
                if (string.IsNullOrWhiteSpace(source.Server) || string.IsNullOrWhiteSpace(source.Database))
                    continue;


                try
                {
                    server = DataAccessPortal.GetInstance().ExpectDatabase(source, DataAccessContext.DataExport).Server;
                }
                catch (Exception exception)
                {
                    ex = exception;
                }

                if (server == null || !server.RespondsWithinTime(10, out ex))
                {
                    BlackListedSources.Add(source);

                    _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs("Blacklisted source '" + source + "'",CheckResult.Fail, ex));

                    //tell them not to bother looking for the cohort data because its inaccessible
                    foreach (ExtractableCohort cohort in Cohorts.Where(c => c.ExternalCohortTable_ID == source.ID).ToArray())
                        cohort.InjectKnown((ExternalCohortDefinitionData)null);

                    continue;
                }

                using (var con = server.GetConnection())
                {
                    con.Open();
                    
                    //Get all of the project numbers and remote origin ids etc from the source in one query
                    var cmd = server.GetCommand(source.GetExternalDataSql(), con);
                    cmd.CommandTimeout = 120;

                    var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        //really should be only one here but still they might for some reason have 2 references to the same external cohort
                        var cohorts = Cohorts.Where(
                            c => c.OriginID == Convert.ToInt32(r["OriginID"]) && c.ExternalCohortTable_ID == source.ID)
                            .ToArray();

                        //Tell the cohorts what their external data values are so they don't have to fetch them themselves individually
                        foreach (ExtractableCohort c in cohorts)
                        {
                            //load external data from the result set
                            var externalData = new ExternalCohortDefinitionData(r, source.Name);

                            //tell the cohort about the data
                            c.InjectKnown(externalData);

                            //for performance also keep a dictionary of project number => compatible cohorts
                            if (!ProjectNumberToCohortsDictionary.ContainsKey(externalData.ExternalProjectNumber))
                                ProjectNumberToCohortsDictionary.Add(externalData.ExternalProjectNumber, new List<ExtractableCohort>());

                            ProjectNumberToCohortsDictionary[externalData.ExternalProjectNumber].Add(c);
                        }
                    }
                }
            }
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
            return GetConfigurations(project).Where(ec => !ec.IsReleased);
        }

        public IEnumerable<SelectedDataSets> GetDatasets(ExtractionConfiguration extractionConfiguration)
        {
            if (_configurationToDatasetMapping.ContainsKey(extractionConfiguration))
                return _configurationToDatasetMapping[extractionConfiguration];

            return new SelectedDataSets[0];
        }

        public IEnumerable<ExtractionConfiguration> GetConfigurations(Project project)
        {
            //Get the extraction configurations node of the project
            var configurationsNode = GetChildren(project).OfType<ExtractionConfigurationsNode>().Single();

            var frozenConfigurationsNode = GetChildren(configurationsNode).OfType<FrozenExtractionConfigurationsNode>().Single();

            //then add all the children extraction configurations
            return GetChildren(configurationsNode).OfType<ExtractionConfiguration>().Union(GetChildren(frozenConfigurationsNode).OfType<ExtractionConfiguration>());
        }

        public IEnumerable<ExtractableDataSet> GetDatasets(ExtractableDataSetPackage package)
        {
            return PackageContents.GetAllDataSets(package, ExtractableDataSets);
        }

        public bool ProjectHasNoSavedCohorts(Project project)
        {
            //get the projects cohort umbrella folder
            var projectCohortsNode = GetChildren(project).OfType<ProjectCohortsNode>().Single();

            //get the saved cohorts folder under it
            var projectSavedCohortsNode = GetChildren(projectCohortsNode).OfType<ProjectSavedCohortsNode>().Single();

            //if ther are no children that are Cohort Sources (cohort databases) under this saved cohorts folder then the Project has no 
            return GetChildren(projectSavedCohortsNode).OfType<CohortSourceUsedByProjectNode>().All(s => s.IsEmptyNode);
        }

        public override Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables()
        {
            var toReturn = base.GetAllSearchables();
            AddToReturnSearchablesWithNoDecendancy(toReturn,Projects);
            AddToReturnSearchablesWithNoDecendancy(toReturn, AllPackages);
            return toReturn;
        }

        /// <summary>
        /// Returns all currently selected ExtractableColumns in the given SelectedDataSets
        /// </summary>
        /// <param name="selectedDataset"></param>
        /// <returns></returns>
        public IEnumerable<ExtractableColumn> GetColumnsIn(SelectedDataSets selectedDataset)
        {
            if (ExtractionConfigurationToExtractableColumnsDictionary.ContainsKey(selectedDataset.ExtractionConfiguration_ID))
            {
                return ExtractionConfigurationToExtractableColumnsDictionary[selectedDataset.ExtractionConfiguration_ID]
                .Where(ec => ec.ExtractableDataSet_ID == selectedDataset.ExtractableDataSet_ID);
            }

            return Enumerable.Empty<ExtractableColumn>();
        }
    }
}
