using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.CohortCreationPipeline.Sources;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline
{
    /// <summary>
    /// Executes an ExtractionConfiguration's CohortRefreshPipeline which should result in the CohortIdentificationConfiguration associated with the 
    /// ExtractionConfiguration (if any) being recalculated and a new updated set of patient identifiers commited as the next version number in the cohort
    /// database for that ExtractionConfiguration.
    /// 
    /// Use this class if you want to re-run a the patient identifiers of an ExtractionConfiguration without changing the cohort identification configuration
    /// query (say 1 month later you want to generate an extract with the new patients fitting cohort criteria).
    /// </summary>
    public class CohortRefreshEngine
    {
        private readonly IDataLoadEventListener _listener;
        private readonly ExtractionConfiguration _configuration;

        public CohortCreationRequest Request { get; private set; }

        public CohortRefreshEngine(IDataLoadEventListener listener, ExtractionConfiguration configuration)
        {
            _listener = listener;
            _configuration = configuration;
            Request = new CohortCreationRequest(configuration);
        }

        public void Execute()
        {
            var engine = Request.GetEngine(_configuration.CohortRefreshPipeline,_listener);

            //if the refresh pipeline is a cic source
            var cicSource = engine.SourceObject as CohortIdentificationConfigurationSource;
            if (cicSource != null)
            {
                //a cohort identification configuration is a complex query possibly with many cached subqueries, if we are refreshing the cic we will want to clear (and recache) identifiers
                //from the live tables
                cicSource.ClearCohortIdentificationConfigurationCacheBeforeRunning = true;
            }
            
            engine.ExecutePipeline(new GracefulCancellationToken());

            var newCohort = Request.CohortCreatedIfAny;
            if (newCohort != null)
            {
                _configuration.Cohort_ID = newCohort.ID;
                _configuration.SaveToDatabase();
            }
        }
    }
}
