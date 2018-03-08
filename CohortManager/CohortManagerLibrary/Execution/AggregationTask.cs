using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using MapsDirectlyToDatabaseTable;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CohortManagerLibrary.Execution
{
    /// <summary>
    /// A single AggregateConfiguration being executed by a CohortCompiler.  The AggregateConfiguration will be a query like 'select distinct patientId from 
    /// TableX where ...'.  The  query result table can/will be commited as a CacheCommitIdentifierList to  the CachedAggregateConfigurationResultsManager.
    /// </summary>
    public class AggregationTask:CacheableTask
    {
        public AggregateConfiguration Aggregate { get; private set; }

        private string _catalogueName;
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

        public AggregationTask(AggregateConfiguration aggregate, CohortCompiler compiler): base(compiler)
        {
            Aggregate = aggregate;
            _catalogueName = aggregate.Catalogue.Name;
            _cohortIdentificationConfiguration = compiler.CohortIdentificationConfiguration;
        }


        public override string GetCatalogueName()
        {
            return _catalogueName;
        }

        public override IMapsDirectlyToDatabaseTable Child
        {
            get { return Aggregate; }
        }


        public override string ToString()
		{
            string name = Aggregate.ToString();

            string expectedTrimStart = _cohortIdentificationConfiguration.GetNamingConventionPrefixForConfigurations();

            if (name.StartsWith(expectedTrimStart))
                return name.Substring(expectedTrimStart.Length);

            return name;
        }

        public override IDataAccessPoint[] GetDataAccessPoints()
        {
            return Aggregate.Catalogue.GetTableInfoList(false);
        }

        public override AggregateConfiguration GetAggregateConfiguration()
        {
            return Aggregate;
        }

        public override CacheCommitArguments GetCacheArguments(string sql, DataTable results, DatabaseColumnRequest[] explicitTypes)
        {
            return new CacheCommitIdentifierList(Aggregate, sql, results, explicitTypes.Single(), Timeout);
        }

        public override void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager)
        {
            manager.DeleteCacheEntryIfAny(Aggregate, AggregateOperation.IndexedExtractionIdentifierList);
        }
    }
}
