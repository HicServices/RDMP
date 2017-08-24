using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CohortManagerLibrary.QueryBuilding;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Sources
{
    public class PatientIndexTableSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<AggregateConfiguration>, IPipelineRequirement<ExtractableCohort>
    {
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;
        private AggregateConfiguration _configuration;

        [DemandsInitialization("The length of time (in seconds) to wait before timing out the SQL command to execute the Aggregate.",DemandType.Unspecified,10000)]
        public int Timeout { get; set; }

        private bool haveSentData = false;
        private ExtractableCohort _extractableCohort;


        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {

            if(haveSentData)
                return null;

            haveSentData = true;

            return GetDataTable(Timeout, listener);
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
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

            var server = _configuration.Catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, false);
            
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

                dt.TableName = SqlSyntaxHelper.GetSensibleTableNameFromString(_configuration.Name + "_ID" + _configuration.ID);
                if (listener != null)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "successfully read " +dt.Rows + " rows from source"));


                return dt;
            }
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                var _sql = GetSQL();
                notifier.OnCheckPerformed(new CheckEventArgs("successfully built extraction SQL:" + _sql, CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not build extraction SQL for '" +_configuration + "' with parent '" +  _cohortIdentificationConfiguration + "'", CheckResult.Fail,e));
            }
            
        }

        private string GetSQL()
        {
            CohortQueryBuilder builder = new CohortQueryBuilder(_configuration,_cohortIdentificationConfiguration.GetAllParameters(),true);

            var sql = builder.SQL;

            var extractionIdentifier = _configuration.AggregateDimensions.Single(d => d.IsExtractionIdentifier);

            string whereString = _configuration.RootFilterContainer_ID != null ? "AND " : "WHERE ";

            var impromptuSql = whereString + extractionIdentifier.SelectSQL + " IN (SELECT " +
                               _extractableCohort.GetPrivateIdentifier() + " FROM " +
                               _extractableCohort.ExternalCohortTable.TableName + " WHERE " + _extractableCohort.WhereSQL() + ")";
            
            //if there is a group by then we must insert the AND patient in cohort bit before the group by but after any WHERE containers
            int insertionPoint = sql.IndexOf("group by", 0, StringComparison.CurrentCultureIgnoreCase);

            //if there isn't a group by
            if (insertionPoint == -1)
                return sql + Environment.NewLine + impromptuSql;
            
            //there is a group by
            return sql.Substring(0, insertionPoint) + Environment.NewLine + impromptuSql + Environment.NewLine + sql.Substring(insertionPoint, sql.Length - insertionPoint);
        }

        public void PreInitialize(AggregateConfiguration value, IDataLoadEventListener listener)
        {
            _configuration = value;

            _cohortIdentificationConfiguration = value.GetCohortIdentificationConfigurationIfAny();

            if(_cohortIdentificationConfiguration == null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Configuration " + _configuration + " is not a valid input because it does not have a CohortIdentificationConfiguration nevermind a JoinableCohortAggregateConfiguration.  Maybe it isn't a patient index table?"));
        }

        public void PreInitialize(ExtractableCohort value, IDataLoadEventListener listener)
        {
            _extractableCohort = value;
        }
    }
}
