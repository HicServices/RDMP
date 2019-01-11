using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using FAnsi;
using Fansi.Implementations.MicrosoftSQL;
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
        static MicrosoftQuerySyntaxHelper _syntaxHelper = new MicrosoftQuerySyntaxHelper();

        private const string ParameterSQLRegexPattern = "(@[A-Za-z0-9_]*)\\s?";

        /// <summary>
        /// Regex pattern used for validating Sql Parameter names e.g. @MyParam
        /// </summary>
        public static Regex ParameterSQLRegex = new Regex(ParameterSQLRegexPattern);

        private const string IsScalarValuedFunctionRegex = @"\(.*\)";

        public static string GetRuntimeName(IColumn column)
        {
            if (!String.IsNullOrWhiteSpace(column.Alias))
                return _syntaxHelper.GetRuntimeName(column.Alias);

            if (!String.IsNullOrWhiteSpace(column.SelectSQL))
                if (Regex.IsMatch(column.SelectSQL, IsScalarValuedFunctionRegex))
                    throw new SyntaxErrorException("The IExtractableColumn.SelectSQL value \"" + column.SelectSQL + "\" looks like a ScalarValuedFunction but it is missing an Alias.  Add an Alias so that it has a runtime name.");
                else
                    return _syntaxHelper.GetRuntimeName(column.SelectSQL);

            if (column.ColumnInfo != null)
                return column.ColumnInfo.GetRuntimeName();

            throw new Exception("IExtractableColumn with ID=" + column.ID + " does not have an Alias, SelectSQL or ColumnInfo, cannot calculate a runtime name ");
        }

        public static void CheckSyntax(IColumn col)
        {
            string regexIsWrapped = @"^[\[`].*[\]`]$";
            char[] invalidColumnValues = new[] { ',', '[', ']', '`', '.' };
            char[] whiteSpace = new[] { ' ', '\t', '\n', '\r' };

            char[] openingCharacters = new[] { '[', '(' };
            char[] closingCharacters = new[] { ']', ')' };

            //it has an alias
            if (!String.IsNullOrWhiteSpace(col.Alias))
                if (!Regex.IsMatch(col.Alias, regexIsWrapped)) //alias is NOT wrapped
                    if (col.Alias.Any(invalidColumnValues.Contains)) //there are invalid characters
                        throw new SyntaxErrorException("Invalid characters found in Alias \"" + col.Alias + "\"");
                    else
                        if (col.Alias.Any(whiteSpace.Contains))
                            throw new SyntaxErrorException("Whitespace found in unwrapped Alias \"" + col.Alias + "\"");

            ParityCheckCharacterPairs(openingCharacters, closingCharacters, col.SelectSQL);
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
                    if (openingCharacters[i] == closingCharacters[i]) //if the opening and closing characters are the same character then there should be an even number of them
                    {
                        //if it is not an even number of them
                        if (sql.Count(c => c == openingCharacters[i]) % 2 == 1)
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
                ParityCheckCharacterPairs(new[] { '(', '[' }, new[] { ')', ']' }, filter.WhereSQL);
            }
            catch (SyntaxErrorException exception)
            {
                throw new SyntaxErrorException("Failed to validate the bracket parity of filter " + filter, exception);
            }

            foreach (ISqlParameter parameter in filter.GetAllParameters())
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
                        throw new SyntaxErrorException("You created a parameter of length " + userSpecifiedLength + " (" + parameter.ParameterName + ") but then put a value in it that is " + actualLength + " characters long, the parameter with the problem was:" + parameter.ParameterName);
                }
            }

            if (isCharBased && !(parameter.Value.Contains("'") || parameter.Value.Contains("@")))
                throw new SyntaxErrorException("Parameter " + parameter.ParameterName + " looks like it is character based but it's value does not contain any single quotes (or at least a reference to another variable)");

            try
            {
                ParityCheckCharacterPairs(new[] { '(', '[', '\'' }, new[] { ')', ']', '\'' }, parameter.ParameterSQL);
                ParityCheckCharacterPairs(new[] { '(', '[', '\'' }, new[] { ')', ']', '\'' }, parameter.Value);
            }
            catch (SyntaxErrorException exception)
            {
                throw new SyntaxErrorException("Failed to validate the bracket parity of parameter " + parameter, exception);
            }
        }

        public static string EnsureValueIsWrapped(string s, DatabaseType type = DatabaseType.MicrosoftSQLServer)
        {
            if (type == DatabaseType.MicrosoftSQLServer)
            {
                //remove any that might already be there and then add some on 
                string stripped = s.Trim(new char[] { '[', ']' });
                return '[' + stripped + ']';
            }
            else if (type == DatabaseType.MySql)
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

            string toReturn = s.Trim(new char[] { '[', ']', '`' });

            if (
                toReturn.Contains("[") ||
                toReturn.Contains("]") ||
                toReturn.Contains("'"))
                throw new Exception("Attempted to strip wrapping from " + s + " but result was " + toReturn + " which contains invalid characters like [ and ], possibly original string was a multipart identifier? e.g. [MyTable].dbo.[Bob]?");

            return toReturn;
        }
    }
}
