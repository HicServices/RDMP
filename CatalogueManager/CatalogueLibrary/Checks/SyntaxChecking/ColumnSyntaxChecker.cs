using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks.SyntaxChecking
{
    public class ColumnSyntaxChecker : SyntaxChecker
    {
        private readonly IColumn _column;

        public  ColumnSyntaxChecker(IColumn column)
        {
            _column = column;
        }

        public override void Check(ICheckNotifier notifier)
        {
            string regexIsWrapped = @"^[\[`].*[\]`]$";
            char[] invalidColumnValues = new[] { ',', '[', ']', '`', '.' };
            char[] whiteSpace = new[] { ' ', '\t', '\n', '\r' };

            char[] openingCharacters = new[] { '[', '(' };
            char[] closingCharacters = new[] { ']', ')' };

            //it has an alias
            if (!String.IsNullOrWhiteSpace(_column.Alias))
                if (!Regex.IsMatch(_column.Alias, regexIsWrapped)) //alias is NOT wrapped
                    if (_column.Alias.Any(invalidColumnValues.Contains)) //there are invalid characters
                        throw new SyntaxErrorException("Invalid characters found in Alias \"" + _column.Alias + "\"");
                    else
                        if (_column.Alias.Any(whiteSpace.Contains))
                            throw new SyntaxErrorException("Whitespace found in unwrapped Alias \"" + _column.Alias + "\"");

            ParityCheckCharacterPairs(openingCharacters, closingCharacters, _column.SelectSQL);
        }
    }
}