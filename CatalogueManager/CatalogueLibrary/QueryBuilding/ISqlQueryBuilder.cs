using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding.Parameters;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.QueryBuilding
{
    public interface ISqlQueryBuilder
    {
        string SQL { get; }
        bool SQLOutOfDate { get; set; }

        string LimitationSQL { get;}
        int LineCount { get; }
        
        List<QueryTimeColumn> SelectColumns { get; }
        List<TableInfo> TablesUsedInQuery { get; }
        List<IFilter> Filters { get; }
        List<JoinInfo> JoinsUsedInQuery { get; }

        IContainer RootFilterContainer{get;set;}
        
        bool CheckSyntax { get; set; }
        TableInfo PrimaryExtractionTable { get; }
        
        bool Sort { get; set; }
        int CurrentLine { get; }
        ParameterManager ParameterManager { get;}

        void AddColumnRange(IColumn[] columnsToAdd);
        void AddColumn(IColumn col);

        int GetLineNumberForColumn(IColumn column);

        void RegenerateSQL();

        string TakeNewLine();
        IEnumerable<Lookup> GetDistinctRequiredLookups();
        List<CustomLine> CustomLines { get; }
        CustomLine TopXCustomLine { get; set; }
    }
}
