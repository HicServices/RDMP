using System;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace ReusableLibraryCode
{
    public class SqlSyntaxHelper
    {
        //todo it is hacky to just wrap this here! for no good reason
        static MicrosoftQuerySyntaxHelper _syntaxHelper = new MicrosoftQuerySyntaxHelper();
        public static string GetRuntimeName(string s)
        {
            return _syntaxHelper.GetRuntimeName(s);
        }
        
        public static MySqlCommand ConvertMsSqlCommandToMySql(SqlCommand cmd)
        {
            //mysql doesnt like square brackets, it uses backwards single quotes instead yay
            MySqlCommand toReturn = new MySqlCommand(cmd.CommandText.Replace("[", "`").Replace("]", "`"));

            foreach (SqlParameter parameter in cmd.Parameters)
                toReturn.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);

            return toReturn;
        }
        
        public static string WrapStringWithHashingAlgorithm(string pattern, string SelectSQL, string salt)
        {
            return String.Format(pattern, SelectSQL, salt);
        }


        public static string EnsureFullyQualifiedMicrosoftSQL(string databaseName, string tableName)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                return null;

            if (String.IsNullOrWhiteSpace(databaseName))
                return tableName;

            //it is probably already fully qualified
            if (tableName.Contains("."))
                return tableName;

            return "[" + GetRuntimeName(databaseName) + "]..[" + GetRuntimeName(tableName) + "]";
        }


        public static string EnsureFullyQualified(string databaseName, string tableName, string columnName)
        {
            if (String.IsNullOrWhiteSpace(columnName))
                return null;

            if (String.IsNullOrWhiteSpace(databaseName) || String.IsNullOrWhiteSpace(tableName))
                return columnName;

            //it is probably already fully qualified
            if (columnName.Contains("."))
                return columnName;

            return "[" + GetRuntimeName(databaseName) + "]..[" + GetRuntimeName(tableName) + "].[" + GetRuntimeName(columnName) + "]";
        }


        public static string GetSensibleTableNameFromString(string potentiallyDodgyName)
        {
            potentiallyDodgyName = GetRuntimeName(potentiallyDodgyName);

            //replace anything that isn't a digit, letter or underscore with underscores
            Regex r = new Regex("[^A-Za-z0-9_]");
            string adjustedHeader = r.Replace(potentiallyDodgyName, "_");
            
            //if it starts with a digit (illegal) put an underscore before it
            if (Regex.IsMatch(adjustedHeader, "^[0-9]"))
                adjustedHeader = "_" + adjustedHeader;

            return adjustedHeader;
        }
    }
}
