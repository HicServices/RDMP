using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Repositories;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Represents a collection of datasets (Catalogues), ExtractableColumns, ExtractionFilters etc and a single ExtractableCohort for a Project.  You can have multiple active
    /// ExtractionConfigurations at a time for example a Project might have two cohorts 'Cases' and 'Controls' and you would have two ExtractionConfiguration possibly containing
    /// the same datasets and filters but with different cohorts.
    /// 
    /// Once you have executed, extracted and released an ExtractionConfiguration then it becomes 'frozen' IsReleased and it is not possible to edit it (unless you unfreeze it 
    /// directly in the database).  This is intended to ensure that once data has gone out the door the configuration that generated the data is immutable.
    /// 
    /// If you need to perform a repeat extraction (e.g. an update of data 5 years on) then you should 'Clone' the ExtractionConfiguration in the Project and give it a new name 
    /// e.g. 'Cases - 5 year update'.
    /// </summary>
    public class ExtractionConfiguration : VersionedDatabaseEntity, IExtractionConfiguration, ICollectSqlParameters,INamed,ICustomSearchString
    {
        #region Database Properties
        private DateTime? _dtCreated;
        private int? _cohort_ID;
        private string _requestTicket;
        private string _releaseTicket;
        private int _project_ID;
        private string _username;
        private string _separator;
        private string _description;
        private bool _isReleased;
        private string _name;
        private int? _clonedFrom_ID;

        private int? _defaultPipeline_ID;
        private int? _cohortIdentificationConfigurationID;
        private int? _cohortRefreshPipelineID;


        public int? CohortRefreshPipeline_ID
        {
            get { return _cohortRefreshPipelineID; }
            set {  SetField(ref _cohortRefreshPipelineID , value); }
        }

        public int? CohortIdentificationConfiguration_ID
        {
            get { return _cohortIdentificationConfigurationID; }
            set { SetField(ref _cohortIdentificationConfigurationID, value); }
        }

        public int? DefaultPipeline_ID
        {
            get { return _defaultPipeline_ID; }
            set { SetField(ref _defaultPipeline_ID, value); }
        }

        public DateTime? dtCreated
        {
            get { return _dtCreated; }
            set { SetField(ref _dtCreated, value); }
        }
        public int? Cohort_ID
        {
            get { return _cohort_ID; }
            set { SetField(ref _cohort_ID, value); }
        }
        public string RequestTicket
        {
            get { return _requestTicket; }
            set { SetField(ref _requestTicket, value); }
        }
        public string ReleaseTicket
        {
            get { return _releaseTicket; }
            set { SetField(ref _releaseTicket, value); }
        }
        public int Project_ID
        {
            get { return _project_ID; }
            set { SetField(ref _project_ID, value); }
        }
        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value); }
        }
        public string Separator
        {
            get { return _separator; }
            set { SetField(ref _separator, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }
        public bool IsReleased
        {
            get { return _isReleased; }
            set { SetField(ref _isReleased, value); }
        }
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public int? ClonedFrom_ID
        {
            get { return _clonedFrom_ID; }
            set { SetField(ref _clonedFrom_ID, value); }
        }

        #endregion
        
        #region MaxLengths
        public static int Username_MaxLength = -1;
        public static int RequestTicket_MaxLength = -1;
        public static int ReleaseTicket_MaxLength = -1;
        public static int Separator_MaxLength = -1;
        public static int Description_MaxLength = -1;
        #endregion

        #region Relationships
        
        /// <inheritdoc cref="Project_ID"/>
        [NoMappingToDatabase]
        public IProject Project
        {
            get { return Repository.GetObjectByID<Project>(Project_ID); }
        }
        
        [NoMappingToDatabase]
        public ISqlParameter[] GlobalExtractionFilterParameters
        {
            get
            {
                return
                    Repository.GetAllObjectsWithParent<GlobalExtractionFilterParameter>(this)
                        .Cast<ISqlParameter>().ToArray();
            }
        }

        [NoMappingToDatabase]
        public IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults
        {
            get
            {
                return Repository.GetAllObjectsWithParent<CumulativeExtractionResults>(this);
            }
        }
        
        /// <inheritdoc cref="Cohort_ID"/>
        [NoMappingToDatabase]
        public IExtractableCohort Cohort
        {
            get
            {
                return Cohort_ID == null ? null : Repository.GetObjectByID<ExtractableCohort>(Cohort_ID.Value);
            }
        }

        [NoMappingToDatabase]
        public ISelectedDataSets[] SelectedDataSets
        {
            get
            {
                return Repository.GetAllObjectsWithParent<SelectedDataSets>(this).Cast<ISelectedDataSets>().ToArray();
            }
        }

        [NoMappingToDatabase]
        public IReleaseLogEntry[] ReleaseLogEntries
        {
            get
            {
                List<IReleaseLogEntry> entries = new List<IReleaseLogEntry>();

                var repo = (DataExportRepository) Repository;
                using (var con = repo.GetConnection())
                {
                    var cmdselect = DatabaseCommandHelper.GetCommand(@"SELECT *
  FROM ReleaseLog
  where
  CumulativeExtractionResults_ID
  in
  (select ID from CumulativeExtractionResults where ExtractionConfiguration_ID = @ExtractionConfiguration_ID)",
                    con.Connection, con.Transaction);

                    DatabaseCommandHelper.AddParameterWithValueToCommand("@ExtractionConfiguration_ID", cmdselect, ID);
                
                    var sqlDataReader = cmdselect.ExecuteReader();
                    while (sqlDataReader.Read())
                        entries.Add(new ReleaseLogEntry(repo, sqlDataReader));
                }

                return entries.ToArray();
            }
        }

        /// <inheritdoc cref="DefaultPipeline_ID"/>
        [NoMappingToDatabase]
        public IPipeline DefaultPipeline {
            get
            {
                if (DefaultPipeline_ID == null)
                    return null;

                return
                    ((DataExportRepository) Repository).CatalogueRepository.GetObjectByID<Pipeline>(DefaultPipeline_ID.Value);
            }}


        /// <inheritdoc cref="CohortIdentificationConfiguration_ID"/>
        [NoMappingToDatabase]
        public CohortIdentificationConfiguration CohortIdentificationConfiguration
        {
            get
            {
                if (CohortIdentificationConfiguration_ID == null)
                    return null;

                return
                    ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<CohortIdentificationConfiguration>(CohortIdentificationConfiguration_ID.Value);
            }
        }

        /// <inheritdoc cref="CohortRefreshPipeline_ID"/>
        [NoMappingToDatabase]
        public IPipeline CohortRefreshPipeline
        {
            get
            {
                if (CohortRefreshPipeline_ID == null)
                    return null;

                return
                    ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<Pipeline>(CohortRefreshPipeline_ID.Value);
            }
        }


        #endregion

        /// <summary>
        /// Used for static Test property, do not ever make public
        /// </summary>
        private ExtractionConfiguration()
        {
        }
        
        public ExtractionConfiguration(IDataExportRepository repository, Project project)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"dtCreated", DateTime.Now},
                {"Project_ID", project.ID},
                {"Username", Environment.UserName},
                {"Description", "Initial Configuration"},
                {"Name","New ExtractionConfiguration" + Guid.NewGuid() }
            });
        }

        internal ExtractionConfiguration(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Project_ID = int.Parse(r["Project_ID"].ToString());

            if (!string.IsNullOrWhiteSpace(r["Cohort_ID"].ToString()))
                Cohort_ID = int.Parse(r["Cohort_ID"].ToString());

            RequestTicket = r["RequestTicket"].ToString();
            ReleaseTicket = r["ReleaseTicket"].ToString();

            object dt = r["dtCreated"];

            if (dt == null || dt == DBNull.Value)
                dtCreated = null;
            else
                dtCreated = (DateTime) dt;

            Username = r["Username"] as string;
            Description = r["Description"] as string;
            Separator = r["Separator"] as string;
            IsReleased = (bool)r["IsReleased"];
            Name = r["Name"] as string;

            if (r["ClonedFrom_ID"] == DBNull.Value)
                ClonedFrom_ID = null;
            else
                ClonedFrom_ID = Convert.ToInt32(r["ClonedFrom_ID"]);

            DefaultPipeline_ID = ObjectToNullableInt(r["DefaultPipeline_ID"]);
            CohortIdentificationConfiguration_ID = ObjectToNullableInt(r["CohortIdentificationConfiguration_ID"]);
            CohortRefreshPipeline_ID = ObjectToNullableInt(r["CohortRefreshPipeline_ID"]);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public string GetSearchString()
        {
            return ToString() + "_" + RequestTicket + "_" + ReleaseTicket;
        }

        public ISqlParameter[] GetAllParameters()
        {
            return GlobalExtractionFilterParameters;
        }

        public override string ToString()
        {
            return Name;
        }

        #region Stuff for updating our internal database records
   

        public IExtractionConfiguration[] GetAllExtractionConfigurationForProject(IProject project)
        {
            return project.ExtractionConfigurations;
        }

        public ExtractionConfiguration GetExtractionConfigurationByID(int id, DbConnection connection = null, DbTransaction transaction = null)
        {
            return Repository.GetObjectByID<ExtractionConfiguration>(id);
        }
        #endregion

        public ExtractionConfiguration DeepCloneWithNewIDs()
        {
            var repo = (DataExportRepository)Repository;
            using (repo.BeginNewTransactedConnection())
            {
                try
                {
                    //clone the root object (the configuration) - this includes cloning the link to the correct project and cohort 
                    ExtractionConfiguration clone = repo.CloneObjectInTable(this);

                    //find each of the selected datasets for ourselves and clone those too
                    foreach (ExtractableDataSet dataSet in GetAllExtractableDataSets())
                    {
                        //clone the link meaning that the dataset is now selected for the clone configuration too 
                        var newSelectedDataSet = new SelectedDataSets(repo, clone, dataSet, null);

                        // now clone each of the columns for each of the datasets that we just created links to (make them the same as the old configuration
                        foreach (IColumn extractableColumn in GetAllExtractableColumnsFor(dataSet))
                        {
                            ExtractableColumn cloneExtractableColumn = repo.CloneObjectInTable((ExtractableColumn)extractableColumn);
                            cloneExtractableColumn.ExtractionConfiguration_ID = clone.ID;
                            cloneExtractableColumn.SaveToDatabase();
                        }

                        try
                        {
                            //clone the root filter container
                            var rootContainer = (FilterContainer)GetFilterContainerFor(dataSet);

                            //turns out there wasn't one to clone at all
                            if (rootContainer == null)
                                continue;

                            //there was one to clone so clone it recursively (all subcontainers) including filters then set the root filter to the new clone 
                            FilterContainer cloneRootContainer = rootContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
                            newSelectedDataSet.RootFilterContainer_ID = cloneRootContainer.ID;
                            newSelectedDataSet.SaveToDatabase();
                        }
                        catch (Exception e)
                        {
                            clone.DeleteInDatabase();
                            throw new Exception("Problem occurred during cloning filters, problem was " + e.Message + " deleted the clone configuration successfully",e);
                        }
                    }

                    clone.dtCreated = DateTime.Now;
                    clone.IsReleased = false;
                    clone.Username = Environment.UserName;
                    clone.Description = "TO"+"DO:Populate change log here";
                    clone.ReleaseTicket = null;

                    //wire up some changes
                    clone.ClonedFrom_ID = this.ID;
                    clone.SaveToDatabase();

                    repo.EndTransactedConnection(true);

                    return clone;
                }
                catch (Exception)
                {
                    repo.EndTransactedConnection(false);
                    throw;
                }
            }
        }

        public IProject GetProject()
        {
            return Repository.GetObjectByID<Project>(Project_ID);
        }

        public ConcreteColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset)
        {
            return Repository.GetAllObjects<ExtractableColumn>(
                string.Format(
                    "WHERE ExtractionConfiguration_ID={0} AND ExtractableDataSet_ID={1}",
                    ID,
                    dataset.ID));
        }
        public IContainer GetFilterContainerFor(IExtractableDataSet dataset)
        {
            var objects = Repository.SelectAllWhere<FilterContainer>(
                "SELECT RootFilterContainer_ID FROM SelectedDataSets WHERE ExtractionConfiguration_ID=@ExtractionConfiguration_ID AND ExtractableDataSet_ID=@ExtractableDataSet_ID",
                "RootFilterContainer_ID",
                new Dictionary<string, object>
                {
                    {"ExtractionConfiguration_ID", ID},
                    {"ExtractableDataSet_ID", dataset.ID}
                });

            return objects.SingleOrDefault();
        }

        public ExternalDatabaseServer GetDistinctLoggingServer(bool testLoggingServer)
        {
            int uniqueLoggingServerID = -1;

            var repo = ((DataExportRepository) Repository);

            foreach (int? catalogueID in GetAllExtractableDataSets().Select(ds=>ds.Catalogue_ID))
            {
                if(catalogueID == null)
                    throw new Exception("Cannot get logging server because some ExtractableDatasets in the configuration do not have associated Catalogues (possibly the Catalogue was deleted)");

                var catalogue = repo.CatalogueRepository.GetObjectByID<Catalogue>((int)catalogueID);

                int? loggingServer = testLoggingServer
                    ? catalogue.TestLoggingServer_ID
                    : catalogue.LiveLoggingServer_ID;

                if ( loggingServer == null)
                    throw new Exception("Catalogue " + catalogue.Name + " does not have a "+(testLoggingServer?"test":"")+" logging server configured");

                if (uniqueLoggingServerID == -1)
                    uniqueLoggingServerID = (int) catalogue.LiveLoggingServer_ID;
                else
                {
                    if(uniqueLoggingServerID != catalogue.LiveLoggingServer_ID)
                        throw new Exception("Catalogues in configuration have different logging servers");
                }
            }

            return repo.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(uniqueLoggingServerID);
        }

        public IExtractableCohort GetExtractableCohort()
        {
            if (Cohort_ID == null)
                return null;

            return Repository.GetObjectByID<ExtractableCohort>(Cohort_ID.Value);
        }

        public IExtractableDataSet[] GetAllExtractableDataSets()
        {
            return Repository.SelectAllWhere<ExtractableDataSet>(
                "SELECT * FROM SelectedDataSets WHERE ExtractionConfiguration_ID = @ExtractionConfiguration_ID",
                "ExtractableDataSet_ID",
                new Dictionary<string, object>
                {
                    {"ExtractionConfiguration_ID", ID}
                })
                .ToArray();
        }


        public void AddDatasetToConfiguration(IExtractableDataSet extractableDataSet)
        {
            //it is already part of the configuration
            if( SelectedDataSets.Any(s => s.ExtractableDataSet_ID == extractableDataSet.ID))
                return;

            var dataExportRepo = (IDataExportRepository)Repository;

            var selectedDataSet = new SelectedDataSets(dataExportRepo, this, extractableDataSet, null);

            ExtractionFilter[] mandatoryExtractionFiltersToApplyToDataset = extractableDataSet.Catalogue.GetAllMandatoryFilters();

            //add mandatory filters
            if (mandatoryExtractionFiltersToApplyToDataset.Any())
            {
                //first we need a root container e.g. an AND container
                //add the AND container and set it as the root container for the dataset configuration
                FilterContainer rootFilterContainer = new FilterContainer(dataExportRepo);
                rootFilterContainer.Operation = FilterContainerOperation.AND;
                rootFilterContainer.SaveToDatabase();

                selectedDataSet.RootFilterContainer_ID = rootFilterContainer.ID;
                selectedDataSet.SaveToDatabase();

                var globals = GlobalExtractionFilterParameters;
                var importer = new FilterImporter(new DeployedExtractionFilterFactory(dataExportRepo), globals);

                var mandatoryFilters = importer.ImportAllFilters(mandatoryExtractionFiltersToApplyToDataset, null);

                foreach (DeployedExtractionFilter filter in mandatoryFilters.Cast<DeployedExtractionFilter>())
                {
                    filter.FilterContainer_ID = rootFilterContainer.ID;
                    filter.SaveToDatabase();
                }
            }

            var legacyColumns = GetAllExtractableColumnsFor(extractableDataSet).Cast<ExtractableColumn>().ToArray();

            //add Core columns
            foreach (var core in extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Core))
                if (legacyColumns.All(l => l.CatalogueExtractionInformation_ID != core.ID))
                    AddColumnToExtraction(extractableDataSet, core);
        }

        public void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet)
        {
            var match = SelectedDataSets.SingleOrDefault(s => s.ExtractableDataSet_ID == extractableDataSet.ID);
            if(match != null)
                match.DeleteInDatabase();
        }

        public ExtractableColumn AddColumnToExtraction(IExtractableDataSet forDataSet,IColumn item)
        {
            if (string.IsNullOrWhiteSpace(item.SelectSQL))
                throw new ArgumentException("IColumn (" + item.GetType().Name + ") " + item +" has a blank value for SelectSQL, fix this in the CatalogueManager", "item");

            string query = "";
            query = item.SelectSQL;

            ExtractableColumn addMe;

            if (item is ExtractionInformation)
                addMe = new ExtractableColumn((IDataExportRepository)Repository, forDataSet, this, item as ExtractionInformation, -1, query);
            else
                addMe = new ExtractableColumn((IDataExportRepository)Repository, forDataSet, this, null, -1, query); // its custom column of some kind, not tied to a catalogue entry

            //Add new things you want to copy from the Catalogue here
            addMe.HashOnDataRelease = item.HashOnDataRelease;
            addMe.IsExtractionIdentifier = item.IsExtractionIdentifier;
            addMe.IsPrimaryKey = item.IsPrimaryKey;
            addMe.Order = item.Order;
            addMe.Alias = item.Alias;
            addMe.SaveToDatabase();
            return addMe;
        }

        public LogManager GetExplicitLoggingDatabaseServerOrDefault()
        {
            ExternalDatabaseServer loggingServer=null;
            try
            {
                loggingServer = GetDistinctLoggingServer(false);
            }
            catch (Exception e)
            {
                //failed to get a logging server correctly

                //see if there is a default
                var defaultGetter = new ServerDefaults(Project.DataExportRepository.CatalogueRepository);
                var defaultLoggingServer = defaultGetter.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

                //there is a default?
                if (defaultLoggingServer != null)
                    loggingServer = (ExternalDatabaseServer)defaultLoggingServer;
                
                //no, there is no default or user does not want to use it.
                throw new Exception("There is no default logging server configured and there was a problem asking Catalogues for a logging server instead.  Configure a default logging server via ManageExternalServersUI", e);
            }
            
            var server = DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging);

            LogManager lm;

            try
            {
                lm = new LogManager(server);

                if (!lm.ListDataTasks().Contains(ExecuteDatasetExtractionSource.AuditTaskName))
                {
                    throw new Exception("The logging database " + server +
                                        " does not contain a DataLoadTask called '" + ExecuteDatasetExtractionSource.AuditTaskName +
                                        "' (all data exports are logged under this task regardless of dataset/Catalogue)");

                }
            }
            catch (Exception e)
            {
                throw new Exception("Problem figuring out what logging server to use:" + Environment.NewLine + "\t" + e.Message, e);
            }

            return lm;
        }

        public void Unfreeze()
        {
            foreach (ICumulativeExtractionResults cumulativeExtractionResult in CumulativeExtractionResults)
            {
                //delete the release audit
                var release = cumulativeExtractionResult.GetReleaseLogEntryIfAny();
                if(release != null)
                    release.DeleteInDatabase();

                //delete the extraction result
                cumulativeExtractionResult.DeleteInDatabase();
            }

            IsReleased = false;
            SaveToDatabase();
        }
    }
}
