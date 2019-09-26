using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryCaching.Aggregation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.QueryBuilding
{
    public class CohortQueryBuilderResult
    {
        public ExternalDatabaseServer CacheServer { get; }
        public CachedAggregateConfigurationResultsManager CacheManager { get; }

        public bool IsForContainer { get; private set; }
        public ICoreChildProvider ChildProvider { get; }
        public CohortQueryBuilderHelper Helper { get; }
        public QueryBuilderCustomArgs Customise { get; }

        private readonly StringBuilder _log = new StringBuilder();

        /// <summary>
        /// Log of all activities undertaken while building
        /// </summary>
        public string Log => _log.ToString();

        /// <summary>
        /// The allowable caching state based on the <see cref="Dependencies"/>, whether there is a
        /// <see cref="CacheServer"/> and if they are on the same or separate servers from one another
        /// </summary>
        public CacheUsage CacheUsageDecision { get; private set; }

        List<CohortQueryBuilderDependency> _dependencies = new List<CohortQueryBuilderDependency>();
        private bool _alreadyBuilt = false;

        public IReadOnlyCollection<CohortQueryBuilderDependency> Dependencies => _dependencies;

        /// <summary>
        /// Only Populated after Building.  If all <see cref="Dependencies"/> are on the same server as one another
        /// then this will contain all tables that must be queried otherwise it will be null
        /// </summary>
        public DataAccessPointCollection DependenciesSingleServer { get; private set; }

        /// <summary>
        /// The final SQL that should be executed on the <see cref="TargetServer"/>
        /// </summary>
        public string Sql { get; private set; }
        
        /// <summary>
        /// The location at which the <see cref="Sql"/> should be run (may be a data server or a cache server or they may be one and the same!)
        /// </summary>
        public DiscoveredServer TargetServer { get; set; }

        public IOrderable StopContainerWhenYouReach { get;set; }

        /// <summary>
        /// Creates a new result for a single <see cref="AggregateConfiguration"/> or <see cref="CohortAggregateContainer"/>
        /// </summary>
        /// <param name="cacheServer"></param>
        /// <param name="childProvider"></param>
        /// <param name="helper"></param>
        public CohortQueryBuilderResult(ExternalDatabaseServer cacheServer, ICoreChildProvider childProvider, CohortQueryBuilderHelper helper,QueryBuilderCustomArgs customise)
        {
            CacheServer = cacheServer;
            ChildProvider = childProvider;
            Helper = helper;
            Customise = customise;

            if(cacheServer != null)
                CacheManager = new CachedAggregateConfigurationResultsManager(CacheServer);
        }


        public void BuildFor(CohortAggregateContainer container)
        {
            ThrowIfAlreadyBuilt();
            IsForContainer = true;

            _log.AppendLine("Starting Build for " + container);
            //gather dependencies
            foreach(var cohortSet in ChildProvider.GetAllChildrenRecursively(container).OfType<AggregateConfiguration>().OrderBy(ac=>ac.Order))
                AddDependency(cohortSet);
            
            if(!Dependencies.Any())
                throw new QueryBuildingException($"There are no AggregateConfigurations under the SET container '{container}'");

            LogDependencies();

            MakeInitialCacheDecision();

            BuildDependenciesSql();

            Sql = BuildSql(container);
        }

        public void BuildFor(AggregateConfiguration configuration)
        {
            ThrowIfAlreadyBuilt();
            IsForContainer = false;

            _log.AppendLine("Starting Build for " + configuration);
            var d = AddDependency(configuration);

            LogDependencies();

            MakeInitialCacheDecision();

            BuildDependenciesSql();

            Sql = BuildSql(d);
        }


        
        private string BuildSql(CohortAggregateContainer container)
        {
            Dictionary<CohortQueryBuilderDependency, string> sqlDictionary;

            //if we are fully cached on everything
            if (Dependencies.All(d => d.SqlFullyCached != null))
            {
                SetTargetServer(GetCacheServer(),"all dependencies are fully cached"); //run on the cache server
                sqlDictionary = Dependencies.ToDictionary(k => k, v => v.SqlFullyCached); //run the fully cached sql
            }
            else
            {
                string uncached = "CacheUsageDecision is " + CacheUsageDecision +
                                  " and the following were not cached:" + string.Join(Environment.NewLine,
                                      Dependencies.Where(d => d.SqlFullyCached == null));

                switch (CacheUsageDecision)
                {
                    case CacheUsage.MustUse:
                        throw new QueryBuildingException(
                            "Could not build final SQL because some queries are not fully cached and " + uncached);

                    case CacheUsage.Opportunistic:

                        //The cache and dataset are on the same server so run it
                        SetTargetServer(DependenciesSingleServer.GetDistinctServer(),"not all dependencies are cached while " + uncached);
                        sqlDictionary =
                            Dependencies.ToDictionary(k => k,
                                v => v.SqlPartiallyCached ?? v.SqlCacheless); //run the fully cached sql
                        break;

                    case CacheUsage.AllOrNothing:

                        //It's not fully cached so we have to run it entirely uncached
                        SetTargetServer(DependenciesSingleServer.GetDistinctServer(),"not all dependencies are cached while " + uncached);
                        sqlDictionary =
                            Dependencies.ToDictionary(k => k, v => v.SqlCacheless); //run the fully cached sql
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return WriteContainers(container, TargetServer.GetQuerySyntaxHelper(), sqlDictionary,0);
        }

        private void SetTargetServer(DiscoveredServer target, string reason)
        {
            if(TargetServer != null)
                throw new InvalidOperationException("You are only supposed to pick a target server once");

            TargetServer = target;
            _log.AppendLine($"Picked TargetServer as {target} because {reason}");

        }

        private string WriteContainers(CohortAggregateContainer container, IQuerySyntaxHelper syntaxHelper,
            Dictionary<CohortQueryBuilderDependency, string> sqlDictionary, int tabs)
        {
            string sql = "";
            
            //Things we need to output
            var toWriteOut = container.GetOrderedContents().Where(IsEnabled).ToArray();

            if (toWriteOut.Any())
                sql += Environment.NewLine + TabIn( "(",tabs) + Environment.NewLine;

            bool firstEntityWritten = false;
            foreach (IOrderable toWrite in toWriteOut)
            {
                if (firstEntityWritten)
                    sql += Environment.NewLine + TabIn(GetSetOperationSql(container.Operation,syntaxHelper.DatabaseType) + Environment.NewLine + Environment.NewLine,tabs);

                if(toWrite is AggregateConfiguration)
                    sql += TabIn(sqlDictionary.Single(kvp => Equals(kvp.Key.CohortSet, toWrite)).Value, tabs);
                
                if (toWrite is CohortAggregateContainer sub)
                    sql += WriteContainers(sub, syntaxHelper, sqlDictionary, tabs++);

                //we have now written the first thing at this level of recursion - all others will need to be separated by the OPERATION e.g. UNION
                firstEntityWritten = true;

                if (StopContainerWhenYouReach != null && StopContainerWhenYouReach.Equals(toWrite))
                    if (tabs != 0)
                        throw new NotSupportedException("Stopping prematurely only works when the aggregate to stop at is in the top level container");
                    else
                        break;
            }

            //if we outputted anything
            if (toWriteOut.Any())
                sql += Environment.NewLine + TabIn(")",tabs) + Environment.NewLine ;

            return sql;
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
        /// <summary>
        /// Returns the SQL keyword for the <paramref name="currentContainerOperation"/>
        /// </summary>
        /// <param name="currentContainerOperation"></param>
        /// <returns></returns>
        protected virtual string GetSetOperationSql(SetOperation currentContainerOperation,DatabaseType dbType)
        {
            if (dbType == DatabaseType.MySql)
                throw new NotSupportedException("INTERSECT / UNION / EXCEPT are not supported by MySql caches");

            switch (currentContainerOperation)
            {
                case SetOperation.UNION:
                    return "UNION";
                case SetOperation.INTERSECT:
                    return "INTERSECT";
                case SetOperation.EXCEPT:
                    if (dbType == DatabaseType.Oracle)
                        return "MINUS";

                    return "EXCEPT";
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentContainerOperation), currentContainerOperation, null);
            }
        }

        private string BuildSql(CohortQueryBuilderDependency dependency)
        {
            //if we are fully cached on everything
            if (dependency.SqlFullyCached != null)
            {
                SetTargetServer(GetCacheServer()," dependency is cached"); //run on the cache server
                return dependency.SqlFullyCached; //run the fully cached sql
            }

            switch (CacheUsageDecision)
            {
                case CacheUsage.MustUse:
                    throw new QueryBuildingException("Could not build final SQL because " + dependency +" is not fully cached and CacheUsageDecision is " + CacheUsageDecision);

                case CacheUsage.Opportunistic:

                    //The cache and dataset are on the same server so run it
                    SetTargetServer(DependenciesSingleServer.GetDistinctServer(),"data and cache are on the same server");
                    return dependency.SqlPartiallyCached ?? dependency.SqlCacheless;
                case CacheUsage.AllOrNothing:

                    //It's not fully cached so we have to run it entirely uncached
                    SetTargetServer(DependenciesSingleServer.GetDistinctServer(),"cache and data are on seperate servers / access credentials and not all datasets are in the cache");
                    return dependency.SqlCacheless;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private DiscoveredServer GetCacheServer()
        {
            return CacheServer.Discover(DataAccessContext.InternalDataProcessing).Server;
        }

        private void BuildDependenciesSql()
        {
            foreach (var d in Dependencies)
                d.Build(this);
        }


        private CohortQueryBuilderDependency AddDependency(AggregateConfiguration cohortSet)
        {
            var join = ChildProvider.AllJoinUses.Where(j => j.AggregateConfiguration_ID == cohortSet.ID).ToArray();

            if(join.Length > 1)
                throw new NotSupportedException($"There are {join.Length} joins configured to AggregateConfiguration {cohortSet}");

            var d = new CohortQueryBuilderDependency(cohortSet, join.SingleOrDefault(), ChildProvider);
            _dependencies.Add(d);

            return d;
        }

        private void MakeInitialCacheDecision()
        {
            if (CacheServer == null)
                SetCacheUsage(CacheUsage.AllOrNothing,"there is no cache server");
            else
                SetCacheUsage(CacheUsage.Opportunistic, "there is a cache server available (dependencies have not yet been evaluated)");
            
            DependenciesSingleServer =  new DataAccessPointCollection(true);

            foreach (var dependency in Dependencies)
            {
                foreach (ITableInfo dependantTable in dependency.CohortSet.Catalogue.GetTableInfoList(false))
                    HandleDependency(dependency.CohortSet, dependantTable);
                
                if(dependency.JoinedTo != null)
                    foreach (ITableInfo dependantTable in dependency.JoinedTo.Catalogue.GetTableInfoList(false))
                        HandleDependency(dependency.JoinedTo,dependantTable);
            }
        }

        private void HandleDependency(AggregateConfiguration aggregate, ITableInfo dependantTable)
        {
            _log.AppendLine($"{aggregate} depends on " + dependantTable);
                    
            //if dependencies are on different servers / access credentials
            if(DependenciesSingleServer != null)
                if (!DependenciesSingleServer.TryAdd(dependantTable))
                {
                    //we can no longer establish a consistent connection to all the dependencies
                    DependenciesSingleServer = null;

                    //if there's no cache server that's a problem!
                    if(CacheServer == null)
                        throw new QueryBuildingException($"Table {dependantTable} is on a different server (or uses different access credentials) from previously seen dependencies and no QueryCache is configured");
                    
                    //there IS a cache so we now Must use it
                    if(CacheUsageDecision != CacheUsage.MustUse)
                        SetCacheUsage(CacheUsage.MustUse,$"Table {dependantTable} is on a different server (or uses different access credentials) from previously seen dependencies.  Therefore the QueryCache MUST be used for all dependencies");
                }

            if(DependenciesSingleServer != null && CacheServer != null && CacheUsageDecision == CacheUsage.Opportunistic)
                SetCacheUsage(CacheUsage.AllOrNothing,"All datasets are on one server/access credentials while Cache is on a seperate one");
        }
        private void LogDependencies()
        {
            _log.AppendLine("Found Dependencies:" + Environment.NewLine +
                            string.Join(Environment.NewLine,Dependencies));
        }

        
        private void SetCacheUsage(CacheUsage value, string thereIsNoCacheServer)
        {
            CacheUsageDecision = value;
            _log.AppendLine($"Setting {nameof(CacheUsageDecision)} to {value} because {thereIsNoCacheServer}");
        }
        
        private void ThrowIfAlreadyBuilt()
        {
            if (_alreadyBuilt)
                throw new InvalidOperationException("Dependencies have already been built");
            
            _alreadyBuilt = true;
        }
        
        public string TabIn(string str, int numberOfTabs)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            var tabs = new String('\t', numberOfTabs);
            return tabs + str.Replace(Environment.NewLine, Environment.NewLine + tabs);
        }


    }

    public class CohortQueryBuilderDependency
    {
        private readonly ICoreChildProvider _childProvider;
        
        /// <summary>
        /// The primary table being queried
        /// </summary>
        public AggregateConfiguration CohortSet { get; }

        /// <summary>
        /// The relationship object describing the JOIN relationship between <see cref="CohortSet"/> and another optional table
        /// </summary>
        public JoinableCohortAggregateConfigurationUse PatientIndexTableIfAny { get; }

        /// <summary>
        /// The aggregate (query) referenced by <see cref="PatientIndexTableIfAny"/>
        /// </summary>
        public AggregateConfiguration JoinedTo { get; }

        /// <summary>
        /// The raw SQL that can be used to join the <see cref="CohortSet"/> and <see cref="PatientIndexTableIfAny"/> (if there is one).  Null if they exist
        /// on different servers (this is allowed only if the <see cref="CohortSet"/> is on the same server as the cache while the <see cref="PatientIndexTableIfAny"/>
        /// is remote).
        /// </summary>
        public string SqlCacheless { get; private set; }
        
        /// <summary>
        /// The raw SQL for the <see cref="CohortSet"/> with a join against the cached artifact for the <see cref="PatientIndexTableIfAny"/>
        /// </summary>
        public string SqlPartiallyCached { get;  private set;}
        
        /// <summary>
        /// Sql for a single cache fetch  that pulls the cached result of the <see cref="CohortSet"/> joined to <see cref="PatientIndexTableIfAny"/> (if there was any)
        /// </summary>
        public string SqlFullyCached { get;  private set;}

        public string SqlJoinableCacheless { get; private set; }
        
        public string SqlJoinableCached { get; private set; }

        public CohortQueryBuilderDependency(AggregateConfiguration cohortSet,
            JoinableCohortAggregateConfigurationUse patientIndexTableIfAny, ICoreChildProvider childProvider)
        {
            _childProvider = childProvider;
            CohortSet = cohortSet;
            PatientIndexTableIfAny = patientIndexTableIfAny;

            if (PatientIndexTableIfAny != null)
            {
                var join = _childProvider.AllJoinables.SingleOrDefault(j =>
                    j.ID == PatientIndexTableIfAny.JoinableCohortAggregateConfiguration_ID);

                if(join == null)
                    throw new Exception("ICoreChildProvider did not know about the provided patient index table");

                JoinedTo = _childProvider.AllAggregateConfigurations.SingleOrDefault(ac =>
                    ac.ID == join.AggregateConfiguration_ID);

                if(JoinedTo == null)
                    throw new Exception("ICoreChildProvider did not know about the provided patient index table AggregateConfiguration");
            }
        }

        public override string ToString()
        {
            return CohortSet.Name + (JoinedTo != null ? PatientIndexTableIfAny.JoinType + " JOIN " + JoinedTo.Name : "");
        }

        public void Build(CohortQueryBuilderResult parent)
        {
            bool isSolitaryPatientIndexTable = CohortSet.IsJoinablePatientIndexTable();

            if (JoinedTo != null)
            {
                SqlJoinableCacheless = parent.Helper.GetSQLForAggregate(JoinedTo,new QueryBuilderArgs(parent.Customise));
                SqlJoinableCached = GetCachFetchSqlIfPossible(parent,JoinedTo,SqlJoinableCacheless,true);
            }


            if (isSolitaryPatientIndexTable)
            {
                //explicit execution of a patient index table on it's own
                //the full uncached SQL for the query
                SqlCacheless = parent.Helper.GetSQLForAggregate(CohortSet,new QueryBuilderArgs(parent.Customise));

                if(SqlJoinableCached != null)
                    throw new QueryBuildingException("Patient index tables can't use other patient index tables!");
            }
            else
            {
                //the full uncached SQL for the query
                SqlCacheless = parent.Helper.GetSQLForAggregate(CohortSet,
                    new QueryBuilderArgs(PatientIndexTableIfAny, JoinedTo,
                        parent.TabIn(SqlJoinableCacheless, 1),parent.Customise));

                
                //if the joined to table is cached we can generate a partial too with full sql for the outer sql block and a cache fetch join
                if (SqlJoinableCached != null)
                    SqlPartiallyCached = parent.Helper.GetSQLForAggregate(CohortSet,
                        new QueryBuilderArgs(PatientIndexTableIfAny, JoinedTo,
                            SqlJoinableCached,parent.Customise));
            }
            
            //We would prefer a cache hit on the exact uncached SQL
            SqlFullyCached = GetCachFetchSqlIfPossible(parent, CohortSet, SqlCacheless, isSolitaryPatientIndexTable);

            //but if that misses we would take a cache hit of an execution of the SqlPartiallyCached
            if(SqlFullyCached == null && SqlPartiallyCached != null)
                SqlFullyCached = GetCachFetchSqlIfPossible(parent,CohortSet,SqlPartiallyCached,isSolitaryPatientIndexTable);
        }

        private string GetCachFetchSqlIfPossible(CohortQueryBuilderResult parent,AggregateConfiguration aggregate, string sql, bool isPatientIndexTable)
        {
            if (parent.CacheManager == null)
                return null;

            var existingTable = parent.CacheManager.GetLatestResultsTable(aggregate, isPatientIndexTable
            ?AggregateOperation.JoinableInceptionQuery:AggregateOperation.IndexedExtractionIdentifierList , sql);
                
            //if there is a cached entry matching the cacheless SQL then we can just do a select from it (in theory)
            if(existingTable != null)
                return
                    CachedAggregateConfigurationResultsManager.CachingPrefix + aggregate.Name + @"*/" + Environment.NewLine +
                    "select * from " + existingTable.GetFullyQualifiedName() + Environment.NewLine;

            return null;
        }
    }


    public enum CacheUsage
    {
        /// <summary>
        /// The cache must be used and all Dependencies must be cached.  This happens if dependencies are on different servers / data access
        /// credentials.  Or the query being built involves SET operations which are not supported by the DBMS of the dependencies (e.g. MySql UNION / INTERSECT etc).
        /// </summary>
        MustUse,

        /// <summary>
        /// All dependencies are on the same server as the cache.  Therefore we can mix and match where we fetch tables from
        /// (live table or cache) depending on whether the cache contains an entry for it or not.
        /// </summary>
        Opportunistic,

        /// <summary>
        /// All dependencies are on the same server but the cache is on a different server.  Therefore we can either
        /// run a fully cached set of queries or we cannot run any cached queries
        /// </summary>
        AllOrNothing
    }
}
