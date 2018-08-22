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

        List<QueryTimeColumn> SelectColumns { get; }
        List<TableInfo> TablesUsedInQuery { get; }
        List<IFilter> Filters { get; }
        List<JoinInfo> JoinsUsedInQuery { get; }

        IContainer RootFilterContainer{get;set;}
        
        bool CheckSyntax { get; set; }
        TableInfo PrimaryExtractionTable { get; }
        
        bool Sort { get; set; }
        ParameterManager ParameterManager { get;}

        void AddColumnRange(IColumn[] columnsToAdd);
        void AddColumn(IColumn col);
        
        void RegenerateSQL();

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
        /// <para>Changing this manually is not recommended, specify it in the <see cref="IQueryBuilder"/> constructor instead</para>
        /// </summary>
        CustomLine TopXCustomLine { get; set; }
    }
}
