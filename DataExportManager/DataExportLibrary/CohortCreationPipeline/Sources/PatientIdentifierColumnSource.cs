using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.QueryBuilding;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Sources
{
    /// <summary>
    /// Pipeline source component that generates a DataTable containing all the unique patient identifiers in the column referenced by the <see cref="IPipelineRequirement"/> 
    /// <see cref="ExtractionInformation"/>.
    /// </summary>
    public class PatientIdentifierColumnSource:IPluginDataFlowSource<DataTable>, IPipelineRequirement<ExtractionInformation>
    {
        private ExtractionInformation _extractionInformation;

        private bool _haveSentData = false;

        [DemandsInitialization("How long to wait for the select query to run before giving up in seconds",DemandType.Unspecified,60)]
        public int Timeout { get; set; }
        
        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_haveSentData)
                return null;
            
            _haveSentData = true;

            return GetDataTable(Timeout,null);
        }

        private DataTable GetDataTable(int timeout, int? topX)
        {
            var qb = new QueryBuilder("distinct", null);
            if(topX != null)
                qb.TopX = topX.Value;
            qb.AddColumn(_extractionInformation);

            var server = _extractionInformation.CatalogueItem.Catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, true);

            var colName = _extractionInformation.GetRuntimeName();

            DataTable dt = new DataTable();
            dt.Columns.Add(colName);
            
            using (var con = server.GetConnection())
            {
                con.Open();
                var cmd = server.GetCommand(qb.SQL, con);
                cmd.CommandTimeout = timeout;

                var r = cmd.ExecuteReader();

                while (r.Read())
                    dt.Rows.Add(new[] { r[colName] });
            }

            return dt;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public DataTable TryGetPreview()
        {
            return GetDataTable(10,1000);
        }

        public void Check(ICheckNotifier notifier)
        {
            if(!_extractionInformation.IsExtractionIdentifier)
                notifier.OnCheckPerformed(new CheckEventArgs("Column '" + _extractionInformation + "' is not marked IsExtractionIdentifier, are you sure it contains patient identifiers?",CheckResult.Fail));

            try
            {
                var dt = GetDataTable(5,1000);

                if (dt.Rows.Count == 0)
                    notifier.OnCheckPerformed(new CheckEventArgs("The table is empty!", CheckResult.Fail));
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to get DataTable/build query", CheckResult.Fail, ex));
            }
            
        }

        public void PreInitialize(ExtractionInformation value, IDataLoadEventListener listener)
        {
            _extractionInformation = value;
        }
    }
}
