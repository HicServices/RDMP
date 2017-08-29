using System;
using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    public interface IQuerySyntaxHelper
    {
        ITypeTranslater TypeTranslater { get; }
        IAggregateHelper AggregateHelper { get; }

        string GetRuntimeName(string s);
        string EnsureFullyQualified(string databaseName,string schemaName, string tableName);
        string EnsureFullyQualified(string databaseName, string schemaName,string tableName, string columnName, bool isTableValuedFunction = false);
        string Escape(string sql);

        TopXResponse HowDoWeAchieveTopX(int x);
        string GetParameterDeclaration(string proposedNewParameterName, DatabaseTypeRequest request);

        string AliasPrefix { get; }
        bool SplitLineIntoSelectSQLAndAlias(string lineToSplit, out string selectSQL, out string alias);

        string GetScalarFunctionSql(MandatoryScalarFunctions function);
    }

    public enum MandatoryScalarFunctions
    {
        GetTodaysDate
        
    }
}