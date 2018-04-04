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
    /// <summary>
    /// Pipeline source component which executes an AggregateConfiguration query in a CohortIdentificationConfiguration which has the role of 
    /// 'Patient Index Table' (JoinableCohortAggregateConfiguration).  A 'Patient Index Table' is what researchers call any table with information
    /// about patients (e.g. a table containing every prescription date for a given drug) in which the data (not patient identifiers) is directly 
    /// used to identify their cohort (e.g. cohort query is 'everyone who has been hospitalised with code X within 6 months of having a prescription
    /// of drug Y - in this case the patient index table is 'the prescribed dates of drug Y').  
    /// 
    /// <para>Since 'Patient Index Tables' always contain a superset of the final identifiers this component will add an additional filter to the query
    /// to restrict rows returned only to those patients in your final cohort list (you must already have a committed final cohort list to use this
    /// component).  This prevents you saving a snapshot of 1,000,000 prescription dates when your final cohort of patients only own 500 of those 
    /// records (because the cohort identification configuration includes further set operations that reduce the patient count beyond the prescribed drug Y).</para>
    /// 
    /// <para>The purpose of all this is usually to ship a table ('Patient Index Table') which was used to build the researchers cohort into the saved cohorts 
    /// database so it can be linked and extracted (as custom data) along with all the normal datasets that make up the researchers extract.</para>
    /// </summary>
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

                dt.TableName = server.GetQuerySyntaxHelper().GetSensibleTableNameFromString(_configuration.Name + "_ID" + _configuration.ID);

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

            //IMPORTANT: We are using impromptu SQL instead of a Spontaneous container / CustomLine because we want the CohortQueryBuilder to decide to use
            //the cached table data (if any).  If it senses we are monkeying with the query it will run it verbatim which will be very slow.

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
