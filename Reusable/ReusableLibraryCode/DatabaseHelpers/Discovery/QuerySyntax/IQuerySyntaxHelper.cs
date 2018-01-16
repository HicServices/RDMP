using System;
using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    /// <summary>
    /// Cross database type functionality for helping build SQL commands that will work regardless of DatabaseType (Microsoft Sql Server / MySql etc).  Describes
    /// how to translate broad requirements like 'database type capable of storing strings up to 10 characters long' into a specific implementation e.g. 
    /// 'varchar(10)' in Microsoft SQL Server and 'varchar2(10)' in Oracle (See ITypeTranslater).
    /// 
    /// Also includes features such as qualifying database entities [MyDatabase]..[MyTable].[MyColumn] in Sql Server vs `MyDatabase`.`MyTable`.`MyColumn` in 
    /// MySql.
    /// 
    /// </summary>
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
        string GetParameterDeclaration(string proposedNewParameterName, string sqlType);

        string AliasPrefix { get; }
        bool SplitLineIntoSelectSQLAndAlias(string lineToSplit, out string selectSQL, out string alias);

        string GetScalarFunctionSql(MandatoryScalarFunctions function);
        string GetSensibleTableNameFromString(string potentiallyDodgyName);
    }

    public enum MandatoryScalarFunctions
    {
        GetTodaysDate
        
    }
}