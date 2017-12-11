using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.DataHelper
{
    /// <summary>
    /// Legacy helper functions for manipulating Sql.  Many methods are Microsoft Sql Server specific.  If possible you should use IQuerySyntaxHelper instead since that is
    /// DatabaseType specific.
    /// </summary>
    [Obsolete("Use IQuerySyntaxHelper instead")]
    public class RDMPQuerySyntaxHelper
    {
        public static readonly string ParameterSQLRegex_Pattern = "(@[A-Za-z0-9_]*)\\s?";
        public static Regex ParameterSQLRegex = new Regex(ParameterSQLRegex_Pattern);
        
        /// <summary>
        /// Looks for @something within the line of SQL and returns @something (including the @ symbol)
        /// </summary>
        /// <param name="parameterSQL"></param>
        /// <returns></returns>
        public static string GetParameterNameFromDeclarationSQL(string parameterSQL)
        {

            if (!ParameterSQLRegex.IsMatch(parameterSQL))
                    throw new Exception("ParameterSQL does not match regex pattern:" + ParameterSQLRegex);

            return ParameterSQLRegex.Match(parameterSQL).Value.Trim();
        }

        public static bool IsValidParameterName(string parameterSQL)
        {
            return ParameterSQLRegex.IsMatch(parameterSQL);
        }
        
       private const string IsScalarValuedFunctionRegex = @"\(.*\)";

        public static string GetRuntimeName(IColumn column)
        {
            if (!String.IsNullOrWhiteSpace(column.Alias))
                return SqlSyntaxHelper.GetRuntimeName(column.Alias);
            
            if (!String.IsNullOrWhiteSpace(column.SelectSQL))
                if (Regex.IsMatch(column.SelectSQL, IsScalarValuedFunctionRegex))
                    throw new SyntaxErrorException("The IExtractableColumn.SelectSQL value \"" + column.SelectSQL + "\" looks like a ScalarValuedFunction but it is missing an Alias.  Add an Alias so that it has a runtime name.");
                else
                    return SqlSyntaxHelper.GetRuntimeName(column.SelectSQL);

            if (column.ColumnInfo != null)
                return column.ColumnInfo.GetRuntimeName();

            throw new Exception("IExtractableColumn with ID=" + column.ID + " does not have an Alias, SelectSQL or ColumnInfo, cannot calculate a runtime name ");
        }

        public static void CheckSyntax(IColumn col)
        {
            string regexIsWrapped = @"^[\[`].*[\]`]$";
            char[] invalidColumnValues = new[] {',', '[', ']','`','.'};
            char[] whiteSpace = new[] {' ','\t','\n','\r'};

            char[] openingCharacters = new[] {'[','('};
            char[] closingCharacters = new[] {']',')'};

            //it has an alias
            if (!String.IsNullOrWhiteSpace(col.Alias))
                if (!Regex.IsMatch(col.Alias, regexIsWrapped)) //alias is NOT wrapped
                    if(col.Alias.Any(invalidColumnValues.Contains)) //there are invalid characters
                        throw new SyntaxErrorException("Invalid characters found in Alias \""+col.Alias+"\"");
                    else
                    if(col.Alias.Any(whiteSpace.Contains))
                        throw new SyntaxErrorException("Whitespace found in unwrapped Alias \"" + col.Alias + "\"");

            ParityCheckCharacterPairs(openingCharacters, closingCharacters,col.SelectSQL);
        }

        /// <summary>
        /// Checks to see if there is a closing bracket for every opening bracket (or any other characters that come in open/close pairs.  Throws SyntaxErrorException if there
        /// is a mismatch in the number of opening/closing of any of the character pairs passed into the method.
        /// </summary>
        /// <param name="openingCharacters">An array of opening characters which start a condition e.g. '['</param>
        /// <param name="closingCharacters">An array of closing characters which must be in the same order (semantically) and size as openingCharacters e.g. if open array element 0 is '[' then closing array element 0 must be ']' </param>
        /// <param name="sql">The string of text to check for equal numbers of opening/closing characters in</param>
        public static void ParityCheckCharacterPairs(char[] openingCharacters, char[] closingCharacters, string sql)
        {
            //it has select sql
            if (!String.IsNullOrWhiteSpace(sql))
                for (int i = 0; i < openingCharacters.Length; i++)
                    if(openingCharacters[i] == closingCharacters[i]) //if the opening and closing characters are the same character then there should be an even number of them
                    {
                        //if it is not an even number of them
                        if(sql.Count(c => c == openingCharacters[i])%2 == 1)
                            throw new SyntaxErrorException("Number of instances of character " + openingCharacters[i] + " are not even (method was informed that this character is both an opening and closing character)");       
                    }
                    else //opening and closing characters are different e.g. '('  and ')', there should be the same number of each of them
                        if (sql.Count(c => c == openingCharacters[i]) != sql.Count(c => c == closingCharacters[i]))
                            throw new SyntaxErrorException("Mismatch in the number of opening '" + openingCharacters[i] + "' and closing '" + closingCharacters[i] + "'");
        }

        public static void CheckSyntax(IFilter filter)
        {
            try
            {
                ParityCheckCharacterPairs(new []{'(','['},new []{')',']'},filter.WhereSQL);
            }
            catch (SyntaxErrorException exception)
            {
                throw new SyntaxErrorException("Failed to validate the bracket parity of filter " + filter,exception);
            }

            foreach(ISqlParameter parameter in filter.GetAllParameters())
                CheckSyntax(parameter);
       }

        public static void CheckSyntax(ISqlParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Value))
                throw new SyntaxErrorException("Parameter " + parameter.ParameterName + " does not have a value");

            bool isCharBased = parameter.ParameterSQL.ToLower().Contains("char");

            if (isCharBased && parameter.Value.Trim().StartsWith("'"))
            {
                Match matchValue = Regex.Match(parameter.Value, "'[^']*'");
                Match matchLength = Regex.Match(parameter.ParameterSQL, "\\([0-9]*\\)");

                if (matchValue.Success && matchLength.Success)
                {
                    int userSpecifiedLength = Int32.Parse(matchLength.Value.Substring(1, matchLength.Value.Length - 2));
                    int actualLength = matchValue.Value.Trim().Length - 2;
                    
                    if (actualLength > userSpecifiedLength)
                        throw new SyntaxErrorException("You created a parameter of length " + userSpecifiedLength + " ("+parameter.ParameterName+") but then put a value in it that is " + actualLength + " characters long, the parameter with the problem was:" + parameter.ParameterName);
                }
            }

            if (isCharBased && !(parameter.Value.Contains("'") || parameter.Value.Contains("@")))
                throw new SyntaxErrorException("Parameter " + parameter.ParameterName + " looks like it is character based but it's value does not contain any single quotes (or at least a reference to another variable)");

            try
            {
                ParityCheckCharacterPairs(new[] { '(', '[' ,'\''}, new[] { ')', ']','\'' }, parameter.ParameterSQL);
                ParityCheckCharacterPairs(new[] { '(', '[', '\'' }, new[] { ')', ']', '\'' }, parameter.Value);
            }
            catch (SyntaxErrorException exception)
            {
                throw new SyntaxErrorException("Failed to validate the bracket parity of parameter " + parameter, exception);
            }
        }

        public static string EnsureValueIsWrapped(string s, DatabaseType type= DatabaseType.MicrosoftSQLServer)
        {
            if (type == DatabaseType.MicrosoftSQLServer)
            {
                //remove any that might already be there and then add some on 
                string stripped = s.Trim(new char[] { '[', ']' });
                return '[' + stripped + ']';
            }
            else if (type == DatabaseType.MYSQLServer)
            {

                string stripped = s.Trim(new char[] { '[', ']' }).Trim('`');
                return '`' + stripped + '`';
            }
            else if (type == DatabaseType.Oracle)
                return s.Trim();
            else
                throw new NotSupportedException("Unknown Type:" + type);
        }

        public static string EnsureValueIsNotWrapped(string s)
        {
            if (s == null)
                return null;

            string toReturn =  s.Trim(new char[] {'[', ']', '`'});
            
            if(
                toReturn.Contains("[") ||
                toReturn.Contains("]") ||
                toReturn.Contains("'"))
                throw new Exception("Attempted to strip wrapping from " + s + " but result was " + toReturn + " which contains invalid characters like [ and ], possibly original string was a multipart identifier? e.g. [MyTable].dbo.[Bob]?");

            return toReturn;
        }


        public static string EnsureMultiPartValueIsWrapped(string s, DatabaseType type = DatabaseType.MicrosoftSQLServer)
        {

            if (type != DatabaseType.MicrosoftSQLServer)
                throw new NotSupportedException();

            if (s.Contains("."))
            {
                StringBuilder result = new StringBuilder();

                foreach (string part in s.Split('.'))
                {
                    string strippedPart = part.Trim(new char[] { '[', ']' });

                    //handle db..table 
                    if (string.IsNullOrWhiteSpace(part))
                        result.Append(".");
                    else
                        result.Append('[' + strippedPart + ']' + ".");

                }

                return result.ToString().Trim('.');
            }


            string stripped = s.Trim(new char[] { '[', ']' });

            return '[' + stripped + ']';
        }

        public static ValueType GetDataType(string dataType)
        {
            if(
                dataType.StartsWith("decimal") ||
                dataType.StartsWith("float") ||
                dataType.Equals("bigint")||
                dataType.Equals("bit")||
                dataType.Contains("decimal")||
                dataType.Equals("int")||
                dataType.Equals("money")||
                dataType.Contains("numeric")||
                dataType.Equals("smallint")||
                dataType.Equals("smallmoney")||
                dataType.Equals("smallint")||
                dataType.Equals("tinyint")||
                dataType.Equals("real")) 
                    return ValueType.Numeric;

            if(dataType.Contains("date"))
                return ValueType.DateTime;
            
            if (dataType.Contains("time"))
                return ValueType.Time;

            if(dataType.Contains("char") || dataType.Contains("text"))
                return ValueType.CharacterString;

            if (dataType.Contains("binary")||dataType.Contains("image"))
                return ValueType.Binary;

            if (dataType.Equals("cursor")||
                dataType.Contains("timestamp")||
                dataType.Contains("hierarchyid")||
                dataType.Contains("uniqueidentifier")||
                dataType.Contains("sql_variant")||
                dataType.Contains("xml")||
                dataType.Contains("table")||
                dataType.Contains("spacial"))
                return ValueType.Freaky;
            
            throw new Exception("Could not figure out the ValueType of SQL Type \"" + dataType + "\"");


        }

        public enum ValueType
        {
            Numeric,
            DateTime,
            Time,
            CharacterString,
            Binary,
            Freaky
        }

        public static string GetNullSubstituteForComparisonsWithDataType(string datatype,bool min)
        {
            //technically these can go lower (real and float) but how realistic is that espcially when SqlServer plays fast and loose with very small numbers in floats... 
            if(datatype.Equals("bigint") || datatype.Equals("real") || datatype.StartsWith("float"))
                if(min)
                    return "-9223372036854775808";
                else
                    return "9223372036854775807";

            if (datatype.Equals("int"))
                if (min)
                    return "-2147483648";
                else
                    return "2147483647";

            if (datatype.Equals("smallint"))
                if (min)
                    return "-32768";
                else
                    return "32767";

            if (datatype.Equals("tinyint"))
                if (min)
                    return "- 1.79E+308";
                else
                    return "255";

            if(datatype.Equals("bit"))
                if (min)
                    return "0";
                else
                    return "1";

            if (datatype.Contains("decimal") || datatype.Contains("numeric"))
            {
                var digits = Regex.Match(datatype, @"(\d+),?(\d+)?");
                string toReturn = "";

                if (min)
                    toReturn = "-";
                
                //ignore element zero because elment zero is always a duplicate see https://msdn.microsoft.com/en-us/library/system.text.regularexpressions.match.groups%28v=vs.110%29.aspx
                if (digits.Groups.Count == 3 && string.IsNullOrWhiteSpace(digits.Groups[2].Value))
                {
                    for (int i = 0; i < Convert.ToInt32(digits.Groups[1].Value); i++)
                        toReturn += "9";

                    return toReturn;
                }

                if (digits.Groups.Count == 3)
                {
                    int totalDigits = Convert.ToInt32(digits.Groups[1].Value);
                    int digitsAfterDecimal = Convert.ToInt32(digits.Groups[2].Value);

                    for (int i = 0; i < totalDigits+1; i++)
                        if(i == totalDigits - digitsAfterDecimal)
                            toReturn += ".";
                        else
                            toReturn += "9";

                    return toReturn;
                }
            }

            ValueType valueType = GetDataType(datatype);

            if (valueType == ValueType.CharacterString)
                if(min)
                    return "''";
                else
                    throw new NotSupportedException("Cannot think what the maxmimum character string would be, maybe use min = true instead?");

            if (valueType == ValueType.DateTime)
                if (min)
                    return "'1753-1-1'";
                else
                    throw new NotSupportedException("Cannot think what the maxmimum date would be, maybe use min = true instead?");

            if (valueType == ValueType.Time)
                if (min)
                    return "'00:00:00'";
                else
                    return "'23:59:59'";

            if (valueType == ValueType.Freaky)
                throw new NotSupportedException("Cannot predict null value substitution for freaky datatypes like " + datatype);

            if (valueType == ValueType.Binary)
                throw new NotSupportedException("Cannot predict null value substitution for binary datatypes like " + datatype);


            throw new NotSupportedException("Didn't know what minimum value type to use for " +datatype);

        }
    }
}
    