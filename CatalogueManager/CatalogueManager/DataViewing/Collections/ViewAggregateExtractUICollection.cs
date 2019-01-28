using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.AutoComplete;
using CatalogueManager.ObjectVisualisation;
using CohortManagerLibrary.QueryBuilding;
using FAnsi.Discovery.QuerySyntax;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.DataViewing.Collections
{
    public class ViewAggregateExtractUICollection : PersistableObjectCollection,IViewSQLAndResultsCollection
    {
        public bool UseQueryCache { get; set; }

        public ViewAggregateExtractUICollection()
        {
        }

        public ViewAggregateExtractUICollection(AggregateConfiguration config):this()
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

        public IEnumerable<string> GetToolStripStrings()
        {
            yield break;
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
            if(AggregateConfiguration != null)
                autoComplete.Add(AggregateConfiguration);
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
