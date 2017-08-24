using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CohortManagerLibrary.QueryBuilding;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Sources
{
    public class CohortIdentificationConfigurationSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<CohortIdentificationConfiguration>
    {
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

        [DemandsInitialization("The length of time (in seconds) to wait before timing out the SQL command to execute the CohortIdentificationConfiguration, if you find it is taking exceptionally long for a CohortIdentificationConfiguration to execute then consider caching some of the subqueries",DemandType.Unspecified,10000)]
        public int Timeout { get; set; }

        [DemandsInitialization("If ticked, will Freeze the CohortIdentificationConfiguration if the import pipeline terminates successfully")]
        public bool FreezeAfterSuccessfulImport { get; set; }

        private bool haveSentData = false;

        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {

            if(haveSentData)
                return null;

            haveSentData = true;

            return GetDataTable(Timeout, listener);
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            //if it didn't crash
            if(pipelineFailureExceptionIfAny == null)
                if(FreezeAfterSuccessfulImport)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Freezing CohortIdentificationConfiguration"));
                    _cohortIdentificationConfiguration.Freeze();
                }
        }


        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public DataTable TryGetPreview()
        {
            return GetDataTable(10,null);
        }

        private DataTable GetDataTable(int timeout, IDataLoadEventListener listener)
        {
            if(listener != null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to lookup which server to interrogate for CohortIdentificationConfiguration " + _cohortIdentificationConfiguration));

            var server = DataAccessPortal.GetInstance()
                .ExpectDistinctServer(_cohortIdentificationConfiguration.GetDistinctTableInfos(),
                    DataAccessContext.DataExport, false);

            using (var con = server.GetConnection())
            {
                con.Open();

                var sql = GetSQL();

                if (listener != null)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Connection opened, ready to send the following SQL (with Timeout "+Timeout+"s):" + Environment.NewLine + sql));

                var cmd = server.GetCommand(sql, con);
                cmd.CommandTimeout = timeout;

                var dt = new DataTable();
                server.GetDataAdapter(cmd).Fill(dt);

                dt.TableName = SqlSyntaxHelper.GetSensibleTableNameFromString(_cohortIdentificationConfiguration.Name +"_ID" + _cohortIdentificationConfiguration.ID);

                if (listener != null)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "successfully read " +dt.Rows + " rows from source"));


                return dt;
            }
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                if (_cohortIdentificationConfiguration.Frozen)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "CohortIdentificationConfiguration " + _cohortIdentificationConfiguration +
                            " is Frozen (By " + _cohortIdentificationConfiguration.FrozenBy + " on " +
                            _cohortIdentificationConfiguration.FrozenDate + ").  It might have already been imported once before.", CheckResult.Warning));

                var _sql = GetSQL();
                notifier.OnCheckPerformed(new CheckEventArgs("successfully built extraction SQL:" + _sql, CheckResult.Success));

                var result = TryGetPreview();
                
                if(result.Rows.Count == 0)
                    throw new Exception("No Identifiers were returned by the cohort query");
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not build extraction SQL for " + _cohortIdentificationConfiguration, CheckResult.Fail,e));
            }
            
        }

        private string GetSQL()
        {
            CohortQueryBuilder builder = new CohortQueryBuilder(_cohortIdentificationConfiguration);
            return builder.SQL;
        }

        public void PreInitialize(CohortIdentificationConfiguration value, IDataLoadEventListener listener)
        {
            _cohortIdentificationConfiguration = value;
        }
    }
}
