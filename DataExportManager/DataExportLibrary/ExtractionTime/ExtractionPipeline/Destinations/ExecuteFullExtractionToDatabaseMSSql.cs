using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.UserPicks;
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
    [Description("The Extraction target for DataExportManager into a Microsoft SQL Database, this should only be used by ExtractionPipelineHost as it is the only class that knows how to correctly call PreInitialize ")]
    public class ExecuteFullExtractionToDatabaseMSSql : IExecuteDatasetExtractionDestination
    {
        private IExtractCommand _request;

        [DemandsInitialization("External server to create the extraction into, a new table will be created for each dataset extracted with datatypes and column names appropriate to the anonymised extracted datasets",Mandatory = true)]
        public ExternalDatabaseServer TargetDatabaseServer { get; set; }
        private DiscoveredDatabase _server;
        private DataTableUploadDestination _destination;
        
        private DataLoadInfo _dataLoadInfo;
        private bool haveExtractedBundledContent = false;

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //give the data table the correct name
            toProcess.TableName = GetTableName();

            if (_request is ExtractDatasetCommand && !haveExtractedBundledContent)
            {
                var bundle = ((ExtractDatasetCommand) _request).DatasetBundle;
                foreach (var sql in bundle.SupportingSQL)
                    bundle.States[sql] = ExtractSupportingSql(sql, listener);

                foreach (var document in ((ExtractDatasetCommand) _request).DatasetBundle.Documents)
                    bundle.States[document] = ExtractSupportingDocument(document, listener);

                haveExtractedBundledContent = true;
            }

            if (_destination == null)
            {
                //see if the user has entered an extraction server/database 
                if (TargetDatabaseServer == null)
                    throw new Exception("TargetDatabaseServer (the place you want to extract the project data to) property has not been set!");
                
                _server = GetDestinationServer(listener);
                
                try
                {
                    if (!_server.Exists())
                        throw new Exception("Could not connect to server " + TargetDatabaseServer.Server);

                    //See if table already exists on the server (likely to cause problems including duplication, schema changes in configuration etc)
                    if (_server.ExpectTable(GetTableName()).Exists())
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                "A table called " + GetTableName() + " already exists on server " + TargetDatabaseServer +
                                ", rows will be appended to this table - or data load might crash if it has an incompatible schema"));
                }
                catch (Exception e)
                {
                    //Probably the database didn't exist or the credentials were wrong or something
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Failed to inspect destination for already existing datatables",e));
                }


                _destination = new DataTableUploadDestination();
                _destination.AllowResizingColumnsAtUploadTime = true;
                _destination.PreInitialize(_server, listener);
                
                //Record that we are loading the table (the drop refers to 'rollback advice' in the audit log - don't worry about it
                TableLoadInfo = new TableLoadInfo(_dataLoadInfo, "Drop table " + toProcess.TableName, toProcess.TableName, new DataSource[] { new DataSource(_request.DescribeExtractionImplementation(), DateTime.Now) }, -1);
            }

            _destination.ProcessPipelineData(toProcess, listener, cancellationToken);
            TableLoadInfo.Inserts += toProcess.Rows.Count;

            return null;
        }


        private string _cachedGetTableNameAnswer;
        


        public string GetTableName()
        {
            //if there is a cached answer return it
            if (_cachedGetTableNameAnswer != null) return _cachedGetTableNameAnswer;

            //otherwise, fetch and cache answer
            var project = _request.Configuration.Project;
            _cachedGetTableNameAnswer =  SqlSyntaxHelper.GetSensibleTableNameFromString(project.Name + "_" + project.ProjectNumber + "_" + _request.Configuration + "_" + _request);

            return _cachedGetTableNameAnswer;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if(_destination != null)
            {
                _destination.Dispose(listener,pipelineFailureExceptionIfAny);
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
        }

        public void PreInitialize(DataLoadInfo value, IDataLoadEventListener listener)
        {
            _dataLoadInfo = value;
        }

        public TableLoadInfo TableLoadInfo { get; private set; }
        public string GetDestinationDescription()
        {
            return GetTableName();
        }

        public void ExtractGlobals(Project project, ExtractionConfiguration configuration, GlobalsBundle globalsToExtract, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            if (globalsToExtract.Any())
            {
                foreach (var sql in globalsToExtract.SupportingSQL)
                    ExtractSupportingSql(sql, listener);

                foreach (var doc in globalsToExtract.Documents)
                    ExtractSupportingDocument(doc, listener);

            }
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
                    dt.TableName = SqlSyntaxHelper.GetSensibleTableNameFromString(sql.Name);

                    listener.OnProgress(this, new ProgressEventArgs("Reading from SupportingSQL " + sql.Name, new ProgressMeasurement(dt.Rows.Count, ProgressType.Records), sw.Elapsed));
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Decided on the following destination table name for SupportingSQL:" + dt.TableName));

                    tempDestination.AllowResizingColumnsAtUploadTime = true;
                    tempDestination.PreInitialize(GetDestinationServer(listener), listener);
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

        private ExtractCommandState ExtractSupportingDocument(SupportingDocument doc, IDataLoadEventListener listener)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    "Ignored SupportingDocument " + doc.Name +
                    " because destination is an SQL database.  File path is " + doc.URL));

            return ExtractCommandState.Warning;
        }


        private DiscoveredDatabase GetDestinationServer(IDataLoadEventListener listener)
        {
            //tell user we are about to inspect it
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to open connection to " + TargetDatabaseServer));

            return DataAccessPortal.GetInstance().ExpectDatabase(TargetDatabaseServer, DataAccessContext.DataExport);
            
        }


        public void Check(ICheckNotifier notifier)
        {
            if (TargetDatabaseServer == null)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Target database server property has not been set (This component does not know where to extract data to!), to fix this you must edit the pipeline and choose an ExternalDatabaseServer to extract to)",
                        CheckResult.Fail));
                return;
            }
            if (string.IsNullOrWhiteSpace(TargetDatabaseServer.Server))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("TargetDatabaseServer does not have a .Server specified", CheckResult.Fail));
                return;
            }

            if(string.IsNullOrWhiteSpace(TargetDatabaseServer.Database))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("TargetDatabaseServer does not have a .Database specified", CheckResult.Fail));
                return;
            }

            try
            {
                var server = DataAccessPortal.GetInstance().ExpectServer(TargetDatabaseServer, DataAccessContext.DataExport);
                DbConnection con = server.GetConnection();
                con.Open();
                con.Close();

                notifier.OnCheckPerformed(new CheckEventArgs("successfully connected to " + TargetDatabaseServer + ")",CheckResult.Success));

                var database = server.ExpectDatabase(TargetDatabaseServer.Database);
                if (database.Exists())
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("Confirmed database " + database + " exists",
                            CheckResult.Success));
                else
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Database " + database +
                            " does not exist on server... how were we even able to connect?!", CheckResult.Fail));
                    return;
                }
                
                var tables = database.DiscoverTables(false);

                if (tables.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("The following preexisting tables were found in the database " + string.Join(",",tables.Select(t=>t.ToString())),CheckResult.Warning));
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("Confirmed that database " + database + " is empty of tables",
                            CheckResult.Success));

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not connect to TargetDatabaseServer '" + TargetDatabaseServer  +"'", CheckResult.Fail, e));
            }


        }

    }
}
