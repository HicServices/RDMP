using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.QueryBuilding;
using DataLoadEngine.Attachers;
using DataLoadEngine.DataFlowPipeline.Destinations;
using DataLoadEngine.DataFlowPipeline.Sources;
using DataLoadEngine.Job;
using DataLoadEngine.Job.Scheduling;
using HIC.Logging;
using LoadModules.Generic.LoadProgressUpdating;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Microsoft.Office.Interop.Excel;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;

namespace LoadModules.Generic.Attachers
{
    /// <summary>
    /// Data load component for loading RAW tables with records read from a remote database server.  Runs the specified query (which can include a date parameter)
    /// and inserts the results of the query into RAW. 
    /// </summary>
    public class RemoteDatabaseAttacher: Attacher, IPluginAttacher
    {
        public RemoteDatabaseAttacher(): base(true)
        {
            
        }

        [DemandsInitialization("The DataSource (Server) connect to in order to read data.  Note that this may be MyFriendlyServer (SqlServer) or something like '192.168.56.101:1521/TRAININGDB'(Oracle)",Mandatory=true)]
        public string RemoteServer { get; set; }

        [DemandsInitialization("The database on the remote host containg the table we will read data from", Mandatory = true)]
        public string RemoteDatabaseName { get; set; }

        [DemandsInitialization("The length of time in seconds to allow for data to be completely read from the destination before giving up (0 for no timeout)")]
        public int Timeout { get; set; }

        [DemandsInitialization("Username and password to use when connecting to fetch data from the remote database (e.g. sql user account).  If not provided then 'Integrated Security' (Windows user account) will be used to authenticate")]
        public DataAccessCredentials RemoteAccessCredentials { get; set; }

        [DemandsInitialization("The database type you are attempting to connect to",DefaultValue=DatabaseType.MicrosoftSQLServer)]
        public DatabaseType DatabaseType { get; set; }
        
        private DiscoveredDatabase _remoteDatabase;
        private string _remoteUsername { get; set; }
        private string _remotePassword { get; set; }
        protected bool _setupDone { get; set; }

        public enum PeriodToLoad
        {
            Month,
            Year,
            Decade
        }

        public override void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            base.Initialize(hicProjectDirectory,dbInfo);

            try
            {
                Setup();
            }
            catch (Exception)
            {
                //use integrated security if this fails
            }
        }
        
        public override void Check(ICheckNotifier notifier)
        {
            //if we have been initialized
            if (HICProjectDirectory != null)
            {
                try
                {
                    try
                    {
                        Setup();

                        //if there is a username and password
                        if(!string.IsNullOrWhiteSpace(_remoteUsername) && !string.IsNullOrWhiteSpace(_remotePassword))
                            notifier.OnCheckPerformed(new CheckEventArgs("Found username and password to use with RemoteTableAttacher",CheckResult.Success, null));
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Failed to setup username/password - proceeding with Integrated Security", CheckResult.Warning, e));
                    }
                    
                    CheckTablesMatchedCataloguesBeingLoaded(notifier);
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Program crashed while trying to inspect remote server " + RemoteServer + "  for presence of tables/databases specified in the load configuration.",
                        CheckResult.Fail, e));
                }

            }
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "HICProjectDirectory was null in Check() for class RemoteTableAttacher",
                    CheckResult.Warning, null));
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
            
        }

        protected void CheckTablesMatchedCataloguesBeingLoaded(ICheckNotifier notifier)
        {
            try
            {
                if (!_remoteDatabase.Exists())
                    throw new Exception("Database " + RemoteDatabaseName + " did not exist on the remote server");

                ////still worthwhile doing this incase we cannot connect to the server
                //var tables = _remoteDatabase.DiscoverTables(true).Select(t => t.GetRuntimeName()).ToArray();

                //var loadables = _dbInfo.DiscoverTables(false).Select(t => t.GetRuntimeName()).ToArray();

                //foreach (var loadable in loadables)
                //{
                //    if (tables.Contains(loadable))
                //        notifier.OnCheckPerformed(new CheckEventArgs("Found " + loadable + " on both sides, will load", CheckResult.Success));
                //    else
                //        notifier.OnCheckPerformed(new CheckEventArgs("Could not find " + loadable + " on remote side!", CheckResult.Fail));
                //}

                //foreach (var table in tables.Except(loadables))
                //{
                //    notifier.OnCheckPerformed(new CheckEventArgs("Found " + table + " only on remote side, will be ignored", CheckResult.Warning));
                //}
                
                // todo: match tables with Catalogues being loaded.

                //user has just picked a table to copy exactly so we can precheck for it
                //if (tables.Contains(RemoteTableName))
                //    notifier.OnCheckPerformed(new CheckEventArgs(
                //        "successfully found table " + RemoteTableName + " on server " + RemoteServer + " on database " +
                //        RemoteDatabaseName,
                //        CheckResult.Success, null));
                //else
                //    notifier.OnCheckPerformed(new CheckEventArgs(
                //        "Could not find table called '" + RemoteTableName + "' on server " + RemoteServer + " on database " +
                //        RemoteDatabaseName +Environment.NewLine+"(The following tables were found:"+string.Join(",",tables)+")",
                //        CheckResult.Fail, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred when trying to enumerate tables on server " + RemoteServer + " on database " +RemoteDatabaseName, CheckResult.Fail, e));
            }
        }
        
        private void Setup()
        {
            if(RemoteAccessCredentials != null)
            {
                _remoteUsername = RemoteAccessCredentials.Username;
                _remotePassword = RemoteAccessCredentials.GetDecryptedPassword();
            }
            
            var helper = new DatabaseHelperFactory(DatabaseType).CreateInstance();
            var builder = helper.GetConnectionStringBuilder(RemoteServer, RemoteDatabaseName, _remoteUsername, _remotePassword);
            _remoteDatabase = new DiscoveredServer(builder).GetCurrentDatabase();
            
            _setupDone = true;
        }

        public override ExitCodeType Attach(IDataLoadJob job)
        {
            base.Attach(job);

            if (job == null)
                throw new Exception("Job is Null, we require to know the job to build a DataFlowPipeline");
      
            string sql;

            var remoteTables = _remoteDatabase.DiscoverTables(true).Select(t => t.GetRuntimeName()).ToArray();
            var loadables = _dbInfo.DiscoverTables(false).Select(t => t.GetRuntimeName()).ToArray();

            foreach (var table in remoteTables)
            {
                if (!loadables.Contains(table))
                    continue;

                sql = "Select * from " + table;

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to execute SQL:" + Environment.NewLine + sql));

                var source = new DbDataCommandDataFlowSource(sql, "Fetch data from " + RemoteServer + " to populate RAW table " + table, 
                                                             _remoteDatabase.Server.Builder, Timeout == 0 ? 50000 : Timeout);

                var destination = new SqlBulkInsertDestination(_dbInfo, table, Enumerable.Empty<string>());

                var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
                var context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo | PipelineUsage.FixedDestination);

                var engine = new DataFlowPipelineEngine<DataTable>(context, source, destination, job);

                ITableLoadInfo loadInfo = job.DataLoadInfo.CreateTableLoadInfo("Truncate RAW table " + table,
                    _dbInfo.Server.Name + "." + _dbInfo.GetRuntimeName(),
                    new[]
                    {
                        new DataSource(
                            "Remote SqlServer Servername=" + RemoteServer + "Database=" + _dbInfo.GetRuntimeName() +

                            //Either list the table or the query depending on what is populated
                            (table != null ? " Table=" + table : " Query = " + sql), DateTime.Now)
                    }, -1);

                engine.Initialize(loadInfo);
                engine.ExecutePipeline(new GracefulCancellationToken());

                if (source.TotalRowsRead == 0)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No rows were read from the remote table " + table + "."));
                }

                job.OnNotify(this, new NotifyEventArgs(source.TotalRowsRead > 0 ? ProgressEventType.Information : ProgressEventType.Warning, "Finished after reading " + source.TotalRowsRead + " rows"));
            }
            
            return ExitCodeType.Success;
        }
    }
}
