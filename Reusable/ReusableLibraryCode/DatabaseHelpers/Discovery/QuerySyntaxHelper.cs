using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class QuerySyntaxHelper:IQuerySyntaxHelper
    {
       public abstract string DatabaseTableSeparator { get; }

        /// <summary>
        /// Symbols (for all database types) which denote wrapped entity names e.g. [dbo].[mytable] contains qualifiers '[' and ']'
        /// </summary>
        public static char[] TableNameQualifiers = { '[',']','`'};
        
        public ITypeTranslater TypeTranslater { get; private set; }
        public IAggregateHelper AggregateHelper { get; private set; }
        public IUpdateHelper UpdateHelper { get; set; }

        protected virtual Regex GetAliasRegex()
        {
            return new Regex(@"\s+as\s+(\w+)$", RegexOptions.IgnoreCase);
        }

        protected virtual string GetAliasConst()
        {
            return " AS ";
        }

        /// <summary>
        /// Regex for identifying parameters in blocks of SQL
        /// </summary>
        /// <returns></returns>
        private static Regex _parameterNamesRegex = new Regex("(@[A-Za-z0-9_]*)\\s?", RegexOptions.IgnoreCase);
        
        public string AliasPrefix
        {
            get
            {
                return ValidateAlias(GetAliasConst());
            }
        }
        
        /// <summary>
        /// Looks for @something within the line of SQL and returns @something (including the @ symbol)
        /// </summary>
        /// <param name="parameterSQL"></param>
        /// <returns></returns>
        public static string GetParameterNameFromDeclarationSQL(string parameterSQL)
        {
            if (!_parameterNamesRegex.IsMatch(parameterSQL))
                throw new Exception("ParameterSQL does not match regex pattern:" + _parameterNamesRegex);

            return _parameterNamesRegex.Match(parameterSQL).Value.Trim();
        }

        public bool IsValidParameterName(string parameterSQL)
        {
            return _parameterNamesRegex.IsMatch(parameterSQL);
        }

        protected QuerySyntaxHelper(ITypeTranslater translater, IAggregateHelper aggregateHelper,IUpdateHelper updateHelper)
        {
            TypeTranslater = translater;
            AggregateHelper = aggregateHelper;
            UpdateHelper = updateHelper;
        }

        public virtual string GetRuntimeName(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

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

        public virtual string GetParameterDeclaration(string proposedNewParameterName, DatabaseTypeRequest request)
        {
            return GetParameterDeclaration(proposedNewParameterName, TypeTranslater.GetSQLDBTypeForCSharpType(request));
        }

        public abstract string GetParameterDeclaration(string proposedNewParameterName, string sqlType);

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

        public abstract string GetScalarFunctionSql(MandatoryScalarFunctions function);

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

        public static string MakeHeaderNameSane(string header)
        {
            if (string.IsNullOrWhiteSpace(header))
                return header;

            //replace anything that isn't a digit, letter or underscore with emptiness (except spaces - these will go but first...)
            Regex r = new Regex("[^A-Za-z0-9_ ]");

            string adjustedHeader = r.Replace(header, "");

            StringBuilder sb = new StringBuilder(adjustedHeader);

            //Camel case after spaces
            for (int i = 0; i < sb.Length; i++)
            {
                //if we are looking at a space
                if (sb[i] == ' ')
                    if (i + 1 < sb.Length) //and there is another character 
                        if (sb[i + 1] >= 'a' && sb[i + 1] <= 'z') //and that character is a lower case letter
                            sb[i + 1] = char.ToUpper(sb[i + 1]);
            }

            adjustedHeader = sb.ToString().Replace(" ", "");

            //if it starts with a digit (illegal) put an underscore before it
            if (Regex.IsMatch(adjustedHeader, "^[0-9]"))
                adjustedHeader = "_" + adjustedHeader;

            return adjustedHeader;
        }

        public string GetSensibleTableNameFromString(string potentiallyDodgyName)
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
