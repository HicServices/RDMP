using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using DataExportLibrary.CohortDescribing;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Data.Hierarchy;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;
using DataExportManager.SimpleDialogs;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;

namespace DataExportManager.Collections.Providers
{
    public class DataExportChildProvider : CatalogueChildProvider
    {
        //root objects
        public AllCohortsNode RootCohortsNode { get; private set; }
        
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly ICheckNotifier _errorsCheckNotifier;

        public ExternalCohortTable[] CohortSources { get; private set; }
        public ExtractableDataSet[] ExtractableDataSets { get; private set; }

        public SelectedDataSets[] SelectedDataSets { get; private set; }

        public ExtractableDataSetPackage[] AllPackages { get; set; }
        public ExtractableDataSetPackageContents PackageContents { get; set; }

        public Project[] Projects { get; set; }

        public ExtractableCohort[] Cohorts { get; private set; }

        public List<CustomDataTableNode> CustomTables { get; private set; }

        public ExtractionConfiguration[] ExtractionConfigurations { get; private set; }
        
        private Dictionary<ExtractionConfiguration, SelectedDataSets[]> _configurationToDatasetMapping;
        private DataExportFilterHierarchy _dataExportFilterHierarchy;

        private static List<ExternalCohortTable> BlackListedSources = new List<ExternalCohortTable>();

        public List<IObjectUsedByProjectNode> DuplicateObjectsButUsedByProjects = new List<IObjectUsedByProjectNode>();

        public Dictionary<int,List<ExtractableCohort>> ProjectNumberToCohortsDictionary = new Dictionary<int, List<ExtractableCohort>>();

        public ProjectCohortIdentificationConfigurationAssociation[] AllProjectAssociatedCics;

        public GlobalExtractionFilterParameter[] AllGlobalExtractionFilterParameters;

        public DataExportChildProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IChildProvider[] pluginChildProviders,ICheckNotifier errorsCheckNotifier) : base(repositoryLocator.CatalogueRepository, pluginChildProviders,errorsCheckNotifier)
        {
            _repositoryLocator = repositoryLocator;
            _errorsCheckNotifier = errorsCheckNotifier;
            var dataExportRepository = repositoryLocator.DataExportRepository;

            AllProjectAssociatedCics = dataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>();
            CohortSources = dataExportRepository.GetAllObjects<ExternalCohortTable>();
            ExtractableDataSets = dataExportRepository.GetAllObjects<ExtractableDataSet>();

            SelectedDataSets = dataExportRepository.GetAllObjects<SelectedDataSets>();
            var dsDictionary = ExtractableDataSets.ToDictionary(ds => ds.ID, d => d);
            foreach (SelectedDataSets s in SelectedDataSets)
                s.SetKnownDataSet(dsDictionary[s.ExtractableDataSet_ID]);
            
            //This means that the ToString method in ExtractableDataSet doesn't need to go lookup catalogue info
            var dictionary = AllCatalogues.ToDictionary(c => c.ID, c2 => c2);
            foreach (ExtractableDataSet ds in ExtractableDataSets)
                if(dictionary.ContainsKey(ds.Catalogue_ID))
                    ds.SetKnownCatalogue(dictionary[ds.Catalogue_ID]);
                
            AllPackages = dataExportRepository.GetAllObjects<ExtractableDataSetPackage>();
            PackageContents = new ExtractableDataSetPackageContents(dataExportRepository);

            Projects = dataExportRepository.GetAllObjects<Project>();
            ExtractionConfigurations = dataExportRepository.GetAllObjects<ExtractionConfiguration>();
            AllGlobalExtractionFilterParameters = dataExportRepository.GetAllObjects<GlobalExtractionFilterParameter>();

            _dataExportFilterHierarchy = new DataExportFilterHierarchy(dataExportRepository);

            Cohorts = dataExportRepository.GetAllObjects<ExtractableCohort>();
            
            GetCustomTables();
            
            _configurationToDatasetMapping = new Dictionary<ExtractionConfiguration, SelectedDataSets[]>();

            foreach (ExtractionConfiguration configuration in ExtractionConfigurations)
                _configurationToDatasetMapping.Add(configuration, SelectedDataSets.Where(c => c.ExtractionConfiguration_ID == configuration.ID).ToArray());

            RootCohortsNode = new AllCohortsNode();
            AddChildren(RootCohortsNode,new DescendancyList(RootCohortsNode));

            foreach (ExtractableDataSetPackage package in AllPackages)
                AddChildren(package, new DescendancyList(package));

            foreach (Project p in Projects)
                AddChildren(p, new DescendancyList(p));

            //work out all the Catalogues that are extractable (Catalogues are extractable if there is an ExtractableDataSet with the Catalogue_ID that matches them)
            var extractableCatalogueIds = new HashSet<int>(ExtractableDataSets.Select(ds => ds.Catalogue_ID));

            //inject extractability into Catalogues
            foreach (Catalogue catalogue in AllCatalogues)
                catalogue.InjectExtractability(extractableCatalogueIds.Contains(catalogue.ID));
        }

        private void AddChildren(ExtractableDataSetPackage package, DescendancyList descendancy)
        {
            var children = new HashSet<object>(PackageContents.GetAllDataSets(package, ExtractableDataSets)
                .Select(ds => new PackageContentNode(package, ds)));

            AddToDictionaries(children, descendancy);

        }
        
        private void AddChildren(Project project, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            var projectCiCsNode = new ProjectCohortIdentificationConfigurationAssociationsNode(project);
            children.Add(projectCiCsNode);
            AddChildren(projectCiCsNode,descendancy.Add(projectCiCsNode));

            var savedCohortsNode = new ProjectSavedCohortsNode(project);
            children.Add(savedCohortsNode);
            AddChildren(savedCohortsNode,descendancy.Add(savedCohortsNode));


            var extractionConfigurationsNode = new ExtractionConfigurationsNode(project);
            children.Add(extractionConfigurationsNode);

            AddChildren(extractionConfigurationsNode,descendancy.Add(extractionConfigurationsNode));
            
            var folder = new ExtractionDirectoryNode(project);
            children.Add(folder);
            AddToDictionaries(children,descendancy);
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
                //inject knowledge of what the cic is so it doesn't have to be fetched during ToString
                association.InjectKnownCohortIdentificationConfiguration(
                    AllCohortIdentificationConfigurations.Single(
                        cic => cic.ID == association.CohortIdentificationConfiguration_ID));
                
                //document that it is a child of the project cics node 
                children.Add(association);
            }

            AddToDictionaries(children, descendancy);
        }

        private void AddChildren(ExtractionConfigurationsNode extractionConfigurationsNode, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            var configs = ExtractionConfigurations.Where(c => c.Project_ID == extractionConfigurationsNode.Project.ID).ToArray();
            foreach (ExtractionConfiguration config in configs)
            {
                AddChildren(config, descendancy.Add(config));
                children.Add(config);
            }

            AddToDictionaries(children, descendancy);
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
            foreach (var usedCohort in cohortSourceUsedByProjectNode.CohortsUsedByProject)
                AddChildren(usedCohort, descendancy.Add(usedCohort));

            AddToDictionaries(new HashSet<object>(cohortSourceUsedByProjectNode.CohortsUsedByProject),descendancy);
        }

        private void AddChildren(ExtractableCohortUsedByProjectNode usedCohort, DescendancyList descendancy)
        {
            if(usedCohort.CustomTablesUsed.Any())
                AddToDictionaries(new HashSet<object>(usedCohort.CustomTablesUsed),descendancy );
        }

        private void GetCustomTables()
        {
            CustomTables = new List<CustomDataTableNode>();

            foreach (ExternalCohortTable source in CohortSources.Except(BlackListedSources))
            {
                
                DiscoveredServer server=null;

                Exception ex=null;

                //it obviously hasn't been initialised properly yet
                if(string.IsNullOrWhiteSpace(source.Server) || string.IsNullOrWhiteSpace(source.Database))
                    continue;
                

                try
                {
                    server = DataAccessPortal.GetInstance().ExpectDatabase(source, DataAccessContext.DataExport).Server;
                }
                catch (Exception exception)
                {
                    ex = exception;
                }
                
                if(server == null || !server.RespondsWithinTime(10, out ex))
                {
                    ExceptionViewer.Show("Could not list custom tables in CohortSource " + source.Name ,ex);

                    var dialog = new BlacklistOptions("CohortSource '" + source.Name + "' is not responding");
                    dialog.ShowDialog();

                    switch (dialog.Response)
                    {
                        case BlacklistResponse.Ignore:
                            continue;
                        case BlacklistResponse.BlacklistSource:
                            BlackListedSources.Add(source);
                            continue;
                        case BlacklistResponse.BlacklistAll:
                            BlackListedSources.AddRange(CohortSources);
                            return;
                        case BlacklistResponse.UnsetDataExport:
                            ((RegistryRepositoryFinder)_repositoryLocator).SetRegistryValue(RegistrySetting.DataExportManager, "");
                            throw new Exception("User has unset Data Export Manager Location");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
               
                using (var con = server.GetConnection())
                {
                    con.Open();

                    var cmd = server.GetCommand(source.GetCustomTableSql(), con);
                    cmd.CommandTimeout = 120; //give it up to 2 minutes
                    var r = cmd.ExecuteReader();

                    while (r.Read())
                    {
                        var cohorts = Cohorts.Where(
                            c => c.OriginID == Convert.ToInt32(r["OriginID"]) && c.ExternalCohortTable_ID == source.ID)
                            .ToArray();

                        foreach (ExtractableCohort c in cohorts)
                            CustomTables.Add(new CustomDataTableNode(c, r["CustomTableName"] as string, Convert.ToBoolean(r["active"])));
                    }
                    r.Close();

                    //Get all of the project numbers and remote origin ids etc from the source in one query
                    var cmd2 = server.GetCommand(source.GetExternalDataSql(), con);
                    cmd2.CommandTimeout = 120;

                    r = cmd2.ExecuteReader();
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
                            var externalData = new ExternalCohortDefinitionData((SqlDataReader) r, source.Name);
                            
                            //tell the cohort about the data
                            c.SetKnownExternalData(externalData);

                            //for performance also keep a dictionary of project number => compatible cohorts
                            if(!ProjectNumberToCohortsDictionary.ContainsKey(externalData.ExternalProjectNumber))
                                ProjectNumberToCohortsDictionary.Add(externalData.ExternalProjectNumber,new List<ExtractableCohort>());
                            
                            ProjectNumberToCohortsDictionary[externalData.ExternalProjectNumber].Add(c);
                            
                        }
                    }
                }
            }
        }

        private void AddChildren(AllCohortsNode cohortsNode, DescendancyList descendancy)
        {
            AddToDictionaries(new HashSet<object>(CohortSources), descendancy);
            foreach (var s in CohortSources)
                AddChildren(s, descendancy.Add(s));
        }

        private void AddChildren(ExternalCohortTable externalCohortTable, DescendancyList descendancy)
        {
            var cohorts = Cohorts.Where(c => c.ExternalCohortTable_ID == externalCohortTable.ID).ToArray();
            foreach (var cohort in cohorts)
                AddChildren(cohort, descendancy.Add(cohort));

            AddToDictionaries(new HashSet<object>(cohorts), descendancy);
        }

        private void AddChildren(ExtractableCohort extractableCohort, DescendancyList descendancy)
        {
            AddToDictionaries(new HashSet<object>(CustomTables.Where(ct => ct.Cohort.Equals(extractableCohort))), descendancy);
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
                var existing = toReturn.SingleOrDefault(s => s.Source.ID == cohort.ExternalCohortTable_ID);

                //make sure we have a record of the source
                if (existing == null)
                {
                    existing = new CohortSourceUsedByProjectNode(project, source);
                    toReturn.Add(existing);
                }

                //add the cohort to the list of known cohorts from this source (a project can have lots of cohorts and even cohorts from different sources) 
                var cohortUsedByProject = new ExtractableCohortUsedByProjectNode(cohort, project, CustomTables);
                existing.CohortsUsedByProject.Add(cohortUsedByProject);

                DuplicateObjectsButUsedByProjects.Add(cohortUsedByProject);
                DuplicateObjectsButUsedByProjects.AddRange(cohortUsedByProject.CustomTablesUsed);
            }

            DuplicateObjectsButUsedByProjects.AddRange(toReturn);

            //if the project has no cohorts then add a ??? node 
            if(!toReturn.Any())
                toReturn.Add(new CohortSourceUsedByProjectNode(project));

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

            //then add all the children extraction configurations
            return GetChildren(configurationsNode).OfType<ExtractionConfiguration>();
        }

        public IEnumerable<ExtractableDataSet> GetDatasets(ExtractableDataSetPackage package)
        {
            return PackageContents.GetAllDataSets(package, ExtractableDataSets);
        }

        public bool ProjectHasNoSavedCohorts(Project project)
        {

            return GetChildren(GetChildren(project).Single(n => n is ProjectSavedCohortsNode))
                .OfType<CohortSourceUsedByProjectNode>().All(s => s.IsEmptyNode);
        }

        public override Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables()
        {
            var toReturn = base.GetAllSearchables();
            AddToReturnSearchablesWithNoDecendancy(toReturn,Projects);
            AddToReturnSearchablesWithNoDecendancy(toReturn, AllPackages);
            return toReturn;
        }
    }
}
