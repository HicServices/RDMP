using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.QueryBuilding;
using ReusableLibraryCode.DataAccess;


namespace Rdmp.Core.DataViewing
{
    class ViewCohortIdentificationConfigurationSqlCollection : PersistableObjectCollection, IViewSQLAndResultsCollection
    {
        public bool UseQueryCache { get; set; }

        public ViewCohortIdentificationConfigurationSqlCollection()
        {
        }

        public ViewCohortIdentificationConfigurationSqlCollection(CohortIdentificationConfiguration config) : this()
        {
            DatabaseObjects.Add(config);
        }

        public IEnumerable<DatabaseEntity> GetToolStripObjects()
        {
            if (UseQueryCache)
            {
                var cache = GetCacheServer();
                if (cache != null)
                    yield return cache;
            }
        }

        private ExternalDatabaseServer GetCacheServer()
        {
            if (CohortIdentificationConfiguration != null && CohortIdentificationConfiguration.QueryCachingServer_ID != null)
                return CohortIdentificationConfiguration.QueryCachingServer;

            return null;
        }


        public IDataAccessPoint GetDataAccessPoint()
        {
            var cache = GetCacheServer();

            if (UseQueryCache && cache != null)
            {
                return cache;
            }
            else
            {
                var builder = new CohortQueryBuilder(CohortIdentificationConfiguration, null);
                builder.RegenerateSQL();
                return new SelfCertifyingDataAccessPoint(builder.Results.TargetServer);
            }
        }

        public string GetSql()
        {
            var builder = new CohortQueryBuilder(CohortIdentificationConfiguration, null);
            
            if (!UseQueryCache && CohortIdentificationConfiguration.QueryCachingServer_ID.HasValue)
                builder.CacheServer = null;
            

            return builder.SQL;
        }

        public string GetTabName()
        {
            return "View " + CohortIdentificationConfiguration;
        }

        public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
        {
        }

        CohortIdentificationConfiguration CohortIdentificationConfiguration
        {
            get
            {
                return DatabaseObjects.OfType<CohortIdentificationConfiguration>().SingleOrDefault();
            }
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return GetDataAccessPoint()?.GetQuerySyntaxHelper();
        }
    }
}
