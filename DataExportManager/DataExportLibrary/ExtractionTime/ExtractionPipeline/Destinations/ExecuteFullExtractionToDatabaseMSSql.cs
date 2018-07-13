using System;
using System.Collections.Generic;
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
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;
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
            if (_destination == null)
                _destination = PrepareDestination(listener);

            //give the data table the correct name
            toProcess.TableName = GetTableName();

            //Record that we are loading the table (the drop refers to 'rollback advice' in the audit log - don't worry about it)
            TableLoadInfo = new TableLoadInfo(_dataLoadInfo, "", GetTableName(), new[] { new DataSource(_request.DescribeExtractionImplementation(), DateTime.Now) }, -1);

            if (_request is ExtractDatasetCommand && !haveExtractedBundledContent)
                WriteBundleContents(((ExtractDatasetCommand) _request).DatasetBundle, listener, cancellationToken);

            if (_request is ExtractGlobalsCommand)
            {
                ExtractGlobals((ExtractGlobalsCommand)_request, listener, _dataLoadInfo);
                return null;
            }

            _destination.ProcessPipelineData(toProcess, listener, cancellationToken);
            TableLoadInfo.Inserts += toProcess.Rows.Count;

            return null;
        }

        private void WriteBundleContents(IExtractableDatasetBundle datasetBundle, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var bundle = ((ExtractDatasetCommand)_request).DatasetBundle;
            foreach (var sql in bundle.SupportingSQL)
                bundle.States[sql] = ExtractSupportingSql(sql, listener, _dataLoadInfo);

            foreach (var document in ((ExtractDatasetCommand)_request).DatasetBundle.Documents)
                bundle.States[document] = ExtractSupportingDocument(_request.GetExtractionDirectory(), document, listener);

            //extract lookups
            foreach (BundledLookupTable lookup in datasetBundle.LookupTables)
            {
                try
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to extract lookup " + lookup));

                    ExtractLookupTableSql(lookup, listener, _dataLoadInfo);
                    
                    datasetBundle.States[lookup] = ExtractCommandState.Completed;
                }
                catch (Exception e)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error occurred trying to extract lookup " + lookup + " on server " + lookup.TableInfo.Server, e));

                    datasetBundle.States[lookup] = ExtractCommandState.Crashed;
                }
            }

            haveExtractedBundledContent = true;
        }
        
        private DataTableUploadDestination PrepareDestination(IDataLoadEventListener listener)
        {
            //see if the user has entered an extraction server/database 
            if (TargetDatabaseServer == null)
                throw new Exception("TargetDatabaseServer (the place you want to extract the project data to) property has not been set!");

            _destinationDatabase = GetDestinationDatabase(listener);

            try
            {
                if (!_destinationDatabase.Exists())
                    _destinationDatabase.Create();

                if (_request is ExtractGlobalsCommand)
                    return null;
                
                var tblName = GetTableName();

                //See if table already exists on the server (likely to cause problems including duplication, schema changes in configuration etc)
                if (_destinationDatabase.ExpectTable(tblName).Exists())
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                                            "A table called " + tblName + " already exists on server " + TargetDatabaseServer + 
                                            ", data load might crash if it is populated and/or has an incompatible schema"));
                else
                {
                    _tableDidNotExistAtStartOfLoad = true;
                }
            }
            catch (Exception e)
            {
                //Probably the database didn't exist or the credentials were wrong or something
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to inspect destination for already existing datatables", e));
            }

            _destination = new DataTableUploadDestination();
            
            PrimeDestinationTypesBasedOnCatalogueTypes();

            _destination.AllowResizingColumnsAtUploadTime = true;
            _destination.PreInitialize(_destinationDatabase, listener);

            return _destination;
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

                //if we do not know the data type or the ei is a transform
                if (colInfo == null || extractionInformation.IsProperTransform())
                    continue;

                //Tell the destination the datatype of the ColumnInfo that underlies the ExtractionInformation (this might be changed by the ExtractionInformation e.g. as a 
                //transform but it is a good starting point.  We don't want to create a varchar(10) column in the destination if the origin dataset (Catalogue) is a varchar(100)
                //since it will just confuse the user.  Bear in mind these data types can be degraded later by the destination
                _destination.AddExplicitWriteType(colName,colInfo.Data_type);
            }

            //Also tell the destination about the extraction identifier column name e.g. ReleaseId is a varchar(10).  ReleaseId is not part of the Catalogue, it's part of the Cohort
            _destination.AddExplicitWriteType(datasetCommand.ExtractableCohort.GetReleaseIdentifier(true),
            datasetCommand.ExtractableCohort.GetReleaseIdentifierDataType());
        }
        
        public string GetTableName(string suffix = null)
        {
            string tblName = TableNamingPattern;
            var project = _request.Configuration.Project;
            
            tblName = tblName.Replace("$p", project.Name);
            tblName = tblName.Replace("$n", project.ProjectNumber.ToString());
            tblName = tblName.Replace("$c", _request.Configuration.Name);

            if (_request is ExtractDatasetCommand)
            {
                tblName = tblName.Replace("$d", ((ExtractDatasetCommand)_request).DatasetBundle.DataSet.Catalogue.Name);
                tblName = tblName.Replace("$a", ((ExtractDatasetCommand)_request).DatasetBundle.DataSet.Catalogue.Acronym);
            }

            if (_request is ExtractGlobalsCommand)
            {
                tblName = tblName.Replace("$d", ExtractionDirectory.GLOBALS_DATA_NAME);
                tblName = tblName.Replace("$a", "G");
            }

            if (_destinationDatabase == null)
                throw new Exception("Cannot pick a TableName until we know what type of server it is going to, _server is null");

            //otherwise, fetch and cache answer
            _cachedGetTableNameAnswer = _destinationDatabase.Server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(tblName);

            if(string.IsNullOrWhiteSpace(_cachedGetTableNameAnswer) )
                throw new Exception("TableNamingPattern '" + TableNamingPattern + "' resulted in an empty string for request '" +_request +"'");

            if (!String.IsNullOrWhiteSpace(suffix))
                _cachedGetTableNameAnswer += "_" + suffix;

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
            
            if (TableLoadInfo != null)
                TableLoadInfo.CloseAndArchive();
            // also close off the cumulative extraction result
            if (_request is ExtractDatasetCommand)
            {
                var result = ((IExtractDatasetCommand)_request).CumulativeExtractionResults;
                if (result != null)
                    result.CompleteAudit(this.GetType(), GetDestinationDescription(), TableLoadInfo.Inserts);
            }
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
            return _request.ToString();
        }

        public string GetDestinationDescription()
        {
            return GetDestinationDescription(suffix: "");
        }

        private string GetDestinationDescription(string suffix = "")
        {
            var tblName = GetTableName(suffix);
            var dbName = GetDatabaseName();
            return TargetDatabaseServer.ID + "|" + dbName + "|" + tblName;
        }

        public DestinationType GetDestinationType()
        {
            return DestinationType.Database;
        }
        
        public void ExtractGlobals(ExtractGlobalsCommand request, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            var globalsToExtract = request.Globals;
            if (globalsToExtract.Any())
            {
                var globalsDirectory = request.GetExtractionDirectory();
                
                foreach (var sql in globalsToExtract.SupportingSQL)
                    ExtractSupportingSql(sql, listener, dataLoadInfo);

                foreach (var doc in globalsToExtract.Documents)
                    ExtractSupportingDocument(globalsDirectory, doc, listener);
            }
        }

        public ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSet)
        {
            return new MsSqlExtractionReleasePotential(repositoryLocator, selectedDataSet);
        }

        public FixedReleaseSource<ReleaseAudit> GetReleaseSource(CatalogueRepository catalogueRepository)
        {
            return new MsSqlReleaseSource<ReleaseAudit>(catalogueRepository);
        }

        private ExtractCommandState ExtractSupportingSql(SupportingSQLTable sql, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
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

                    dt.TableName = GetTableName(_destinationDatabase.Server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(sql.Name));

                    var tableLoadInfo = dataLoadInfo.CreateTableLoadInfo("", dt.TableName, new[] { new DataSource(sql.SQL, DateTime.Now) }, -1);
                    tableLoadInfo.Inserts = dt.Rows.Count;

                    listener.OnProgress(this, new ProgressEventArgs("Reading from SupportingSQL " + sql.Name, new ProgressMeasurement(dt.Rows.Count, ProgressType.Records), sw.Elapsed));
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Decided on the following destination table name for SupportingSQL: " + dt.TableName));

                    tempDestination.AllowResizingColumnsAtUploadTime = true;
                    tempDestination.PreInitialize(GetDestinationDatabase(listener), listener);
                    tempDestination.ProcessPipelineData(dt, listener, new GracefulCancellationToken());
                    tempDestination.Dispose(listener, null);

                    //end auditing it
                    tableLoadInfo.CloseAndArchive();

                    if (_request is ExtractDatasetCommand)
                    {
                        var result = (_request as ExtractDatasetCommand).CumulativeExtractionResults;
                        var supplementalResult = result.AddSupplementalExtractionResult(sql.SQL, sql);
                        supplementalResult.CompleteAudit(TargetDatabaseServer.ID + "|" + GetDatabaseName() + "|" + dt.TableName, dt.Rows.Count);
                    }
                    else
                    {
                        var extractGlobalsCommand = (_request as ExtractGlobalsCommand);
                        Debug.Assert(extractGlobalsCommand != null, "extractGlobalsCommand != null");
                        var result =
                            new SupplementalExtractionResults(extractGlobalsCommand.RepositoryLocator.DataExportRepository,
                                                              extractGlobalsCommand.Configuration,
                                                              sql.SQL,
                                                              sql);
                        result.CompleteAudit(TargetDatabaseServer.ID + "|" + GetDatabaseName() + "|" + dt.TableName, dt.Rows.Count);
                        extractGlobalsCommand.ExtractionResults.Add(result);
                    }
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
            SupportingDocumentsFetcher fetcher = new SupportingDocumentsFetcher(doc);

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing to copy " + doc + " to directory " + directory.FullName));
            try
            {
                var outputPath = fetcher.ExtractToDirectory(directory);
                if (_request is ExtractDatasetCommand)
                {
                    var result = (_request as ExtractDatasetCommand).CumulativeExtractionResults;
                    var supplementalResult = result.AddSupplementalExtractionResult(null, doc);
                    supplementalResult.CompleteAudit(outputPath, 0);
                }
                else
                {
                    var extractGlobalsCommand = (_request as ExtractGlobalsCommand);
                    Debug.Assert(extractGlobalsCommand != null, "extractGlobalsCommand != null");
                    var result = new SupplementalExtractionResults(extractGlobalsCommand.RepositoryLocator.DataExportRepository,
                                                                   extractGlobalsCommand.Configuration,
                                                                   null,
                                                                   doc);
                    result.CompleteAudit(outputPath, 0);
                    extractGlobalsCommand.ExtractionResults.Add(result);
                }
                return ExtractCommandState.Completed;
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to copy file " + doc + " to directory " + directory.FullName, e));
                return ExtractCommandState.Crashed;
            }
        }

        private void ExtractLookupTableSql(BundledLookupTable lookup, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            try
            {
                var tempDestination = new DataTableUploadDestination();
                
                var server = DataAccessPortal.GetInstance().ExpectServer(lookup.TableInfo, DataAccessContext.DataExport);
                
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to download SQL for lookup " + lookup.TableInfo.Name));
                using (var con = server.GetConnection())
                {
                    con.Open();
                    var sqlString = "SELECT * FROM " + lookup.TableInfo.Name;
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Connection opened successfully, about to send SQL command: " + sqlString));
                    var cmd = DatabaseCommandHelper.GetCommand(sqlString, con);
                    var da = DatabaseCommandHelper.GetDataAdapter(cmd);

                    var sw = new Stopwatch();

                    sw.Start();
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dt.TableName = GetTableName(_destinationDatabase.Server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(lookup.TableInfo.Name));

                    var tableLoadInfo = dataLoadInfo.CreateTableLoadInfo("", dt.TableName, new[] { new DataSource(sqlString, DateTime.Now) }, -1);
                    tableLoadInfo.Inserts = dt.Rows.Count;

                    listener.OnProgress(this, new ProgressEventArgs("Reading from Lookup " + lookup.TableInfo.Name, new ProgressMeasurement(dt.Rows.Count, ProgressType.Records), sw.Elapsed));
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Decided on the following destination table name for Lookup: " + dt.TableName));

                    tempDestination.AllowResizingColumnsAtUploadTime = true;
                    tempDestination.PreInitialize(GetDestinationDatabase(listener), listener);
                    tempDestination.ProcessPipelineData(dt, listener, new GracefulCancellationToken());
                    tempDestination.Dispose(listener, null);

                    //end auditing it
                    tableLoadInfo.CloseAndArchive();

                    if (_request is ExtractDatasetCommand)
                    {
                        var result = (_request as ExtractDatasetCommand).CumulativeExtractionResults;
                        var supplementalResult = result.AddSupplementalExtractionResult("SELECT * FROM " + lookup.TableInfo.Name, lookup.TableInfo);
                        supplementalResult.CompleteAudit(TargetDatabaseServer.ID + "|" + GetDatabaseName() + "|" + dt.TableName, dt.Rows.Count);
                    }
                }
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Extraction of Lookup " + lookup.TableInfo.Name + " failed ", e));
                throw;
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
                notifier.OnCheckPerformed(new CheckEventArgs("TableNamingPattern must contain either $d or $a, the name/acronym of the dataset being extracted otherwise you will get collisions when you extract multiple tables at once", CheckResult.Warning));

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
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Database " + database + " already exists! if an extraction has already been run " +
                            "you may have errors if you are re-extracting the same tables", CheckResult.Warning));
                else
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Database " + database + " does not exist on server... it will be created at runtime",
                            CheckResult.Success));
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
