using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.DataAccess;

namespace DataExportLibrary.CohortDescribing
{
    /// <summary>
    /// Creates ExtractableCohortDescription objects for each of your cohorts, this involves issuing an async request to the cohort endpoints to calcualte things like 
    /// Count, CountDistinct etc.  The ExtractableCohortDescription objects returned from Create will not be populated with values until the async finishes and will have only
    /// placeholder values like "Loading..." etc
    /// </summary>
    public class CohortDescriptionFactory
    {
        private ExternalCohortTable[] _sources;
        private ExtractableCohort[] _cohorts;

        /// <summary>
        /// Creates ExtractableCohortDescription objects for each of your cohorts, this involves issuing an async request to the cohort endpoints to calcualte things like 
        /// Count, CountDistinct etc.  The ExtractableCohortDescription objects returned from Create will not be populated with values until the async finishes and will have only
        /// placeholder values like "Loading..." etc
        /// </summary>
        /// <param name="repository">The DataExportRepository containing all your cohort refrences (ExtractableCohorts)</param>
        public CohortDescriptionFactory(IDataExportRepository repository)
        {
            _sources = repository.GetAllObjects<ExternalCohortTable>();
            _cohorts = repository.GetAllObjects<ExtractableCohort>();
        }

        /// <summary>
        /// Starts 1 async fetch request for each cohort endpoint e.g. NormalCohorts ExternalCohortTable contains 100 cohorts while FreakyCohorts ExternalCohortTable has another 30. 
        /// These async requests are managed by the CohortDescriptionDataTableAsyncFetch object which has a callback for compeltion.  Each ExtractableCohortDescription subscribes to
        /// the callback to self populate
        /// </summary>
        /// <returns></returns>
        public Dictionary<CohortDescriptionDataTableAsyncFetch, ExtractableCohortDescription[]> Create()
        {
            var toReturn = new Dictionary<CohortDescriptionDataTableAsyncFetch,ExtractableCohortDescription[]>();

            foreach (ExternalCohortTable source in _sources)
            {
                //setup the async data retreival which can take a long time if there are a lot of cohorts or millions of identifiers
                var asyncFetch = new CohortDescriptionDataTableAsyncFetch(source);
                var cohorts = _cohorts.Where(c => c.ExternalCohortTable_ID == source.ID).Select(c => new ExtractableCohortDescription(c, asyncFetch)).ToArray();

                asyncFetch.Begin();

                toReturn.Add(asyncFetch,cohorts);

            }

            return toReturn;
        }
    }
}
