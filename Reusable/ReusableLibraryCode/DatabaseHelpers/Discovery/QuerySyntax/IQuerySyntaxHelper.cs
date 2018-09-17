using System;
using System.Collections;
using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    /// <summary>
    /// Cross database type functionality for helping build SQL commands that will work regardless of DatabaseType (Microsoft Sql Server / MySql etc).  Describes
    /// how to translate broad requirements like 'database type capable of storing strings up to 10 characters long' into a specific implementation e.g. 
    /// 'varchar(10)' in Microsoft SQL Server and 'varchar2(10)' in Oracle (See ITypeTranslater).
    /// 
    /// <para>Also includes features such as qualifying database entities [MyDatabase]..[MyTable].[MyColumn] in Sql Server vs `MyDatabase`.`MyTable`.`MyColumn` in 
    /// MySql.</para>
    /// 
    /// </summary>
    public interface IQuerySyntaxHelper
    {
        ITypeTranslater TypeTranslater { get; }
        IAggregateHelper AggregateHelper { get; }
        IUpdateHelper UpdateHelper { get; set; }

        char ParameterSymbol { get; }

        string GetRuntimeName(string s);

        DatabaseType DatabaseType {get;}
        
        /// <summary>
        /// Ensures that the supplied single entity object e.g. "mytable" , "mydatabase, "[mydatabase]", "`mydatabase` etc is returned wrapped in appropriate qualifiers for
        /// the database we are providing syntax for
        /// </summary>
        /// <param name="databaseOrTableName"></param>
        /// <returns></returns>
        string EnsureWrapped(string databaseOrTableName);

        string EnsureFullyQualified(string databaseName,string schemaName, string tableName);
        string EnsureFullyQualified(string databaseName, string schemaName,string tableName, string columnName, bool isTableValuedFunction = false);
        string Escape(string sql);

        TopXResponse HowDoWeAchieveTopX(int x);
        string GetParameterDeclaration(string proposedNewParameterName, DatabaseTypeRequest request);
        string GetParameterDeclaration(string proposedNewParameterName, string sqlType);
        
        bool IsValidParameterName(string parameterSQL);

        string AliasPrefix { get; }
        bool SplitLineIntoSelectSQLAndAlias(string lineToSplit, out string selectSQL, out string alias);

        string GetScalarFunctionSql(MandatoryScalarFunctions function);
        string GetSensibleTableNameFromString(string potentiallyDodgyName);
        
        /// <summary>
        /// The SQL that would be valid for a CREATE TABLE statement that would result in a given column becoming auto increment e.g. "IDENTITY(1,1)"
        /// </summary>
        /// <returns></returns>
        string GetAutoIncrementKeywordIfAny();

        /// <summary>
        /// Get a list of functions to SQL code (including parameter names).  This is used primarily in autocomplete situations where the user wants to
        /// know the available functions within the targeted dbms.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetSQLFunctionsDictionary();

        bool IsBasicallyNull(object value);
        bool IsTimeout(Exception exception);
        
        /// <summary>
        /// Returns SQL that will wrap a single line of SQL with the SQL to calculate MD5 hash e.g. change `MyTable`.`MyColumn` to md5(`MyTable`.`MyColumn`)
        /// <para>The SQL might include transform functions e.g. UPPER etc</para>
        /// </summary>
        /// <param name="selectSql"></param>
        /// <returns></returns>
        string HowDoWeAchieveMd5(string selectSql);
    }

    public enum MandatoryScalarFunctions
    {
        None = 0,

        /// <summary>
        /// A scalar function which must return todays datetime.  Must be valid as a column default too
        /// </summary>
        GetTodaysDate,
        
        /// <summary>
        /// A scalar function which must return a new random GUID.
        /// </summary>
        GetGuid
    }
}
