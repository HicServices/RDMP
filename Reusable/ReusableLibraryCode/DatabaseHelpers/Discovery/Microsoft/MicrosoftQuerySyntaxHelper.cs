using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftQuerySyntaxHelper : QuerySyntaxHelper
    {
        public MicrosoftQuerySyntaxHelper() : base(new MicrosoftSQLTypeTranslater(),new MicrosoftSQLAggregateHelper())
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
                default:
                    throw new ArgumentOutOfRangeException("function");
            }
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