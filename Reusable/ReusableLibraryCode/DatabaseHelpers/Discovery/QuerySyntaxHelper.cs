using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class QuerySyntaxHelper:IQuerySyntaxHelper
    {
       public abstract string DatabaseTableSeparator { get; }

        public ITypeTranslater TypeTranslater { get; private set; }
        public IAggregateHelper AggregateHelper { get; private set; }


        protected virtual Regex GetAliasRegex()
        {
            return new Regex(@"\s+as\s+(\w+)$", RegexOptions.IgnoreCase);
        }

        protected virtual string GetAliasConst()
        {
            return " AS ";
        }

        public string AliasPrefix
        {
            get
            {
                return ValidateAlias(GetAliasConst());
            }
        }

        protected QuerySyntaxHelper(ITypeTranslater translater, IAggregateHelper aggregateHelper)
        {
            TypeTranslater = translater;
            AggregateHelper = aggregateHelper;
        }

        public virtual string GetRuntimeName(string s)
        {
            var match = GetAliasRegex().Match(s.Trim());//if it is an aliased entity e.g. AS fish then we should return fish (this is the case for table valued functions and not much else)
            if (match.Success)
                return match.Groups[1].Value;

            return s.Substring(s.LastIndexOf(".") + 1).Trim('[', ']', '`');
        }

        public virtual string EnsureFullyQualified(string databaseName, string schema, string tableName)
        {

            string toReturn = GetRuntimeName(databaseName);
            
            if (!string.IsNullOrWhiteSpace(schema))
                toReturn += "." + schema;
            
            toReturn += "." + GetRuntimeName(tableName);

            return toReturn;
        }

        public virtual string EnsureFullyQualified(string databaseName, string schema, string tableName, string columnName, bool isTableValuedFunction=false)
        {
            if (isTableValuedFunction)
                return GetRuntimeName(tableName) + "." + GetRuntimeName(columnName);//table valued functions do not support database name being in the column level selection list area of sql queries

            return EnsureFullyQualified(databaseName,schema,tableName)+ "." +GetRuntimeName(columnName);
        }

        public virtual string Escape(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            return sql.Replace("'", "''");
        }
        public abstract TopXResponse HowDoWeAchieveTopX(int x);
        public abstract string GetParameterDeclaration(string proposedNewParameterName, DatabaseTypeRequest request);


        private string ValidateAlias(string getAlias)
        {
            if (!(getAlias.StartsWith(" ") && getAlias.EndsWith(" ")))
                throw new NotSupportedException("GetAliasConst method on Type " + this.GetType().Name +" returned a value that was not bounded by whitespace ' '.  GetAliasConst must start and end with a space e.g. ' AS '");

            var testString = "col " + getAlias + " bob";
            var match = GetAliasRegex().Match(testString);
            if(!match.Success)
                throw new NotSupportedException("GetAliasConst method on Type " + this.GetType().Name + " returned a value that was not matched by  GetAliasRegex()");

            if (match.Groups.Count < 2 || !match.Groups[1].Value.Equals("bob"))
                throw new NotSupportedException("GetAliasRegex method on Type " + this.GetType().Name + @" must return a regex with a capture group that matches the runtime name of the line e.g. \s+AS\s+(\w+)$");
                

            return getAlias;
        }

        public virtual bool SplitLineIntoSelectSQLAndAlias(string lineToSplit, out string selectSQL, out string alias)
        {
            StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase;

            if (lineToSplit.IndexOf(AliasPrefix, comparisonType) == -1)
            {
                //doesn't have the alias prefix
                selectSQL = lineToSplit.TrimEnd(',', ' ', '\n', '\r');
                alias = null;
                return false;
            }

            if (lineToSplit.IndexOf(AliasPrefix, comparisonType) != lineToSplit.LastIndexOf(AliasPrefix, comparisonType))
                throw new SyntaxErrorException("Found two instances of the alias prefix:\"" + AliasPrefix + "\"");

            int splitPoint = lineToSplit.IndexOf(AliasPrefix, comparisonType);

            selectSQL = lineToSplit.Substring(0, splitPoint);

            //could end with the alias and then be blank if the user is busy typing it all in a oner
            if (splitPoint + AliasPrefix.Length < lineToSplit.Length)
            {
                alias = lineToSplit.Substring(splitPoint + AliasPrefix.Length).TrimEnd(',',' ','\n','\r');

                return true;
            }

            alias = null;
            return false;
        }


        /// <summary>
        /// Takes a line line " count(*) " and returns "count" and "*"
        /// Also handles LTRIM(RTRIM(FishFishFish)) by returning "LTRIM" and  "RTRIM(FishFishFish)"
        /// </summary>
        /// <param name="lineToSplit"></param>
        /// <param name="method"></param>
        /// <param name="contents"></param>
        public void SplitLineIntoOuterMostMethodAndContents(string lineToSplit, out string method, out string contents)
        {
            if(string.IsNullOrWhiteSpace(lineToSplit))
                throw new ArgumentException("line must not be blank",lineToSplit);

            if(lineToSplit.Count(c=>c.Equals('(')) != lineToSplit.Count(c=>c.Equals(')')))
                throw new ArgumentException("The number of opening parentheses must match the number of closing parentheses", "lineToSplit");

            int firstBracket = lineToSplit.IndexOf('(');

            if(firstBracket == -1)
                throw new ArgumentException("Line must contain at least one pair of parentheses","lineToSplit");

            method = lineToSplit.Substring(0, firstBracket).Trim();
            
            int lastBracket = lineToSplit.LastIndexOf(')');

            int length = lastBracket - (firstBracket + 1);

            if (length == 0)
                contents = ""; //it's something like count()
            else
                contents = lineToSplit.Substring(firstBracket + 1, length).Trim();
        }
    }


}
