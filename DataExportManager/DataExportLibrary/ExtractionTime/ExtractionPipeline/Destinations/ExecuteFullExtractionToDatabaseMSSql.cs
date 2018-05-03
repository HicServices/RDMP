using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataLoadEngine.DataFlowPipeline.Destinations;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations
{
    /// <summary>
    /// Alternate extraction pipeline destination in which the DataTable containing the extracted dataset is written to an Sql Server database
    /// </summary>
    [Description("The Extraction target for DataExportManager into a Microsoft SQL Database, this should only be used by ExtractionPipelineHost as it is the only class that knows how to correctly call PreInitialize ")]
    public class ExecuteFullExtractionToDatabaseMSSql : IExecuteDatasetExtractionDestination, IPipelineRequirement<IProject>
    {
        [DemandsInitialization("External server to create the extraction into, a new database will be created for the project based on the naming pattern provided",Mandatory = true)]
        public IExternalDatabaseServer TargetDatabaseServer { get; set; }

        [DemandsInitialization(@"How do you want to name datasets, use the following tokens if you need them:   
         $p - Project Name ('e.g. My Project')
         $n - Project Number (e.g. 234)
         $t - Master Ticket (e.g. 'LINK-1234')
         ", Mandatory = true, DefaultValue = "Proj_$n_$t")]
        public string DatabaseNamingPattern { get; set; }

        [DemandsInitialization(@"How do you want to name datasets, use the following tokens if you need them:   
         $p - Project Name ('e.g. My Project')
         $n - Project Number (e.g. 234)
         $c - Configuration Name (e.g. 'Cases')
         $d - Dataset name (e.g. 'Prescribing')
         $a - Dataset acronym (e.g. 'Presc') 

         You must have either $a or $d
         ",Mandatory=true,DefaultValue="$c_$d")]
        public string TableNamingPattern { get; set; }

        [DemandsInitialization(@"If the extraction fails half way through AND the destination table was created during the extraction then the table will be dropped from the destination rather than being left in a half loaded state ",defaultValue:true)]
        public bool DropTableIfLoadFails { get; set; }

        public TableLoadInfo TableLoadInfo { get; private set; }
        public DirectoryInfo DirectoryPopulated { get; private set; }
        public bool GeneratesFiles { get { return false; } }
        public string OutputFile { get { return String.Empty; } }
        public int SeparatorsStrippedOut { get { return 0; } }
        public string DateFormat { get { return String.Empty; } }

        private DiscoveredDatabase _destinationDatabase;
        private DataTableUploadDestination _destination;
        
        private DataLoadInfo _dataLoadInfo;
        private bool haveExtractedBundledContent = false;

        private bool _tableDidNotExistAtStartOfLoad;
        private string _cachedGetTableNameAnswer;
        private IExtractCommand _request;
        private IProject _project;

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_request is ExtractDatasetCommand && !haveExtractedBundledContent)
            {
                var bundle = ((ExtractDatasetCommand)_request).DatasetBundle;
                foreach (var sql in bundle.SupportingSQL)
                    bundle.States[sql] = ExtractSupportingSql(sql, listener);

                foreach (var document in ((ExtractDatasetCommand)_request).DatasetBundle.Documents)
                    bundle.States[document] = ExtractSupportingDocument(_request.GetExtractionDirectory(), document, listener);

                haveExtractedBundledContent = true;
            }
         
            if (_destination == null)
            {
                //see if the user has entered an extraction server/database 
                if (TargetDatabaseServer == null)
                    throw new Exception("TargetDatabaseServer (the place you want to extract the project data to) property has not been set!");
                
                _destinationDatabase = GetDestinationDatabase(listener);
                
                try
                {
                    if (!_destinationDatabase.Exists())
                        _destinationDatabase.Create();
                        //throw new Exception("Could not connect to server " + TargetDatabaseServer.Server);
                    
                    var tblName = GetTableName();

                    //See if table already exists on the server (likely to cause problems including duplication, schema changes in configuration etc)
                    if (_destinationDatabase.ExpectTable(tblName).Exists())
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                "A table called " + tblName + " already exists on server " + TargetDatabaseServer +
                                ", rows will be appended to this table - or data load might crash if it has an incompatible schema"));
                    else
                    {
                        _tableDidNotExistAtStartOfLoad = true;
                    }
                }
                catch (Exception e)
                {
                    //Probably the database didn't exist or the credentials were wrong or something
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Failed to inspect destination for already existing datatables",e));
                }

                _destination = new DataTableUploadDestination();

                PrimeDestinationTypesBasedOnCatalogueTypes();

                _destination.AllowResizingColumnsAtUploadTime = false;
                _destination.PreInitialize(_destinationDatabase, listener);
                
                //Record that we are loading the table (the drop refers to 'rollback advice' in the audit log - don't worry about it)
                TableLoadInfo = new TableLoadInfo(_dataLoadInfo, "Drop table " + GetTableName(), GetTableName(), new DataSource[] { new DataSource(_request.DescribeExtractionImplementation(), DateTime.Now) }, -1);
            }

            //give the data table the correct name
            toProcess.TableName = GetTableName();

            _destination.ProcessPipelineData(toProcess, listener, cancellationToken);
            TableLoadInfo.Inserts += toProcess.Rows.Count;

            return null;
        }

        private void PrimeDestinationTypesBasedOnCatalogueTypes()
        { 
            //if the extraction is of a Catalogue
            var datasetCommand = _request as IExtractDatasetCommand;
            
            if(datasetCommand == null)
                return;

            //for every extractable column in the Catalogue
            foreach (var extractionInformation in datasetCommand.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            {
                var colName = extractionInformation.GetRuntimeName();
                var colInfo = extractionInformation.ColumnInfo;

                if(colInfo == null)
                    continue;

                //Tell the destination the datatype of the ColumnInfo that underlies the ExtractionInformation (this might be changed by the ExtractionInformation e.g. as a 
                //transform but it is a good starting point.  We don't want to create a varchar(10) column in the destination if the origin dataset (Catalogue) is a varchar(100)
                //since it will just confuse the user.  Bear in mind these data types can be degraded later by the destination
                _destination.AddExplicitWriteType(colName,colInfo.Data_type);
            }

            //Also tell the destination about the extraction identifier column name e.g. ReleaseId is a varchar(10).  ReleaseId is not part of the Catalogue, it's part of the Cohort
            _destination.AddExplicitWriteType(
                datasetCommand.ExtractableCohort.GetReleaseIdentifier(true),
            datasetCommand.ExtractableCohort.GetReleaseIdentifierDataType());

        }
        
        public string GetTableName()
        {
            //if there is a cached answer return it
            if (_cachedGetTableNameAnswer != null) return _cachedGetTableNameAnswer;
            
            string tblName = TableNamingPattern;
            var project = _request.Configuration.Project;

            var extractDataset = _request as ExtractDatasetCommand;

            tblName = tblName.Replace("$p", project.Name);
            tblName = tblName.Replace("$n", project.ProjectNumber.ToString());
            tblName = tblName.Replace("$c", _request.Configuration.Name);

            if(extractDataset != null)
            {
                tblName = tblName.Replace("$d", extractDataset.DatasetBundle.DataSet.Catalogue.Name);
                tblName = tblName.Replace("$a", extractDataset.DatasetBundle.DataSet.Catalogue.Acronym);
            }
            else
            {
                tblName = tblName.Replace("$d", _request.ToString());
                tblName = tblName.Replace("$a", _request.ToString());
            }

            if (_destinationDatabase == null)
                throw new Exception("Cannot pick a TableName until we know what type of server it is going to, _server is null");

            //otherwise, fetch and cache answer
            _cachedGetTableNameAnswer = _destinationDatabase.Server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(tblName);

            if(string.IsNullOrWhiteSpace(_cachedGetTableNameAnswer) )
                throw new Exception("TableNamingPattern '" + TableNamingPattern + "' resulted in an empty string for request '" +_request +"'");

            return _cachedGetTableNameAnswer;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if(_destination != null)
            {
                _destination.Dispose(listener,pipelineFailureExceptionIfAny);

                //if the extraction failed, the table didn't exist in the destination (i.e. the table was created during the extraction) and we are to DropTableIfLoadFails
                if (pipelineFailureExceptionIfAny != null && _tableDidNotExistAtStartOfLoad && DropTableIfLoadFails)
                {
                    if(_destinationDatabase != null)
                    {
                        var tbl = _destinationDatabase.ExpectTable(GetTableName());
                        
                        if(tbl.Exists())
                        {
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "DropTableIfLoadFails is true so about to drop table " + tbl));
                            tbl.Drop();
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Dropped table " + tbl));
                        }
                    }
                }
            }
            TableLoadInfo.CloseAndArchive();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            if(_destination != null)
                _destination.Abort(listener);
        }

        public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
        {
            _request = value;

            DirectoryPopulated = _request.GetExtractionDirectory();
        }

        public void PreInitialize(DataLoadInfo value, IDataLoadEventListener listener)
        {
            _dataLoadInfo = value;
        }

        public string GetFilename()
        {
            return _request.Name;
        }

        public string GetDestinationDescription()
        {
            var tblName = GetTableName();
            var dbName = GetDatabaseName();
            return TargetDatabaseServer.ID + "|" + dbName + "|" + tblName;
        }

        public DestinationType GetDestinationType()
        {
            return DestinationType.Database;
        }

        public void ExtractGlobals(Project project, ExtractionConfiguration configuration, GlobalsBundle globalsToExtract, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            if (globalsToExtract.Any())
            {
                ExtractionDirectory targetDirectory = new ExtractionDirectory(project.ExtractionDirectory, configuration);
                DirectoryInfo globalsDirectory = targetDirectory.GetGlobalsDirectory();
                if (_destination == null)
                {
                    //see if the user has entered an extraction server/database 
                    if (TargetDatabaseServer == null)
                        throw new Exception(
                            "TargetDatabaseServer (the place you want to extract the project data to) property has not been set!");

                    _destinationDatabase = GetDestinationDatabase(listener);

                    try
                    {
                        if (!_destinationDatabase.Exists())
                            _destinationDatabase.Create();
                    }
                    catch (Exception e)
                    {
                        //Probably the database didn't exist or the credentials were wrong or something
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Error,
                                "Failed to inspect destination for already existing datatables", e));
                    }
                }

                foreach (var sql in globalsToExtract.SupportingSQL)
                    ExtractSupportingSql(sql, listener);

                foreach (var doc in globalsToExtract.Documents)
                    ExtractSupportingDocument(globalsDirectory, doc, listener);
            }
        }

        public ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration configuration, ExtractableDataSet dataSet)
        {
            return new MsSqlExtractionReleasePotential(repositoryLocator, configuration, dataSet);
        }

        public FixedReleaseSource<ReleaseAudit> GetReleaseSource(CatalogueRepository catalogueRepository)
        {
            return new MsSqlReleaseSource<ReleaseAudit>(catalogueRepository);
        }

        private ExtractCommandState ExtractSupportingSql(SupportingSQLTable sql, IDataLoadEventListener listener)
        {
            try
            {
                var tempDestination = new DataTableUploadDestination();

                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to download SQL for global SupportingSQL " + sql.Name));
                using (var con = sql.GetServer().GetConnection())
                {
                    con.Open();
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Connection opened successfully, about to send SQL command " + sql.SQL));
                    var cmd = DatabaseCommandHelper.GetCommand(sql.SQL, con);
                    var da = DatabaseCommandHelper.GetDataAdapter(cmd);

                    var sw = new Stopwatch();

                    sw.Start();
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dt.TableName = _destinationDatabase.Server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(sql.Name);

                    listener.OnProgress(this, new ProgressEventArgs("Reading from SupportingSQL " + sql.Name, new ProgressMeasurement(dt.Rows.Count, ProgressType.Records), sw.Elapsed));
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Decided on the following destination table name for SupportingSQL:" + dt.TableName));

                    tempDestination.AllowResizingColumnsAtUploadTime = true;
                    tempDestination.PreInitialize(GetDestinationDatabase(listener), listener);
                    tempDestination.ProcessPipelineData(dt, listener, new GracefulCancellationToken());
                    tempDestination.Dispose(listener, null);
                }
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Extraction of SupportingSQL " + sql + " failed ",e));
                return ExtractCommandState.Crashed;
            }

            return ExtractCommandState.Completed;
        }

        private ExtractCommandState ExtractSupportingDocument(DirectoryInfo directory, SupportingDocument doc, IDataLoadEventListener listener)
        {
            SupportingDocumentsFetcher fetcher = new SupportingDocumentsFetcher(null);

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing to copy " + doc + " to directory " + directory.FullName));
            try
            {
                fetcher.ExtractToDirectory(directory, doc);
                return ExtractCommandState.Completed;
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to copy file " + doc + " to directory " + directory.FullName, e));
                return ExtractCommandState.Crashed;
            }
        }
        
        private DiscoveredDatabase GetDestinationDatabase(IDataLoadEventListener listener)
        {
            //tell user we are about to inspect it
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to open connection to " + TargetDatabaseServer));

            var databaseName = GetDatabaseName();

            var discoveredServer = DataAccessPortal.GetInstance().ExpectServer(TargetDatabaseServer, DataAccessContext.DataExport, setInitialDatabase: false);
            
            return discoveredServer.ExpectDatabase(databaseName);
        }

        private string GetDatabaseName()
        {
            string dbName = DatabaseNamingPattern;

            dbName = dbName.Replace("$p", _project.Name)
                           .Replace("$n", _project.ProjectNumber.ToString())
                           .Replace("$t", _project.MasterTicket);

            return dbName;
        }

        public void Check(ICheckNotifier notifier)
        {
            if (TargetDatabaseServer == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Target database server property has not been set (This component does not know where to extract data to!), " +
                                                             "to fix this you must edit the pipeline and choose an ExternalDatabaseServer to extract to)", 
                                                             CheckResult.Fail));
                return;
            }

            if (string.IsNullOrWhiteSpace(TargetDatabaseServer.Server))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("TargetDatabaseServer does not have a .Server specified", CheckResult.Fail));
                return;
            }

            if(!string.IsNullOrWhiteSpace(TargetDatabaseServer.Database))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("TargetDatabaseServer has .Database specified but this will be ignored!", CheckResult.Warning));
            }

            if(string.IsNullOrWhiteSpace(TableNamingPattern))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("You must specify TableNamingPattern, this will tell the component how to name tables it generates in the remote destination",CheckResult.Fail));
                return;
            }

            if (string.IsNullOrWhiteSpace(DatabaseNamingPattern))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("You must specify DatabaseNamingPattern, this will tell the component what database to create or use in the remote destination", CheckResult.Fail));
                return;
            }

            if (!DatabaseNamingPattern.Contains("$p") && !DatabaseNamingPattern.Contains("$n") && !DatabaseNamingPattern.Contains("$t"))
                notifier.OnCheckPerformed(new CheckEventArgs("DatabaseNamingPattern does not contain any token. The tables may be created alongside existing tables and Release would be impossible.", CheckResult.Warning));

            if (!TableNamingPattern.Contains("$d") && !TableNamingPattern.Contains("$a"))
                notifier.OnCheckPerformed(new CheckEventArgs("TableNamingPattern must contain either $d or $a, the name/acronym of the dataset being extracted otherwise you will get collisions when you extract multiple tables at once",CheckResult.Warning));

            if (_request == ExtractDatasetCommand.EmptyCommand)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Request is ExtractDatasetCommand.EmptyCommand, will not try to connect to Database",CheckResult.Warning));
                return;
            }

            try
            {
                var server = DataAccessPortal.GetInstance().ExpectServer(TargetDatabaseServer, DataAccessContext.DataExport, setInitialDatabase: false);
                var database = server.ExpectDatabase(GetDatabaseName());

                if (database.Exists())
                    notifier.OnCheckPerformed(new CheckEventArgs("Confirmed database " + database + " exists", CheckResult.Success));
                else
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Database " + database + " does not exist on server... how were we even able to connect?!", CheckResult.Fail));
                    return;
                }
                
                var tables = database.DiscoverTables(false);

                if (tables.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("The following preexisting tables were found in the database " + string.Join(",",tables.Select(t=>t.ToString())),CheckResult.Warning));
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("Confirmed that database " + database + " is empty of tables", CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not connect to TargetDatabaseServer '" + TargetDatabaseServer  +"'", CheckResult.Fail, e));
            }
        }

        public void PreInitialize(IProject value, IDataLoadEventListener listener)
        {
            this._project = value;
        }
    }
}
