using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.AutoComplete;
using CatalogueManager.ObjectVisualisation;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueManager.DataViewing.Collections
{
    public class ViewAggregateExtractUICollection : IViewSQLAndResultsCollection
    {
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        public bool UseQueryCache { get; set; }

        public ViewAggregateExtractUICollection()
        {
            Helper = new PersistStringHelper();
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
        }

        public ViewAggregateExtractUICollection(AggregateConfiguration config):this()
        {
            DatabaseObjects.Add(config);
        }

        public string SaveExtraText()
        {
            return "";
        }

        public void LoadExtraText(string s)
        {
            
        }

        public IHasDependencies GetAutocompleteObject()
        {
            return AggregateConfiguration;
        }

        public void SetupRibbon(RDMPObjectsRibbonUI ribbon)
        {
            ribbon.Add(AggregateConfiguration);
            
            if(UseQueryCache)
            {
                var cache = GetCacheServer();
                if (cache != null)
                    ribbon.Add(cache);
            }
        }

        private ExternalDatabaseServer GetCacheServer()
        {
            var cic = AggregateConfiguration.GetCohortIdentificationConfigurationIfAny();

            if (cic != null && cic.QueryCachingServer_ID != null)
                return cic.QueryCachingServer;

            return null;
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            var dim = AggregateConfiguration.AggregateDimensions.FirstOrDefault();

            //the aggregate has no dimensions
            if (dim == null)
            {
                var table = AggregateConfiguration.ForcedJoins.FirstOrDefault();
                if(table == null)
                    throw new Exception("AggregateConfiguration '" + AggregateConfiguration +"' has no AggregateDimensions and no TableInfo forced joins, we do not know where/what table to run the query on");

                return table;
            }

            return dim.ColumnInfo.TableInfo;
        }

        public string GetSql()
        {
            string sql = "";
            var ac = AggregateConfiguration;

            if (ac.IsCohortIdentificationAggregate)
            {
                var cic = ac.GetCohortIdentificationConfigurationIfAny();
                var isJoinable = ac.IsJoinablePatientIndexTable();
                var globals = cic.GetAllParameters();

                var builder = new CohortQueryBuilder(ac, globals, isJoinable);
                
                if(UseQueryCache)
                    builder.CacheServer = GetCacheServer();

                sql = builder.GetDatasetSampleSQL(100);
            }
            else
            {
                var builder = ac.GetQueryBuilder();
                sql = builder.SQL;
            }

            return sql;
        }

        public string GetTabName()
        {
            return "View Top 100 " + AggregateConfiguration;
        }

        public void AdjustAutocomplete(AutoCompleteProvider autoComplete)
        {
            
        }

        AggregateConfiguration AggregateConfiguration { get
        {
            return DatabaseObjects.OfType<AggregateConfiguration>().SingleOrDefault();
        } }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var a = AggregateConfiguration;
            return a != null?a.GetQuerySyntaxHelper():null;
        }
    }
}
