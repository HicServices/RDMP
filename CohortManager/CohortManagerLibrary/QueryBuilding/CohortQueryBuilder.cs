using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.QueryBuilding.Parameters;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using QueryCaching.Aggregation;
using ReusableLibraryCode.Checks;

namespace CohortManagerLibrary.QueryBuilding
{
    /// <summary>
    /// Builds complex cohort identification queries by combining subqueries with SQL set operations (UNION / INTERSECT / EXCEPT).  Cohort identification
    /// sub queries fundamentally take the form of 'Select distinct patientId from TableX'.  All the complexity comes in the form of IFilters (WHERE Sql), 
    /// parameters, using cached query results, patient index tables etc.
    /// 
    /// <para>User cohort identification queries are all create under a CohortIdentificationConfiguration which will have a single root CohortAggregateContainer.  A
    /// final count for the number of patients in the cohort can be determined by running the root CohortAggregateContainer.  The user will often want to run each
    /// sub query independently however to get counts for each dataset involved.  Sub queries are defined in AggregateConfigurations.</para>
    /// 
    /// <para>In order to build complex multi table queries across multiple datasets with complex where/parameter/join logic with decent performance RDMP supports 
    /// caching.  Caching involves executing each sub query (AggregateConfiguration) and storing the resulting patient identifier list in an indexed table on
    /// the caching server (See CachedAggregateConfigurationResultsManager).  These cached queries are versioned by the SQL used to generate them (to avoid stale
    /// result lists).  Where available CohortQueryBuilder will use the cached result list instead of running the full query since it runs drastically faster.</para>
    /// 
    /// <para>The SQL code for individual queries is created by CohortQueryBuilderHelper (using AggregateBuilder).</para>
    /// </summary>
    public class CohortQueryBuilder
    {
        private readonly ISqlParameter[] _globals;
        object oSQLLock = new object();
        private string _sql;

        public string SQL
        {
            get
            {
                lock (oSQLLock)
                {
                    if (SQLOutOfDate)
                        RegenerateSQL();
                    return _sql;
                }
            }
        }
        
        public int TopX { get; set; }

        private CohortAggregateContainer container;
        private AggregateConfiguration configuration;
        private readonly bool _isExplicitRequestForJoinableInceptionAggregateQuery;

        public ExternalDatabaseServer CacheServer
        {
            get { return _cacheServer; }
            set
            {
                _cacheServer = value;
                if(helper != null)
                    helper.CacheServer = value;

                SQLOutOfDate = true;
            }
        }

        public ParameterManager ParameterManager = new ParameterManager();

        public int CountOfSubQueries { get { return helper != null? helper.CountOfSubQueries:- 1; } }
        public int CountOfCachedSubQueries { get { return helper != null ? helper.CountOfCachedSubQueries : -1; } }

        #region constructors
        //Constructors - This one is the base one called by all others
        private CohortQueryBuilder(IEnumerable<ISqlParameter> globals)
        {
            globals = globals ?? new ISqlParameter[] {};
            _globals = globals.ToArray();
            TopX = -1;
            
            SQLOutOfDate = true;

            foreach (ISqlParameter parameter in _globals)
                ParameterManager.AddGlobalParameter(parameter);

            RecreateHelper();
        }

        public CohortQueryBuilder(CohortIdentificationConfiguration configuration):this(configuration.GetAllParameters())
        {
            if (configuration == null)
                throw new QueryBuildingException("Configuration has not been set yet");

            if (configuration.RootCohortAggregateContainer_ID == null)
                throw new QueryBuildingException("Root container not set on CohortIdentificationConfiguration " + configuration);

            if (configuration.QueryCachingServer_ID != null)
                CacheServer = configuration.QueryCachingServer;

            //set ourselves up to run with the root container
            container = configuration.RootCohortAggregateContainer;
        }
        public CohortQueryBuilder(CohortAggregateContainer c,IEnumerable<ISqlParameter> globals): this(globals)
        {
            //set ourselves up to run with the root container
            container = c;
        }
        public CohortQueryBuilder(AggregateConfiguration config, IEnumerable<ISqlParameter> globals, bool isExplicitRequestForJoinableInceptionAggregateQuery = false): this(globals)
        {
            //set ourselves up to run with the root container
            configuration = config;
            _isExplicitRequestForJoinableInceptionAggregateQuery = isExplicitRequestForJoinableInceptionAggregateQuery;
        }

        #endregion

        public string GetDatasetSampleSQL(int topX = 1000)
        {
            if(configuration == null)
                throw new NotSupportedException("Can only generate select * statements when constructed for a single AggregateConfiguration, this was constructed with a container as the root entity (it may even reflect a UNION style query that spans datasets)");

            if(!string.IsNullOrWhiteSpace(configuration.HavingSQL))
                throw new NotSupportedException("Cannot generate select * statements when the AggregateConfiguration has HAVING Sql");

            //create a clone of ourselves so we don't mess up the ParameterManager of this instance
            var cloneBuilder = new CohortQueryBuilder(configuration, _globals);
            cloneBuilder.TopX = topX;
            cloneBuilder.CacheServer = CacheServer;

            string sampleSQL = 
                cloneBuilder.GetSQLForAggregate(configuration,
                0, 
                _isExplicitRequestForJoinableInceptionAggregateQuery,
                _isExplicitRequestForJoinableInceptionAggregateQuery?null: "*", //preview means we override the select columns with * unless its a patient index table
                ""); //gets rid of the distinct keyword

            string parameterSql = "";

            //get resolved parameters for the select * query (via the clone builder
            var finalParams = cloneBuilder.ParameterManager.GetFinalResolvedParametersList().ToArray();

            if(finalParams.Any())
            {
                foreach (ISqlParameter param in finalParams)
                    parameterSql += QueryBuilder.GetParameterDeclarationSQL(param);

                return parameterSql + Environment.NewLine + sampleSQL;
            }
            
            return sampleSQL;
        }

        

        private CohortQueryBuilderHelper helper;
        public void RegenerateSQL()
        {
            RecreateHelper();
            
            _sql = "";

            if (container != null)
                AddContainerRecursively(container, 0);    //user constructed us with a container (and possibly subcontainers even - any one of them chock full of aggregates)
            else
                AddAggregate(configuration, -1);//user constructed us without a container, he only cares about 1 aggregate
            
            //Still finalise the ParameterManager even if we are not writting out the parameters so that it is in the Finalized state
            var finalParameters = ParameterManager.GetFinalResolvedParametersList();

            if(!DoNotWriteOutParameters)
            {
                string parameterSql = "";

                //add the globals
                foreach (ISqlParameter param in finalParameters)
                    parameterSql += QueryBuilder.GetParameterDeclarationSQL(param);

                _sql =  parameterSql + _sql;
            }
            SQLOutOfDate = false;
        }

        private void RecreateHelper()
        {
            helper = new CohortQueryBuilderHelper(_globals, ParameterManager, CacheServer);
        }

        /// <summary>
        /// Tells the Builder not to write out parameter SQL, unlike AggregateBuilder this will not clear the ParameterManager it will just hide them from the SQL output
        /// </summary>
        public bool DoNotWriteOutParameters
        {
            get { return _doNotWriteOutParameters; }
            set
            {
                _doNotWriteOutParameters = value;
                SQLOutOfDate = true;
            }
        }
        
        private void AddContainerRecursively(CohortAggregateContainer currentContainer, int tabDepth)
        {
            string tabs;
            string tabplusOne;
            helper.GetTabs(tabDepth, out tabs, out tabplusOne);

            //Things we need to output
            var toWriteOut = currentContainer.GetOrderedContents().Where(IsEnabled).ToArray();

            if (toWriteOut.Any())
                _sql += Environment.NewLine + tabs + "(" + Environment.NewLine;

            bool firstEntityWritten = false;
            foreach (IOrderable toWrite in toWriteOut)
            {
                if (firstEntityWritten)
                    _sql += Environment.NewLine + Environment.NewLine + tabplusOne + currentContainer.Operation + Environment.NewLine + Environment.NewLine;

                if(toWrite is AggregateConfiguration)
                    AddAggregate((AggregateConfiguration)toWrite, tabDepth);

                if (toWrite is CohortAggregateContainer)
                {
                    if(_isExplicitRequestForJoinableInceptionAggregateQuery)
                        throw new NotSupportedException("Flag _isExplicitRequestForJoinableInceptionAggregateQuery is not supported when outputing an entire container, how did you even manage to set this private flag?");

                    AddContainerRecursively((CohortAggregateContainer) toWrite, tabDepth + 1);

                }


                //we have now written the first thing at this level of recursion - all others will need to be separated by the OPERATION e.g. UNION
                firstEntityWritten = true;

                if (StopContainerWhenYouReach != null && StopContainerWhenYouReach.Equals(toWrite))
                    if (tabDepth != 0)
                        throw new NotSupportedException("Stopping prematurely only works when the aggregate to stop at is in the top level container");
                    else
                        break;
            }

            //if we outputted anything
            if (toWriteOut.Any())
                _sql += Environment.NewLine + tabs + ")" + Environment.NewLine ;
        }

        /// <summary>
        /// Objects are enabled if they do not support disabling (<see cref="IDisableable"/>) or are <see cref="IDisableable.IsDisabled"/> = false
        /// </summary>
        /// <returns></returns>
        private bool IsEnabled(IOrderable arg)
        {
            //skip disabled things
            var dis = arg as IDisableable;
            return dis == null || !dis.IsDisabled;
        }

        private void AddAggregate(AggregateConfiguration aggregate, int tabDepth)
        {
            _sql += GetSQLForAggregate(aggregate, tabDepth, _isExplicitRequestForJoinableInceptionAggregateQuery);
        }

        private string GetSQLForAggregate(AggregateConfiguration aggregate, int tabDepth, bool isJoinAggregate = false, string overrideSelectList = null, string overrideLimitationSQL=null)
        {
            return helper.GetSQLForAggregate(aggregate, tabDepth, isJoinAggregate, overrideSelectList,overrideLimitationSQL,TopX);
        }

        public bool SQLOutOfDate { get; set; }

        private IOrderable _stopContainerWhenYouReach;
        private bool _doNotWriteOutParameters;
        private ExternalDatabaseServer _cacheServer;

        public IOrderable StopContainerWhenYouReach
        {
            get { return _stopContainerWhenYouReach; }
            set
            {
                _stopContainerWhenYouReach = value;
                SQLOutOfDate = true;
            }
        }
    }
}
