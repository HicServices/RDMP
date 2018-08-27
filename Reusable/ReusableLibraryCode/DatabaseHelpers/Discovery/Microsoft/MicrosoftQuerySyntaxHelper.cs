using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft.Update;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftQuerySyntaxHelper : QuerySyntaxHelper
    {
        public MicrosoftQuerySyntaxHelper() : base(new MicrosoftSQLTypeTranslater(),new MicrosoftSQLAggregateHelper(),new MicrosoftSQLUpdateHelper(),DatabaseType.MicrosoftSQLServer)
        {
        }

        public override string DatabaseTableSeparator
        {
           get { return "."; }
        }
        
        public override TopXResponse HowDoWeAchieveTopX(int x)
        {
            return new TopXResponse("TOP " + x, QueryComponent.SELECT);
        }

        public override string GetParameterDeclaration(string proposedNewParameterName, string sqlType)
        {
            return "DECLARE " + proposedNewParameterName + " AS " + sqlType + ";";
        }

        public override string GetScalarFunctionSql(MandatoryScalarFunctions function)
        {
            switch (function)
            {
                case MandatoryScalarFunctions.GetTodaysDate:
                    return "GETDATE()";
               case MandatoryScalarFunctions.GetGuid:
                    return "newid()";
                default:
                    throw new ArgumentOutOfRangeException("function");
            }
        }

        public override string GetAutoIncrementKeywordIfAny()
        {
            return "IDENTITY(1,1)";
        }

        public override Dictionary<string, string> GetSQLFunctionsDictionary()
        {
            return new Dictionary<string, string>()
            {
                { "left", "LEFT ( character_expression , integer_expression )" },
                { "right", "RIGHT ( character_expression , integer_expression )" },
                { "upper", "UPPER ( character_expression )" },
                { "substring","SUBSTRING ( expression ,start , length ) "},
                { "dateadd","DATEADD (datepart , number , date )"},
                { "datediff", "DATEDIFF ( datepart , startdate , enddate )  "},
                { "getdate", "GETDATE()"},
                { "cast", "CAST ( expression AS data_type [ ( length ) ] )"},
                { "convert","CONVERT ( data_type [ ( length ) ] , expression [ , style ] ) "},
                { "case","CASE WHEN x=y THEN 'something' WHEN x=z THEN 'something2' ELSE 'something3' END"}
            };
        }

        public override bool IsTimeout(Exception exception)
        {
            var sqlE = exception as SqlException;

            if (sqlE != null)
            {
                if (sqlE.Number == -2 || sqlE.Number == 11 || sqlE.Number == 1205)
                    return true;

                //yup, I've seen this behaviour from Sql Server.  ExceptionMessage of " " and .Number of 
                if (string.IsNullOrWhiteSpace(sqlE.Message) && sqlE.Number == 3617)
                    return true;
            }

            return base.IsTimeout(exception);
        }

        public override string HowDoWeAchieveMd5(string selectSql)
        {
            return "CONVERT(NVARCHAR(32),HASHBYTES('MD5'," + selectSql + "),2)";
        }

        public override string EnsureWrappedImpl(string databaseOrTableName)
        {
            return "[" + GetRuntimeName(databaseOrTableName) + "]";
        }

        public override string EnsureFullyQualified(string databaseName, string schema, string tableName)
        {
            //if there is no schema address it as db..table (which is the same as db.dbo.table in Microsoft SQL Server)
            if(string.IsNullOrWhiteSpace(schema))
                return "["+ GetRuntimeName(databaseName) +"]"+ DatabaseTableSeparator + DatabaseTableSeparator + "["+GetRuntimeName(tableName)+"]";


            //there is a schema so add it in
            return "[" + GetRuntimeName(databaseName) + "]" + DatabaseTableSeparator + schema + DatabaseTableSeparator + "[" + GetRuntimeName(tableName) + "]";
        }

        public override string EnsureFullyQualified(string databaseName, string schema, string tableName, string columnName, bool isTableValuedFunction = false)
        {
            if (isTableValuedFunction)
                return GetRuntimeName(tableName) + ".[" + GetRuntimeName(columnName)+"]";//table valued functions do not support database name being in the column level selection list area of sql queries

            return EnsureFullyQualified(databaseName,schema,tableName) + ".[" + GetRuntimeName(columnName)+"]";
        }
    }
}