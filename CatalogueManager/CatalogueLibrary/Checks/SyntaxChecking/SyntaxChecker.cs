using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks.SyntaxChecking
{
    /// <summary>
    /// Base class for all Checkers which check the Sql Syntax of an object e.g. an IFilter's WhereSql
    /// </summary>
    public abstract class SyntaxChecker : ICheckable
    {
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

        /// <summary>
        /// Checks to ensure char based parameters contains a value, are not longer than the expected length and contain either single quotes or an @ symbol before performing bracket parity checks
        /// </summary>
        /// <param name="parameter"></param>
        public void CheckSyntax(ISqlParameter parameter)
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

        /// <summary>
        /// Override in child classes to check the currently configured Sql of the object
        /// </summary>
        /// <param name="notifier"></param>
        public abstract void Check(ICheckNotifier notifier);
    }
}
