using System;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Sources
{
    /// <summary>
    /// Pipeline source component which executes an AggregateConfiguration query (e.g. Aggregate Graph / Joinable patient index table)
    /// </summary>
    public class AggregateConfigurationTableSource : IPluginDataFlowSource<DataTable>,IPipelineRequirement<AggregateConfiguration>
    {
        protected AggregateConfiguration AggregateConfiguration;
        protected CohortIdentificationConfiguration CohortIdentificationConfigurationIfAny;

        private bool _haveSentData = false; 

        [DemandsInitialization("The length of time (in seconds) to wait before timing out the SQL command to execute the Aggregate.", DemandType.Unspecified, 10000)]
        public int Timeout { get; set; }

        protected virtual string GetSQL()
        {
            var builder = AggregateConfiguration.GetQueryBuilder();

            return builder.SQL;
        }
        
        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_haveSentData)
                return null;

            _haveSentData = true;

            return GetDataTable(Timeout, listener);
        }

        private DataTable GetDataTable(int timeout, IDataLoadEventListener listener)
        {
            if (listener != null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to lookup which server to interrogate for AggregateConfiguration '" + AggregateConfiguration +"'"));

            var server = AggregateConfiguration.Catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, false);


            using (var con = server.GetConnection())
            {
                con.Open();

                var sql = GetSQL();

                if (listener != null)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Connection opened, ready to send the following SQL (with Timeout " + Timeout + "s):" + Environment.NewLine + sql));

                var cmd = server.GetCommand(sql, con);
                cmd.CommandTimeout = timeout;

                var dt = new DataTable();
                server.GetDataAdapter(cmd).Fill(dt);

                dt.TableName = server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(AggregateConfiguration.Name + "_ID" + AggregateConfiguration.ID);

                if (listener != null)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "successfully read " + dt.Rows + " rows from source"));


                return dt;
            }
        }
        public DataTable TryGetPreview()
        {
            return GetDataTable(10, null);
        }



        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {

        }

        public void Abort(IDataLoadEventListener listener)
        {

        }

        public virtual void Check(ICheckNotifier notifier)
        {
            try
            {
                var _sql = GetSQL();
                notifier.OnCheckPerformed(new CheckEventArgs("successfully built extraction SQL:" + _sql, CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not build extraction SQL for '" + AggregateConfiguration + "' (ID="+AggregateConfiguration.ID+ ")", CheckResult.Fail, e));
            }
        }

        public virtual void PreInitialize(AggregateConfiguration value, IDataLoadEventListener listener)
        {
            AggregateConfiguration = value;

            CohortIdentificationConfigurationIfAny = value.GetCohortIdentificationConfigurationIfAny();
        }
    }
}