using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataLoadEngine.Checks;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using FAnsi;
using FAnsi.Discovery;
using LoadModules.Generic.Attachers;
using LoadModules.Generic.Mutilators;
using Diagnostics.TestData;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Diagnostics
{

    public class UserAcceptanceTestEnvironment : UserAcceptanceTest, ICheckable
    {
        private readonly string _datasetFolderPath;
        private readonly string _loggingTask;

        public const string TestDatabase = "DMP_Test";
        private const string TestTableName = "TestTableForDMP";
        public const string CatalogueName = "DMPTestCatalogue";

        private const string LoadMetaDataName = "Example Test Data Loader";
        private const string CSVAttacherProcessName = "Example CSV Attacher";
        private const string ResolvePrimaryKeyDuplicationProcessName = "Resolve Primary Key Duplication";
        
        private const string CSVTestFileName = "test.csv";

        private TableInfo _demographyTableInfo;
        public TableInfo DemographyTableInfo { get { return _demographyTableInfo; } }

        private ColumnInfo[] _demographyColumnInfos;

        public Catalogue DemographyCatalogue { get; private set; }

        LoadMetadata _loadMetaData;
        
        HICProjectDirectory _hicProjectDirectory;
        private bool implementAnonymisationIntoTestConfiguration = false;

        Dictionary<DiscoveredDatabase,string> externalDatabaseNamesDictionary = new Dictionary<DiscoveredDatabase, string>();
        Dictionary<DiscoveredDatabase,SqlConnectionStringBuilder>  buildersDictionary = new Dictionary<DiscoveredDatabase, SqlConnectionStringBuilder>();

        //existing databases
        public DiscoveredDatabase ANOIdentifiers { get; set; }
        public DiscoveredDatabase IdentifierDump { get; set; }
        public DiscoveredDatabase LoggingServer { get; set; }
        
        private readonly SqlConnectionStringBuilder _serverToCreateRawDataOn;
        private readonly DiscoveredServer _discoveredServerToCreateRawDataOn;

        //catalogue entries once they exist
        private ExternalDatabaseServer _anoServer;
        private ExternalDatabaseServer _identifierDumpServer;
        private ExternalDatabaseServer _loggingServer;

        private TestDemography _testDemography;
        private ANOTable _anoTable;

        private List<ExternalDatabaseServer> _serversCreated = new List<ExternalDatabaseServer>();

        ArgumentFactory _argumentFactory = new ArgumentFactory();

        public UserAcceptanceTestEnvironment(SqlConnectionStringBuilder serverToCreateRawDataOnBuilder, string datasetFolderPath, SqlConnectionStringBuilder loggingBuilder, string LoggingTask, SqlConnectionStringBuilder anoBuilder, SqlConnectionStringBuilder dumpBuilder, IRDMPPlatformRepositoryServiceLocator repositoryLocator) : base(repositoryLocator)
        {
            LoggingServer = new DiscoveredServer(loggingBuilder).ExpectDatabase(loggingBuilder.InitialCatalog);
            buildersDictionary.Add(LoggingServer,loggingBuilder);

            if (anoBuilder != null)
                if (dumpBuilder != null)
                {
                    implementAnonymisationIntoTestConfiguration = true;

                    IdentifierDump = new DiscoveredServer(dumpBuilder).ExpectDatabase(dumpBuilder.InitialCatalog);
                    buildersDictionary.Add(IdentifierDump,dumpBuilder);

                    ANOIdentifiers = new DiscoveredServer(anoBuilder).ExpectDatabase(anoBuilder.InitialCatalog);
                    buildersDictionary.Add(ANOIdentifiers,anoBuilder);
                }
                else
                    throw new NotSupportedException("if ANOIdentifiers is set then IdentifierDump must also be set (you can have both or none but not one or the other - XNOR)");

            //create a copy so we can mutate it without affecting other users
            _serverToCreateRawDataOn = new SqlConnectionStringBuilder(serverToCreateRawDataOnBuilder.ConnectionString);
            _discoveredServerToCreateRawDataOn = new DiscoveredServer(_serverToCreateRawDataOn);

            _datasetFolderPath = datasetFolderPath;
            _loggingTask = LoggingTask;

            //each of the DiscoveredDatabase objects will be imported into catalogue and configured as ExternalDatabaseServers, this dictionary decides the naem of those entities
            externalDatabaseNamesDictionary.Add(LoggingServer, "Logging Server (Created by SetupTestDatasetEnvironment)");

            if (implementAnonymisationIntoTestConfiguration)
            {
                externalDatabaseNamesDictionary.Add(ANOIdentifiers, "ANO Identifiers Server (Created by SetupTestDatasetEnvironment)");
                externalDatabaseNamesDictionary.Add(IdentifierDump, "Identifier Dump Server (Created by SetupTestDatasetEnvironment)");
            }
        }

        
        public void Check(ICheckNotifier notifier)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_serverToCreateRawDataOn.ConnectionString))
                {
                    //if any of these steps fail, stop the test
                    if (CheckDatasetFolderPathExists(notifier))
                        if (SetupConnectionAndDatabase(con, notifier))
                            if (SetupTestDemographyTable(con, notifier))
                                if (SetupTestHospitalAdmissionsTable(con, notifier))
                                    if (ImportTestTableIntoCatalogue(notifier))
                                        if (CreateDemographyCatalogue(notifier))
                                            if (SetupAnonymisationServers(notifier))
                                                if (ConfigureAnANOColumn(notifier))
                                                    if (SetupLogging(notifier))
                                                        if (CreateLoadMetadata(notifier))
                                                            if (CreateHICProjectDirectory(notifier))
                                                                if (SetupTestLoadProcessAndArgumentsForCSVAttacher(notifier))
                                                                    if (AddDuplicateResolutionMutilator(notifier))
                                                                        if (CreateTestCSVFileInForLoading(notifier))
                                                                            if (SetupPrimaryKeyConflictResolution(notifier))
                                                                                if (RunPreLoadChecks(notifier))
                                                                                {
                                                                                    notifier.OnCheckPerformed(new CheckEventArgs("Setup Complete, Now you should configure Logging for your Catalogue and then launch the DataLoadEngine to test your configuration", CheckResult.Success, null));
                                                                                    return;
                                                                                }
                }

                

                notifier.OnCheckPerformed(new CheckEventArgs("Setup Abandoned due to stage failure", CheckResult.Fail, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Entire setup process crashed unexpectedly, See Exception for details",
                    CheckResult.Fail, e));
            }                    
        }
        
        private bool RunPreLoadChecks(ICheckNotifier notifier)
        {
            try
            {
                var builder = ((SqlConnectionStringBuilder)_discoveredServerToCreateRawDataOn.Builder);

                var defaults = new ServerDefaults(RepositoryLocator.CatalogueRepository);
                if (defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.RAWDataLoadServer) == null)
                {
                    var create = notifier.OnCheckPerformed(new CheckEventArgs("There is no currently configured RAW server",
                        CheckResult.Warning, null,
                        "Set the RAW server to " + builder.DataSource + "? Say 'No' to stick with localhost"));

                    if(create)
                    {
                        var raw = new ExternalDatabaseServer(RepositoryLocator.CatalogueRepository, "RAW");
                        raw.Server = _discoveredServerToCreateRawDataOn.Name;
                        raw.Username = builder.UserID;
                        raw.Password = builder.Password;
                        raw.SaveToDatabase();

                        defaults.SetDefault(ServerDefaults.PermissableDefaults.RAWDataLoadServer, raw);
                        
                        _serversCreated.Add(raw);
                    }
                }
                
                new CheckEntireDataLoadProcess(_loadMetaData, new HICDatabaseConfiguration(_loadMetaData),
                    new HICLoadConfigurationFlags(), RepositoryLocator.CatalogueRepository.MEF).Check(new AcceptAllCheckNotifier());
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to run CheckEntireDataLoadProcess",CheckResult.Fail, e));
                return false;
            }
            return true;
        }


        private bool SetupTestDemographyTable(SqlConnection con, ICheckNotifier notifier)
        {
            _testDemography = new TestDemography();

            string sql = _testDemography.GetCreateDemographyTableSql(implementAnonymisationIntoTestConfiguration, TestTableName);

            PreviewAndExecuteSQL("Create Test Demography", sql, con, notifier);

            sql = _testDemography.GetINSERTIntoDemographyTableSql(implementAnonymisationIntoTestConfiguration,TestTableName);

            PreviewAndExecuteSQL("Populate Test Demography", sql, con, notifier);

            return true;
        }

        private bool SetupTestHospitalAdmissionsTable(SqlConnection con, ICheckNotifier notifier)
        {
            
            TestHospitalAdmissions testHospitalAdmissions;

            try
            {

                testHospitalAdmissions = new TestHospitalAdmissions(_testDemography, new Random());
                notifier.OnCheckPerformed(new CheckEventArgs("Created test hospital admissions data (in memory)", CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Crashed setting up test hospital admissions data (in memory)",CheckResult.Fail, e));
                return false;
            }

            string sql = testHospitalAdmissions.GetCreateHospitalAdmissions(implementAnonymisationIntoTestConfiguration);

            if (!PreviewAndExecuteSQL("Create Test Hospital Admissions", sql, con, notifier))
                return false;

            sql = testHospitalAdmissions.GetINSERTIntoHospitalAdmissions(implementAnonymisationIntoTestConfiguration);
            if (!PreviewAndExecuteSQL("Populate Test Hospital Admissions", sql, con, notifier))
                return false;

            sql = testHospitalAdmissions.GetCreateLookupsSql();
            if (!PreviewAndExecuteSQL("Create lookup for codes", sql, con, notifier))
                return false;

            sql = testHospitalAdmissions.GetINSERTLookupsSql();
            if (!PreviewAndExecuteSQL("Populate lookup for codes", sql, con, notifier))
                return false;


            
            return true;

        }

        private bool AddDuplicateResolutionMutilator(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            //"LoadModules.Generic.Mutilators.PrimaryKeyCollisionResolverMutilation"
            //see if process task already exists
            IProcessTask duplicationProcessTask = _loadMetaData.ProcessTasks.SingleOrDefault(pt => pt.Name.Equals(ResolvePrimaryKeyDuplicationProcessName));

            //it does exist so warn user
            if (duplicationProcessTask != null)
                notifier.OnCheckPerformed(new CheckEventArgs(ResolvePrimaryKeyDuplicationProcessName + " Process Task already exists so will not create it ", CheckResult.Warning,null));
            else
                try
                {
                    //it does not exist yet so create it
                    duplicationProcessTask = new ProcessTask(repository, _loadMetaData, LoadStage.AdjustRaw)
                    {
                        Name = ResolvePrimaryKeyDuplicationProcessName,
                        Path = typeof (PrimaryKeyCollisionResolverMutilation).FullName,
                        ProcessTaskType = ProcessTaskType.MutilateDataTable
                    };

                    duplicationProcessTask.SaveToDatabase();
                    notifier.OnCheckPerformed(new CheckEventArgs("Created new ProcessTask ("+duplicationProcessTask.Path+") called " + duplicationProcessTask.Name, CheckResult.Success));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to create new ProcessTask (" + duplicationProcessTask.Path + ") called " + duplicationProcessTask.Name, CheckResult.Fail, e));
                    return false;
                }
            
            try
            {
                //now since all the process task arguments are driven by reflection we need to determine what the class expects and create a ProcessTaskArgument for each DemandsInitialization property
                var created = ProcessTaskArgument.CreateArgumentsForClassIfNotExists<PrimaryKeyCollisionResolverMutilation>(duplicationProcessTask).ToArray();

                //if we created some
                if (created.Count() != 0)
                    notifier.OnCheckPerformed(new CheckEventArgs("Created Arguments:" + created.Aggregate("",(s,n)=>s + n.Name +",") + "  for " + duplicationProcessTask.Name, CheckResult.Success,null));
                else
                {
                    //we didnt create some, its fine they probably were already there or something
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Tried to create Arguments for "+ duplicationProcessTask.Name+" but none were created, possibly they already exist",
                        CheckResult.Warning, null));
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to create ProcessTaskArguments for " + duplicationProcessTask.Name, CheckResult.Fail, e));
                return false;
            }




            //now set the values for each of the properties that we expect to have been created
            try
            {
                var arguments = duplicationProcessTask.GetAllArguments().ToArray();

                //make sure these exist 
                if (!arguments.Any(a => a.Name.Equals("TargetTable")))
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Expected ProcessTaskArgument called TargetTable to exist but it did not (Has someone refactored" +typeof (PrimaryKeyCollisionResolverMutilation).Name+"?)", CheckResult.Fail, null));
                
                //set their values
                foreach (IArgument argument in arguments)
                {
                    try
                    {
                        switch (argument.Name)
                        {
                            case "TargetTable":
                                argument.SetValue(_demographyTableInfo);
                                break;
                         default :
                                notifier.OnCheckPerformed(new CheckEventArgs(
                                    "Unknown property " + argument.Name +
                                    " found on " + typeof(PrimaryKeyCollisionResolverMutilation).Name + ", not sure what value to put in it for tests so leaving it blank",
                                    CheckResult.Warning, null));

                                break;
                        }

                        argument.SaveToDatabase();
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred setting the value of ProcessTaskArgument for Property (of " + typeof(PrimaryKeyCollisionResolverMutilation).Name + ")", CheckResult.Fail, e));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred getting CustomProcessTaskArguments", CheckResult.Fail, e));
                return false;
            }

            

        }

        private const string anoIdentifierTable = "ANOCHITest";

        private bool ConfigureAnANOColumn(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            if (!implementAnonymisationIntoTestConfiguration)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Skipping anonymisation configuration because no anonymisation servers were selected", CheckResult.Warning, null));
                return true;
            }

            var columnInfo = _demographyColumnInfos.SingleOrDefault(info => info.GetRuntimeName() == "ANOCHI");

            if (columnInfo == null)
                throw new Exception("Could not find ColumnInfo named ANOCHI, because implementAnonymisationIntoTestConfiguration is 'true' so it should have been created already");

            #region cleanup Old remenants to ANOTables/Configurations etc
            if (anoIdentifierTable.Equals("ANOCHI"))
                throw new Exception("Oops dont nuke this table name");

            ANOTable toCleanup = repository.GetAllObjects<ANOTable>("WHERE TableName = '" + anoIdentifierTable+"'").SingleOrDefault();
            
            if(toCleanup != null)
            {

                notifier.OnCheckPerformed(new CheckEventArgs("Found remnant ANOTable" + toCleanup, CheckResult.Success, null));
            
                try
                {
                    toCleanup.DeleteInDatabase();
                    notifier.OnCheckPerformed(new CheckEventArgs("Remnant successfully deleted" + toCleanup, CheckResult.Success, null));
                }
                catch (Exception e)
                {
                    var tbl = toCleanup.GetPushedTable();
                    
                    //Maybe we couldn't delete it because it had rows in it?
                    if (tbl.Exists() && tbl.GetRowCount() > 0)
                    {
                        
                        tbl.Truncate();

                        //should now be possible to delete remannt
                        toCleanup.DeleteInDatabase();
                        notifier.OnCheckPerformed(new CheckEventArgs("Remnant successfully deleted (but only after truncating table " + anoIdentifierTable+")" + toCleanup, CheckResult.Warning, null));
                    }
                    else
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not delete remnant ANOTable " + toCleanup,CheckResult.Fail,e));
                        notifier.OnCheckPerformed(new CheckEventArgs("Calling ANOTable.Check() to help you diagnose the problem:" + toCleanup, CheckResult.Fail, e));
                        toCleanup.Check(notifier);

                        return false;
                    }
                }
            }
            notifier.OnCheckPerformed(new CheckEventArgs("successfully removed old remnant table" + anoIdentifierTable +" in ANO database " + _anoServer.Name,CheckResult.Success, null));
            
            #endregion

            _anoTable = new ANOTable(repository, _anoServer, anoIdentifierTable, "A");
            _anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 10;
            _anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 0;
            _anoTable.SaveToDatabase();
            
            _anoTable.PushToANOServerAsNewTable("varchar(10)",notifier);

            columnInfo.ANOTable_ID = _anoTable.ID;
            columnInfo.SaveToDatabase();

            notifier.OnCheckPerformed(new CheckEventArgs("About to run anonymisation checker", CheckResult.Success, null));

            _demographyTableInfo.IdentifierDumpServer_ID = _identifierDumpServer.ID;
            _demographyTableInfo.SaveToDatabase();

            AnonymisationChecks checker = new AnonymisationChecks(_demographyTableInfo);
            checker.Check(notifier);
            
            return true;
        }

        private bool SetupAnonymisationServers(ICheckNotifier notifier)
        {
            if (!implementAnonymisationIntoTestConfiguration)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Skipping anonymisation configuration because no anonymisation servers were selected",CheckResult.Warning, null));
                return true;
            }
            
            _anoServer = SetupExternalServer(ANOIdentifiers,notifier);
            _identifierDumpServer = SetupExternalServer(IdentifierDump, notifier);
            
            return true;

        }

        private bool SetupLogging(ICheckNotifier notifier)
        {
            //user did not choose to setup logging
            if (string.IsNullOrWhiteSpace(_loggingTask))
            {
                //give fail message but proceede anyway 
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "No logging task specified, skipping logging configuration (This is not fatal problem, you can still configure logging for the Catalogue manually",
                    CheckResult.Fail, null));
                return true;
            }

            
            _loggingServer = SetupExternalServer(LoggingServer, notifier);

            if (_loggingServer == null)
                return false;

            //user does not want to recreate it but it has the correct name so we have to use it as the correct logging server
            DemographyCatalogue.LiveLoggingServer_ID = _loggingServer.ID;
            DemographyCatalogue.TestLoggingServer_ID = _loggingServer.ID;//just use the same logging server for live and tests
            DemographyCatalogue.LoggingDataTask = _loggingTask;
            DemographyCatalogue.SaveToDatabase();
            
            
            //if there are not currently any default logging servers, set them to the one we just created.
            ServerDefaults defaults = new ServerDefaults(RepositoryLocator.CatalogueRepository);

            if (defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID) == null)
                defaults.SetDefault(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID,_loggingServer);

            if (defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.TestLoggingServer_ID) == null)
                defaults.SetDefault(ServerDefaults.PermissableDefaults.TestLoggingServer_ID, _loggingServer);

            return true;

        }

        private ExternalDatabaseServer SetupExternalServer(DiscoveredDatabase discoveredDatabase, ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            //decide what the servers name will be called once it is imported into the Catalogue database as an ExternalDatabaseServer
            if (!externalDatabaseNamesDictionary.ContainsKey(discoveredDatabase))
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not find name setup for server " + discoveredDatabase +
                    " (bad programming? why would this be missing from dictionary?", CheckResult.Fail, null));
                return null;
            }

            string nameForServer = externalDatabaseNamesDictionary[discoveredDatabase];

            //Interrogate the data catalogue to see what existing servers it can find
            try
            {
                //see if there is already a reference to a test logging server (could have incorrect settings in it)
                var externalDatabaseServer = repository.GetAllObjectsWhere<ExternalDatabaseServer>("WHERE Name = @Name", new Dictionary<string, object>
                {
                    {"Name", nameForServer}
                }).SingleOrDefault();

                //if it does already exist prompt user to recreate it
                if (externalDatabaseServer != null)
                {

                                        //is it identical?
                    if (IsSameDatabase(externalDatabaseServer,discoveredDatabase.Server.Name,discoveredDatabase.GetRuntimeName()))
                        return externalDatabaseServer;
                    
                    //remove existing dependencies
                    IEnumerable<ANOTable> anoTableDependencies = repository.GetAllObjects<ANOTable>().Where(a => a.Server_ID == externalDatabaseServer.ID);

                    foreach (ANOTable dependency in anoTableDependencies)
                    {
                        if(dependency.TableName.Equals(anoIdentifierTable))
                        {
                            foreach (ColumnInfo columnInfo in repository.GetAllObjects<ColumnInfo>().Where(c => c.ANOTable_ID == dependency.ID))
                            {
                                //clear it's anotable
                                columnInfo.ANOTable_ID = null;
                                columnInfo.SaveToDatabase();
                            }

                            //delete the anotable
                            dependency.DeleteInDatabase();
                        }
                        else
                        {
                            notifier.OnCheckPerformed(new CheckEventArgs(
                                "Failed to delete reference to external server " + externalDatabaseServer.Name +
                                " because of dependency on ANOTable " + dependency.TableName +
                                " (we were reluctant to delete the dependency because it's name did not match expectations)",
                                CheckResult.Fail, null));
                        }
                    }

                    var cataloguesUsingServer = repository.GetAllCatalogues(true)
                        .Where(
                            c =>
                                c.LiveLoggingServer_ID == externalDatabaseServer.ID ||
                                c.TestLoggingServer_ID == externalDatabaseServer.ID).ToArray();

                    foreach (Catalogue user in cataloguesUsingServer)
                    {
                        //if it is one we created! then it is either a hangover from previous execution or there is a default on the live/test server field
                        if (user.Name.Equals(TestHospitalAdmissions.HospitalAdmissionsTableName)  || user.Name.Equals(CatalogueName))
                        {

                            user.LiveLoggingServer_ID = null;
                            user.TestLoggingServer_ID = null;
                            user.SaveToDatabase();
                        }
                        else
                            notifier.OnCheckPerformed(new CheckEventArgs(
                                "Unable to delete ExternalLoggingServer because Catalogue " + user +
                                " uses it for logging", CheckResult.Fail, null));
                    }
                        
                    externalDatabaseServer.DeleteInDatabase();//user wants to recreate it so delete it and drop through
                    
                }

                //There isn't a matching server with the exact name but there could still be an existing server known to go to Servername/DatabaseName but with a different title e.g. if they have already setup a External called 'MyTestLoggingDatabase' or something
                var externalDatabaseServers = repository.GetAllObjects<ExternalDatabaseServer>().ToArray();

                //if there are external servers already configured
                if (externalDatabaseServers.Length != 0)
                {
                    //see if there are any that match the server/database
                    var correctServer = externalDatabaseServers.Where(s => IsSameDatabase(s,discoveredDatabase.Server.Name, discoveredDatabase.GetRuntimeName())).ToArray();

                    //there are two or more external references e.g. JANUS,HICSSISLogging (with integrated security) and then JANUS,HICSSISLogging with user account - at any rate the user has plenty to choose from himself!
                    if (correctServer.Length > 1)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Found " + correctServer.Length + " servers with the servername " + discoveredDatabase.Server + " and database " + discoveredDatabase.GetRuntimeName() + ", did not know which one to use for the logging task of this Catalogue, you will have to configure logging yourself manually", CheckResult.Fail, null));
                        return null;
                    }

                    if (correctServer.Length == 1)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Found existing ExternalDatabaseServer reference to server Server:" + discoveredDatabase +
                            " Database:" + discoveredDatabase  + " so using it",
                            CheckResult.Success, null));
                        return correctServer.Single();
                    }

                    if (correctServer.Length == 0)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Found " + externalDatabaseServers.Length +
                            " ExternalDatabaseServer(s) but it was not the server specified so we will create a new external reference",
                            CheckResult.Warning, null));
                    }
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to check existing ExternalDatabaseServer list", CheckResult.Fail, e));
                return null;
            }

            //if we have not returned by here then we are required to create the external reference

            try
            {
                //Create new external server
                var newServer = new ExternalDatabaseServer(repository, nameForServer)
                {
                    Server = discoveredDatabase.Server.Name,
                    Database = discoveredDatabase.GetRuntimeName()
                };

                //populate and save details

                SqlConnectionStringBuilder builder = buildersDictionary[discoveredDatabase];
                if (!string.IsNullOrWhiteSpace(builder.UserID))
                {
                    newServer.Username = builder.UserID;
                    newServer.Password = builder.Password;
                }

                newServer.SaveToDatabase();

                //tell user 
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "successfully created new ExternalDatabaseServer reference to " + discoveredDatabase + " and called it '" + newServer.Name + "'", CheckResult.Success, null));

                _serversCreated.Add(newServer);
                return newServer;
            }
            catch (Exception e)
            {
                //failed to create external reference so abort all future stuff
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Failed to create new ExternalDatabaseServer reference to Server:" + discoveredDatabase, CheckResult.Fail, e));
                return null;
            }
        }

        private bool SetupPrimaryKeyConflictResolution(ICheckNotifier notifier)
        {
            try
            {
                _demographyColumnInfos.Single(c => c.GetRuntimeName().Equals("DateOfBirth")).DuplicateRecordResolutionOrder = 1;
                _demographyColumnInfos.Single(c => c.GetRuntimeName().Equals("DateOfBirth")).DuplicateRecordResolutionIsAscending = false; //sort descending to get the most recent record

                _demographyColumnInfos.Single(c => c.GetRuntimeName().Equals("Gender")).DuplicateRecordResolutionOrder = 2;
                _demographyColumnInfos.Single(c => c.GetRuntimeName().Equals("Gender")).DuplicateRecordResolutionIsAscending = false; //sort descending to get the most recent record

                _demographyColumnInfos.Single(c => c.GetRuntimeName().Equals("Forename")).DuplicateRecordResolutionOrder = 3;
                _demographyColumnInfos.Single(c => c.GetRuntimeName().Equals("Surname")).DuplicateRecordResolutionOrder = 4;
                
                //do not add CHI to duplication order as it is the primary key on the dataset and therefore is never used in the order list

                foreach (ColumnInfo columnInfo in _demographyColumnInfos)
                    columnInfo.SaveToDatabase();

                notifier.OnCheckPerformed(new CheckEventArgs("Set DuplicateRecordResolutionOrder for ColumnInfos to DateOfBirth->Gender->Forename->Surname", CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to set DuplicateRecordResolutionOrder for ColumnInfos",CheckResult.Fail, e));
            }

            try
            {
                _loadMetaData.SaveToDatabase();
                notifier.OnCheckPerformed(new CheckEventArgs("Turned on EnablePrimaryKeyDuplicationResolution on LoadMetadata", CheckResult.Success,null));
                return true;

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to turn on EnablePrimaryKeyDuplicationResolution on LoadMetadata",CheckResult.Fail, e));
                return false;
            }
        }

        private bool CreateTestCSVFileInForLoading(ICheckNotifier notifier)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Path.Combine(_hicProjectDirectory.ForLoading.FullName,CSVTestFileName));
                _testDemography.CreateTestCSVFile(sw);
                sw.Flush();
                sw.Close();
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Created test CSV file at " + _hicProjectDirectory.ForLoading.FullName +
                    " (contains duplication and some blank lines for fun)", CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Failed to create test CSV file at " + _hicProjectDirectory.ForLoading.FullName, CheckResult.Fail, e));
                return false;
            }


            try
            {
                _loadMetaData.LocationOfFlatFiles = _hicProjectDirectory.RootPath.FullName;
                _loadMetaData.SaveToDatabase();
                notifier.OnCheckPerformed(new CheckEventArgs("Set LoadMetadata Root folder to HICProjectDirectory:" + _hicProjectDirectory.RootPath.FullName,CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to set LoadMetadata Root folder to HICProjectDirectory",CheckResult.Fail, e));
            }


            return true;
        }

        private bool SetupTestLoadProcessAndArgumentsForCSVAttacher(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            //see if process task already exists
            IProcessTask csvProcessTask = _loadMetaData.ProcessTasks.SingleOrDefault(pt => pt.Name.Equals(CSVAttacherProcessName));

            //it does exist so warn user
            if (csvProcessTask != null)
                notifier.OnCheckPerformed(new CheckEventArgs(CSVAttacherProcessName + " Process Task already exists so will not create it ", CheckResult.Warning,null));
            else
                try
                {
                    //it does not exist yet so create it
                    csvProcessTask = new ProcessTask(repository, _loadMetaData, LoadStage.Mounting)
                    {
                        Name = CSVAttacherProcessName,
                        Path = typeof (AnySeparatorFileAttacher).FullName,
                        ProcessTaskType = ProcessTaskType.Attacher
                    };

                    csvProcessTask.SaveToDatabase();
                    notifier.OnCheckPerformed(new CheckEventArgs("Created new ProcessTask (AnySeparatorFileAttacher) called " + CSVAttacherProcessName, CheckResult.Success, null));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to create new ProcessTask (AnySeparatorFileAttacher) called " + CSVAttacherProcessName, CheckResult.Fail, e));
                    return false;
                }
            
            try
            {
                //now since all the process task arguments are driven by reflection we need to determine what the class expects and create a ProcessTaskArgument for each DemandsInitialization property
                var createdArguments = _argumentFactory.CreateArgumentsForClassIfNotExistsGeneric<AnySeparatorFileAttacher>(csvProcessTask, csvProcessTask.GetAllArguments().ToArray()).ToArray();

                //if we created some
                if (createdArguments.Count() != 0)
                    notifier.OnCheckPerformed(new CheckEventArgs("Created Arguments:" + createdArguments.Aggregate("", (s, n) => s + n.Name + ",") + "  for AnySeparatorFileAttacher", CheckResult.Success, null));
                else
                {
                    //we didnt create some, its fine they probably were already there or something
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Tried to create Arguments for AnySeparatorFileAttacher but none were created, possibly they already exist",
                        CheckResult.Warning, null));
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to create ProcessTaskArguments for type AnySeparatorFileAttacher", CheckResult.Fail, e));
                return false;
            }




            //now set the values for each of the properties that we expect to have been created
            try
            {
                var arguments = csvProcessTask.GetAllArguments().ToArray();

                //make sure these exist 
                if (!arguments.Any(a => a.Name.Equals("TableName")))
                    notifier.OnCheckPerformed(new CheckEventArgs("Expected ProcessTaskArgument called TableName to exist but it did not (Has someone refactored AnySeparatorFileAttacher?)", CheckResult.Fail, null));
                if (!arguments.Any(a => a.Name.Equals("FilePattern")))
                    notifier.OnCheckPerformed(new CheckEventArgs("Expected ProcessTaskArgument called FilePattern to exist but it did not (Has someone refactored AnySeparatorFileAttacher?)", CheckResult.Fail, null));

                //set their values
                foreach (ProcessTaskArgument argument in arguments)
                {
                    try
                    {
                        switch (argument.Name)
                        {
                            case "TableName":
                                argument.Value = TestTableName;
                                break;
                            case "FilePattern":
                                argument.Value = CSVTestFileName;
                                break;
                            case "ForceHeaders":
                                argument.Value = null;
                                break;
                            case "SendLoadNotRequiredIfFileNotFound":
                                argument.Value = "True";
                                break;
                            case "UnderReadBehaviour":
                                argument.Value = "AppendNextLineToCurrentRow";
                                break;
                            case "IgnoreQuotes":
                                argument.Value = "False";
                                break;
                            case "IgnoreBlankLines":
                                argument.Value = "True";
                                break;
                            case "ForceHeadersReplacesFirstLineInFile":
                                argument.Value = null;
                                break;
                            case "Separator":
                                argument.Value = ",";
                                break;
                            default :
                                notifier.OnCheckPerformed(new CheckEventArgs(
                                    "Unknown property " + argument.Name +
                                    " found on AnySeparatorFileAttacher, not sure what value to put in it for tests so leaving it blank",
                                    CheckResult.Warning, null));

                                break;
                        }

                        argument.SaveToDatabase();
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred setting the value of ProcessTaskArgument for Property (of AnySeparatorFileAttacher)", CheckResult.Fail, e));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred getting CustomProcessTaskArguments", CheckResult.Fail, e));
                return false;
            }
        }

        private bool CreateHICProjectDirectory(ICheckNotifier notifier)
        {
            //Decide where the project/dataset folder should be (with the root folder)
            DirectoryInfo projectFolder = new DirectoryInfo(Path.Combine(_datasetFolderPath,CatalogueName));

            //if it exists
            if(projectFolder.Exists)
            {
                //tell user it exists
                bool nukeProjectFolder = notifier.OnCheckPerformed(new CheckEventArgs("HICProjectDirectory dataset folder " + projectFolder.FullName+ " already exists", CheckResult.Warning, null,"Delete and recreate folder '" + projectFolder.FullName +"'"));

                if (nukeProjectFolder)
                    try
                    {
                        projectFolder.Delete(true); //delete it and let the method progress (to create again)
                    } catch (Exception e2)
                    {
                        //even the delete failed! - wow this is one messed up environment!
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not delete folder " + projectFolder.FullName, CheckResult.Fail, e2));
                        return false;
                    }
            }

            //folder does not exist or we just nuked it
            try
            {
                //folder does not exist so try to create it
                _hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(projectFolder.Parent, CatalogueName);
                notifier.OnCheckPerformed(new CheckEventArgs("successfully created HICProjectDirectory with RootPath:'" + _hicProjectDirectory.RootPath.FullName +"' ", CheckResult.Success, null));
                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to create HICProjectDirectory in folder:'" + projectFolder.Parent + "' ", CheckResult.Fail, e));
                return false;
            }
        }

        private bool CheckDatasetFolderPathExists(ICheckNotifier notifier)
        {
            DirectoryInfo target;
            try
            {

                target = new DirectoryInfo(_datasetFolderPath);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred when trying to connect to dataset folder '" + _datasetFolderPath + "'",CheckResult.Fail, e));
                return false;
            }

            if (target.Exists)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Confirmed dataset folder '" + _datasetFolderPath + "' exists",CheckResult.Success, null));
                return true;
            }
            
            notifier.OnCheckPerformed(new CheckEventArgs("Dataset folder '" + _datasetFolderPath + "' does not exist", CheckResult.Fail, null));
            return false;
        }

        private bool CreateLoadMetadata(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;

            //see if load metadata already exists
            try
            {
                _loadMetaData = repository.GetAllObjects<LoadMetadata>().SingleOrDefault(lmd => lmd.Name.Equals(LoadMetaDataName));
            }
            catch (Exception exception)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to retrieve LoadMetaDatas from Data Catalogue (We wanted to check to see if LoadMetaData called " + LoadMetaDataName + " already existed)", CheckResult.Fail, exception));
                return false;
            }

            //give user the option to delete the LoadMetadata again
            if(_loadMetaData != null)
                try
                {
                    //disassociate load
                    DemographyCatalogue.LoadMetadata_ID = null;
                    DemographyCatalogue.SaveToDatabase();

                    //delete load
                    _loadMetaData.DeleteInDatabase();
                    _loadMetaData = null;
                    notifier.OnCheckPerformed(new CheckEventArgs("Deleted LoadMetadata " + LoadMetaDataName + " ready for re-creation", CheckResult.Success, null));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to delete LoadMetadata", CheckResult.Fail, e));
                    return false;
                }
                

            //load metadata does not already exist, create it
            if (_loadMetaData == null)
            {
                try
                {
                    _loadMetaData = new LoadMetadata(repository)
                    {
                        Name = LoadMetaDataName,
                        Description =
                            "This is an example data load created by the Diagnostics screen of the CatalogueManager for use with the Catalogue " +
                            CatalogueName,
                        CacheArchiveType = CacheArchiveType.Zip
                    };

                    _loadMetaData.SaveToDatabase();
                    notifier.OnCheckPerformed(new CheckEventArgs("Created new LoadMetaData called " + LoadMetaDataName, CheckResult.Success));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to create LoadMetaData called " + LoadMetaDataName,
                        CheckResult.Fail, e));
                    return false;
                }
            }
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "LoadMetaData called " + LoadMetaDataName + " already existed so skipping create",
                    CheckResult.Warning));

            //associate load metadata with catalogue if it isn't already
            if(DemographyCatalogue.LoadMetadata_ID != _loadMetaData.ID)
            {
                DemographyCatalogue.LoadMetadata_ID = _loadMetaData.ID;
                DemographyCatalogue.SaveToDatabase();
                notifier.OnCheckPerformed(new CheckEventArgs(
             "LoadMetaData " + LoadMetaDataName + " associated with Catalogue " + CatalogueName,
             CheckResult.Warning));

            }

            return true;
        }

        private bool CreateDemographyCatalogue(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            try
            {
                DemographyCatalogue = repository.GetAllCatalogues().SingleOrDefault(cata => cata.Name.Equals(CatalogueName));
                
                if (DemographyCatalogue != null)
                {
                    DemographyCatalogue.DeleteInDatabase();

                    //also delete old reference to it
                    try
                    {
                        foreach (var dataSet in RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableDataSet>()
                                .Where(e => e.Catalogue_ID == DemographyCatalogue.ID))
                            dataSet.DeleteInDatabase();
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not delete old ExtractableDataset which referenced the old Catalogue (this will likely leave a reference to 'Catalogue Deleted' in your DataExportManager application", CheckResult.Warning, e));
                    }

                }

                //there was no existing catalogue or we opted to delete and recreate catalogue
                DemographyCatalogue = new Catalogue(repository, CatalogueName);

                notifier.OnCheckPerformed(new CheckEventArgs("Created Catalogue " + DemographyCatalogue.Name, CheckResult.Success));

                var catalogueItems = DemographyCatalogue.CatalogueItems;
                int order = 0;
                //create catalogueitems
                foreach (ColumnInfo col in _demographyColumnInfos)
                {
                    if (catalogueItems.Any(c => c.Name.Equals(col.GetRuntimeName())))
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Skipped creating CatalogueItem for ColumnInfo " + col.GetRuntimeName() +
                            " because it already existed", CheckResult.Warning, null));
                        continue;
                    }

                    var cataItem = new CatalogueItem(repository, DemographyCatalogue, col.GetRuntimeName());
                    cataItem.SetColumnInfo(col);

                    ExtractionInformation extractionInformation = new ExtractionInformation(repository, cataItem, col, col.Name)
                    {
                        Order = order
                    };
                    extractionInformation.SaveToDatabase();
                    
                    //set the descriptions of the catalogueItem
                    switch (cataItem.Name)
                    {
                        case "CHI":
                            cataItem.Description = TestPerson.CHIDescription;
                            extractionInformation.IsExtractionIdentifier = true;
                            break;
                        case "ANOCHI":
                            cataItem.Description = TestPerson.ANOCHIDescription;
                            extractionInformation.IsExtractionIdentifier = true;
                            break;
                        case "DateOfBirth":
                            cataItem.Description = TestPerson.DateOfBirthDescription;
                             
                            //set the time coverage field
                            DemographyCatalogue.TimeCoverage_ExtractionInformation_ID = extractionInformation.ID;
                            DemographyCatalogue.SaveToDatabase();

                            break;
                        case "Forename":
                            cataItem.Description = TestPerson.ForenameDescription;
                            break;
                        case "Surname":
                            cataItem.Description = TestPerson.SurnameDescription;
                            break;
                        case "Gender":
                            cataItem.Description = TestPerson.GenderDescription;
                            break;
                        default:
                            throw new Exception("did not know what description to use for catalogueitem called " + cataItem.Name);
                    }
                    cataItem.SaveToDatabase();//save the description

                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Created CatalogueItem for ColumnInfo " + col.GetRuntimeName() , CheckResult.Success, null));
                }
                order++;

                DemographyCatalogue.Description = TestDemography.DatasetDescription;
                DemographyCatalogue.Acronym = "TEST";
                DemographyCatalogue.SaveToDatabase();

                notifier.OnCheckPerformed(new CheckEventArgs("Populated Catalogue description", CheckResult.Success, null));
                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to Create Catalogue and CatalogueItems or populated descriptions", CheckResult.Fail, e));
                return false;
            }

        }


        private bool ImportTestTableIntoCatalogue(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            //see if it is already in database
            try
            {
                _demographyTableInfo = repository.GetAllObjects<TableInfo>().FirstOrDefault(t => t.GetRuntimeName().Equals(TestTableName));
                if (_demographyTableInfo != null)
                {
                    bool delete = notifier.OnCheckPerformed(new CheckEventArgs("Found TableInfo already in Catalogue, Do you want to delete and reimport the TableInfo?",CheckResult.Warning, null,"Delete and Recreate TableInfo?"));
                    if(delete)
                    {
                        foreach (var preLoadDiscardedColumn in _demographyTableInfo.PreLoadDiscardedColumns)
                            preLoadDiscardedColumn.DeleteInDatabase();
                        
                        _demographyTableInfo.DeleteInDatabase();
                    }
                    else
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("User chose not to reimport TableInfo even though it might have mismatched or dodgy columns", CheckResult.Warning, null));
                        _demographyColumnInfos = _demographyTableInfo.ColumnInfos.ToArray();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Failed to get TableInfo/ColumnInfo list from Catalogue (we were trying to see if the TableInfo already existed)",
                    CheckResult.Fail, e));
                return false;
            }

            //import table as TableInfo
            try
            {

                TableInfo tableInfo;
                ColumnInfo[] columnInfos;

                _serverToCreateRawDataOn.InitialCatalog = TestDatabase;
                TableInfoImporter importer = new TableInfoImporter(repository, _serverToCreateRawDataOn.DataSource, TestDatabase, TestTableName, DatabaseType.MicrosoftSQLServer, username: _serverToCreateRawDataOn.UserID, password: _serverToCreateRawDataOn.Password, usageContext: DataAccessContext.Any);
                importer.DoImport(out tableInfo,out columnInfos); 
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Table successfully imported into Data Catalogue)", CheckResult.Success,null));
                _demographyTableInfo = tableInfo;
                _demographyColumnInfos = columnInfos;
                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Exception occurred during import of the test table '" + TestTableName +
                    "' into Data Catalogue (ConnectionString=" + repository.ConnectionString +
                    ")", CheckResult.Fail, e));
                return false;
            }
        }


        private bool SetupConnectionAndDatabase(SqlConnection con, ICheckNotifier notifier)
        {
            try
            {
                con.Open();
                notifier.OnCheckPerformed(new CheckEventArgs("Connected to data server " + _serverToCreateRawDataOn, CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not connect to data server " + _serverToCreateRawDataOn + " using connection string: " +
                    _serverToCreateRawDataOn.ConnectionString, CheckResult.Fail, e));
                return false;
            }

            try
            {
                var db = _discoveredServerToCreateRawDataOn.ExpectDatabase(TestDatabase);
                
                if(db.Exists())
                {

                    bool drop = notifier.OnCheckPerformed(new CheckEventArgs("Database " + TestDatabase + " already exists",CheckResult.Warning,null,"Drop and recreate " + TestDatabase));

                    if (drop)
                    {
                        foreach (DiscoveredTable table in db.DiscoverTables(true))
                        {
                            bool dropTable =
                                notifier.OnCheckPerformed(
                                    new CheckEventArgs("Table " + table + " in " + TestDatabase + " must be dropped",
                                        CheckResult.Warning, null, "Drop table " + table + "?"));

                            if (dropTable)
                                table.Drop();
                            else
                                throw new Exception("User chose not to drop table " + table);
                        }

                        db.Drop();
                    }
                    else
                        throw new Exception("User chose not to drop database " + db);
                }

                new DiscoveredServer(_serverToCreateRawDataOn).CreateDatabase(TestDatabase);
                notifier.OnCheckPerformed(new CheckEventArgs("successfully created database " + TestDatabase, CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Create database failed", CheckResult.Fail, e));
            }

            try
            {
                con.ChangeDatabase(TestDatabase);
                notifier.OnCheckPerformed(new CheckEventArgs("Switched connection to " + TestDatabase, CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Switch connection to " + TestDatabase + " failed (did creation fail? or was it skipped?)",
                    CheckResult.Fail, e));
                return false;
            }

            return true;
        }

        public void DestroyEnvironment()
        {
            //drop the endpoint database
            var liveServer = DataAccessPortal.GetInstance().ExpectDatabase(DemographyTableInfo, DataAccessContext.InternalDataProcessing);
            liveServer.Drop();

            var staging = liveServer.Server.ExpectDatabase("DLE_STAGING");
            if(staging.Exists())
                staging.Drop();

            //drop the catalogue entities
            var lmd = DemographyCatalogue.LoadMetadata;
            DemographyCatalogue.DeleteInDatabase();
            lmd.DeleteInDatabase();

            var credentials = (DataAccessCredentials)DemographyTableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            DemographyTableInfo.DeleteInDatabase();

            if(credentials != null)
                credentials.DeleteInDatabase();
            
            //check RAW server
            var defaults = new ServerDefaults(RepositoryLocator.CatalogueRepository);
            var rawServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.RAWDataLoadServer);

            //did we create it?
            if (rawServer != null && _serversCreated.Contains(rawServer))//yes
                defaults.ClearDefault(ServerDefaults.PermissableDefaults.RAWDataLoadServer);//so clear the default we created

            //any servers we created need to be deleted
            foreach (var server in _serversCreated)
                server.DeleteInDatabase();
            
            if(_anoTable != null)
            {
                //drop the ANOTable from the ANO database
                var remoteAnoDatabase = DataAccessPortal.GetInstance().ExpectDatabase(_anoTable.Server, DataAccessContext.InternalDataProcessing);
                var remoteAnoTable = remoteAnoDatabase.ExpectTable(_anoTable.TableName);

                if (remoteAnoDatabase.Exists() && remoteAnoTable.Exists())remoteAnoTable.Drop();
                    _anoTable.DeleteInDatabase();
            }
        }
        public bool IsSameDatabase(ExternalDatabaseServer eds, string server, string database)
        {
            if (!string.IsNullOrWhiteSpace(eds.Server) && eds.Server.Equals(server))
                if (!string.IsNullOrWhiteSpace(eds.Database) && eds.Database.Equals(database))
                    return true;

            return false;
        }

    }
}
