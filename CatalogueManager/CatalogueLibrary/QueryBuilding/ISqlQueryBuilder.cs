using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding.Parameters;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// Shared interface for both the RDMP query builders (QueryBuilder and AggregateBuilder).  Query Building in RDMP consists of defining mono atomic comonents 
    /// ('I want this column',  'I want LimitationSQL: DISTINCT' etc) then the ISqlQueryBuilder turns it into SQL 
    /// (See namespace CatalogueLibraryTests.Integration.QueryBuildingTests.QueryBuilderTests).
    /// 
    /// <para>The main purpose of this interface is to move common logic such as finding which TableInfos to join and resolving Parameter overriding into SqlQueryBuilderHelper </para>
    /// </summary>
    public interface ISqlQueryBuilder
    {
        /// <summary>
        /// Builds (if out of date) and returns the SQL query that reflects the currently selected columns, tables etc of the query builder.
        /// </summary>
        string SQL { get; }

        /// <summary>
        /// True if changes have been made to the state of this class since you last fetched <see cref="SQL"/>.
        /// </summary>
        bool SQLOutOfDate { get; set; }

        /// <summary>
        /// Any SQL to inject between SELECT and the first column, e.g. "DISTINCT"
        /// </summary>
        string LimitationSQL { get;}

        /// <summary>
        /// Wrapper class for the columns you added to the query with <see cref="AddColumn"/>).  After fetching the query (See <see cref="SQL"/>) this will be populated with 
        /// facts about the <see cref="IColumn"/> including it's status in lookup joins etc 
        /// </summary>
        List<QueryTimeColumn> SelectColumns { get; }

        /// <summary>
        /// All tables identified during the query as being required to build the query (based on the <see cref="IColumn"/>) and any forced joins etc.
        /// 
        /// <para>Do not modify this manually</para>
        /// </summary>
        List<TableInfo> TablesUsedInQuery { get; }

        /// <summary>
        /// All filters found in the <see cref="RootFilterContainer"/> (recursively)
        /// <para>Do not modify this manually</para>
        /// </summary>
        List<IFilter> Filters { get; }

        /// <summary>
        /// List of all the <see cref="JoinInfo"/> found for joining the <see cref="TablesUsedInQuery"/> for building the query.
        /// <para>Do not modify this manually</para>
        /// </summary>
        List<JoinInfo> JoinsUsedInQuery { get; }

        /// <summary>
        /// The <see cref="IContainer"/> (AND / OR) that contains all the lines of WHERE SQL (<see cref="IFilter"/>) including subcontainers.
        /// </summary>
        IContainer RootFilterContainer{get;set;}
        
        /// <summary>
        /// True to check the syntax of columns, parameters etc.  This should result in SyntaxErrorException being thrown when generating the SQL if it is substantially malformed
        /// </summary>
        bool CheckSyntax { get; set; }

        /// <summary>
        /// The single <see cref="TableInfo"/> amongst <see cref="TablesUsedInQuery"/> that was <see cref="TableInfo.IsPrimaryExtractionTable"/>
        /// </summary>
        TableInfo PrimaryExtractionTable { get; }
        
        /// <summary>
        /// Manages the addition of <see cref="ISqlParameter"/> to the <see cref="ISqlQueryBuilder"/> in a scoped way (globals, query level etc).
        /// 
        /// </summary>
        ParameterManager ParameterManager { get;}

        /// <summary>
        /// Adds the provided columns to the query as lines of SELECT SQL
        /// </summary>
        /// <param name="columnsToAdd"></param>
        void AddColumnRange(IColumn[] columnsToAdd);

        /// <summary>
        /// Adds the column to the query as single line of SELECT SQL
        /// </summary>
        /// <param name="col"></param>
        void AddColumn(IColumn col);
        

        /// <summary>
        /// Manually forces the query builder to rebuild the query.  This will update <see cref="TablesUsedInQuery"/>, <see cref="SelectColumns"/> etc.
        /// 
        /// <para>This automatically happens if you make a change to the state of the class and call <see cref="SQL"/> property (if the change resulted in <see cref="SQLOutOfDate"/> becoming true).</para>
        /// </summary>
        void RegenerateSQL();

        /// <summary>
        /// Returns all the <see cref="Lookup"/> classes found and used in the query.
        /// 
        /// <para>This only happens if you have a <see cref="Lookup"/> configured and your query has both the code column and the description column in it's SELECT columns</para>
        /// </summary>
        /// <returns></returns>
        IEnumerable<Lookup> GetDistinctRequiredLookups();

        /// <summary>
        /// List of all added CustomLines so far, use <see cref="AddCustomLine"/> to add new ones.
        /// </summary>
        List<CustomLine> CustomLines { get; }

        /// <summary>
        /// Add the provided text at the specified position in the query.  This will result in the query being rebuilt when you call <see cref="SQL"/>. 
        ///  
        /// </summary>
        /// <param name="text">The SQL to inject</param>
        /// <param name="positionToInsert">The position in the query to inject it</param>
        /// <returns>The CustomLine that was created and added to the query </returns>
        CustomLine AddCustomLine(string text, QueryComponent positionToInsert);

        /// <summary>
        /// The line of SQL code and it's position in the query which results in result limiting (e.g. LIMIT X in MySql and TOP X in SqlServer)
        /// 
        /// <para>Changing this manually is not recommended, specify it in the <see cref="ISqlQueryBuilder"/> constructor instead</para>
        /// </summary>
        CustomLine TopXCustomLine { get; set; }
    }
}
