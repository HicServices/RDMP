// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.Core.CatalogueLibrary.Data.Cohort.Joinables;
using Rdmp.Core.CatalogueLibrary.FilterImporting;
using Rdmp.Core.CatalogueLibrary.QueryBuilding;
using Rdmp.Core.CatalogueLibrary.QueryBuilding.Parameters;
using Rdmp.Core.CatalogueLibrary.Spontaneous;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.CohortCreation.QueryBuilding
{
    /// <summary>
    /// Helper for CohortQueryBuilder which contains code for building individual cohort identification subqueries.  Subqueries are actually built by 
    /// AggregateBuilder but this class handles tab indentation, parameter renaming (where there are other subqueries with conflicting sql parameter names), 
    /// injecting globals etc.
    /// </summary>
    public class CohortQueryBuilderHelper
    {
        private readonly ISqlParameter[] _globals;
        public ParameterManager ParameterManager { get; set; }
        public ExternalDatabaseServer CacheServer { get; set; }

        public int CountOfSubQueries = 0;
        public int CountOfCachedSubQueries = 0;

        public CohortQueryBuilderHelper(ISqlParameter[] globals,ParameterManager parameterManager, ExternalDatabaseServer cacheServer)
        {
            _globals = globals;
            ParameterManager = parameterManager;
            CacheServer = cacheServer;
        }

        public string GetSQLForAggregate(AggregateConfiguration aggregate, int tabDepth, bool isJoinAggregate = false, string overrideSelectList = null, string overrideLimitationSQL=null, int topX = -1)
        {
            string toReturn ="";

            CountOfSubQueries++;
            
            string tabs = "";
            string tabplusOne = "";

            if (tabDepth != -1)
                GetTabs(tabDepth, out tabs, out tabplusOne);

            //make sure it is a valid configuration
            string reason;
            if (!aggregate.IsAcceptableAsCohortGenerationSource(out reason))
                throw new QueryBuildingException("Cannot generate a cohort using AggregateConfiguration " + aggregate + " because:" + reason);

            //get the extraction identifier (method IsAcceptableAsCohortGenerationSource will ensure this linq returns 1 so no need to check again)
            AggregateDimension extractionIdentifier = aggregate.AggregateDimensions.Single(d => d.IsExtractionIdentifier);

            //create a builder but do it manually, we care about group bys etc or count(*) even 
            AggregateBuilder builder;

            //we are getting SQL for a cohort identification aggregate without a HAVING/count statement so it is actually just 'select patientIdentifier from tableX'
            if (string.IsNullOrWhiteSpace(aggregate.HavingSQL) && string.IsNullOrWhiteSpace(aggregate.CountSQL))
            {
                //select list is the extraction identifier
                string selectList;

                if (!isJoinAggregate)
                    selectList = extractionIdentifier.SelectSQL +  (extractionIdentifier.Alias != null ? " " + extractionIdentifier.Alias: "");
                else
                    //unless we are also including other columns because this is a patient index joinable inception query
                    selectList = string.Join("," + Environment.NewLine + tabs,
                        aggregate.AggregateDimensions.Select(e => e.SelectSQL + (e.Alias != null ? " " + e.Alias : ""))); //joinable patient index tables have patientIdentifier + 1 or more other columns

                if (overrideSelectList != null)
                    selectList = overrideSelectList;

                string limitationSQL = overrideLimitationSQL ?? "distinct";

                //select list is either [chi] or [chi],[mycolumn],[myexcitingcol] (in the case of a patient index table)
                builder = new AggregateBuilder(limitationSQL, selectList, aggregate, aggregate.ForcedJoins);

                if (topX != -1)
                    builder.AggregateTopX = new SpontaneouslyInventedAggregateTopX(new MemoryRepository(), topX,AggregateTopXOrderByDirection.Descending,null);

                //false makes it skip them in the SQL it generates (it uses them only in determining JOIN requirements etc but since we passed in the select SQL explicitly it should be the equivellent of telling the query builder to generate a regular select 
                if(!isJoinAggregate)
                    builder.AddColumn(extractionIdentifier, false);
                else
                    builder.AddColumnRange(aggregate.AggregateDimensions.ToArray(), false);
            }
            else
            {
                if (overrideSelectList != null)
                    throw new NotSupportedException("Cannot override Select list on aggregates that have HAVING / Count SQL configured in them");

                builder = new AggregateBuilder("distinct", aggregate.CountSQL, aggregate, aggregate.ForcedJoins);

                //add the extraction information and do group by it
                if (!isJoinAggregate)
                    builder.AddColumn(extractionIdentifier, true);
                else
                    builder.AddColumnRange(aggregate.AggregateDimensions.ToArray(), true);//it's a joinable inception query (See JoinableCohortAggregateConfiguration) - these are allowed additional columns

                builder.DoNotWriteOutOrderBy = true;
            }
            
            AddJoinablesToBuilder(builder, aggregate,tabDepth);

            //set the where container
            builder.RootFilterContainer = aggregate.RootFilterContainer;

            string builderSqlVerbatimForCheckingAgainstCache = null;

            if (CacheServer != null)
                builderSqlVerbatimForCheckingAgainstCache = GetSqlForBuilderForCheckingAgainstCache(builder);

            //we will be harnessing the parameters via ImportAndElevate so do not add them to the SQL directly
            builder.DoNotWriteOutParameters = true;
            var builderSqlWithoutParameters = builder.SQL;

            //if we have caching 
            if (CacheServer != null)
            {
                CachedAggregateConfigurationResultsManager manager = new CachedAggregateConfigurationResultsManager(CacheServer);
                var existingTable = manager.GetLatestResultsTable(aggregate, isJoinAggregate? AggregateOperation.JoinableInceptionQuery: AggregateOperation.IndexedExtractionIdentifierList, builderSqlVerbatimForCheckingAgainstCache);

                //if we have the answer already 
                if (existingTable != null)
                {
                    //reference the answer table
                    CountOfCachedSubQueries++;
                    toReturn += tabplusOne +  CachedAggregateConfigurationResultsManager.CachingPrefix + aggregate.Name + @"*/"+Environment.NewLine;
                    toReturn += tabplusOne + "select * from " + existingTable.GetFullyQualifiedName() + Environment.NewLine;
                    return toReturn;
                }

                //we do not have an uptodate answer available in the cache :(
            }

            //get the SQL from the builder (for the current configuration) - without parameters
            string currentBlock = builderSqlWithoutParameters;

            //import parameters unless caching was used
            Dictionary<string, string> renameOperations;
            ParameterManager.ImportAndElevateResolvedParametersFromSubquery(builder.ParameterManager, out renameOperations);

            //rename in the SQL too!
            foreach (KeyValuePair<string, string> kvp in renameOperations)
                currentBlock = ParameterCreator.RenameParameterInSQL(currentBlock, kvp.Key, kvp.Value);

            //tab in the current block
            toReturn += tabplusOne + currentBlock.Replace("\r\n", "\r\n" + tabplusOne);
            return toReturn;
        }

        public void AddJoinablesToBuilder(AggregateBuilder builder, AggregateConfiguration aggregate, int tabDepth)
        {
            var users = aggregate.Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfigurationUse>(aggregate);

            foreach (var use in users)
            {

                var joinableTableAlias = use.GetJoinTableAlias();

                var joinAggregate = use.JoinableCohortAggregateConfiguration.AggregateConfiguration;

                var identifierCol = aggregate.AggregateDimensions.Single();
                var identifierColInJoinAggregate = joinAggregate.AggregateDimensions.SingleOrDefault(d => d.IsExtractionIdentifier);

                if (identifierColInJoinAggregate == null)
                    throw new QueryBuildingException("AggregateConfiguration " + aggregate + " uses a join aggregate (patient index aggregate) of " + joinAggregate + " but that AggregateConfiguration does not have an IsExtractionIdentifier dimension so how are we supposed to join these tables on the patient identifier?");

                var joinSQL = GetSQLForAggregate(joinAggregate, tabDepth + 1, true);

                string joinDirection = use.GetJoinDirectionSQL();

                // will end up with something like this where 51 is the ID of the joinTable:
                // LEFT Join (***INCEPTION QUERY***)ix51 on ["+TestDatabaseNames.Prefix+@"ScratchArea]..[BulkData].[patientIdentifier] = ix51.patientIdentifier

                builder.AddCustomLine(" " + joinDirection + " Join (" + Environment.NewLine + joinSQL + Environment.NewLine + ")" + joinableTableAlias + Environment.NewLine + "on " + identifierCol.SelectSQL + " = " + joinableTableAlias + "." + identifierColInJoinAggregate.GetRuntimeName(),QueryComponent.JoinInfoJoin);
            }
        }

        private string GetSqlForBuilderForCheckingAgainstCache(AggregateBuilder builder)
        {
            string toReturn;

            if (builder.ParameterManager.ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].Any())
                throw new NotSupportedException("Why did builder already have globals?");

            //make sure the builder has the globals since it did have globals when it was committed to the cache
            foreach (var g in _globals)
                builder.ParameterManager.AddGlobalParameter(g);

            //the result of the aggregate only (verbatim so that we can check vs the cache for this exact SQL being run in the recent past)
            toReturn = builder.SQL;

            //now that we have the verbatim SQL we should clear the globals since we will be processing all the aggregates into the CompositeQueryLevel together we don't want globals sneaking in there - caller might reuse the builder you see and we should leave it in the same state we found it
            builder.ParameterManager.ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].Clear();

            return toReturn;
        }


        public void GetTabs(int tabDepth, out string tabs, out string tabplusOne)
        {
            tabs = "";
            for (int i = 0; i < tabDepth; i++)
                tabs += "\t";

            tabplusOne = tabs + "\t";
        }
   
    }
}