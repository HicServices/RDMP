// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding.SyntaxChecking;

/// <summary>
///     Checks whether an IColumn has an alias and if so whether it is wrapped and whether it contains invalid characters
///     or whitespace
/// </summary>
public class ColumnSyntaxChecker : SyntaxChecker
{
    private static readonly Regex RegexIsWrapped = new(@"^[\[`].*[\]`]$", RegexOptions.Compiled);

    private readonly IColumn _column;

    /// <summary>
    ///     Prepares the checker to check the IColumn supplied
    /// </summary>
    /// <param name="column"></param>
    public ColumnSyntaxChecker(IColumn column)
    {
        _column = column;
    }

    /// <summary>
    ///     Checks to see if there is an alias and if there is whether it is wrapped. If it is not wrapped and there are
    ///     invalid characters or whitespace in the alias this causes a SyntaxErrorException to be thrown.
    /// </summary>
    /// <param name="notifier"></param>
    public override void Check(ICheckNotifier notifier)
    {
        var invalidColumnValues = new[] { ',', '[', ']', '`', '.' };
        var whiteSpace = new[] { ' ', '\t', '\n', '\r' };

        var openingCharacters = new[] { '[', '(' };
        var closingCharacters = new[] { ']', ')' };

        //it has an alias
        if (!string.IsNullOrWhiteSpace(_column.Alias) && !RegexIsWrapped.IsMatch(_column.Alias))
            //alias is NOT wrapped
            if (_column.Alias.Any(invalidColumnValues.Contains)) //there are invalid characters
                throw new SyntaxErrorException($"Invalid characters found in Alias \"{_column.Alias}\"");
            else if (_column.Alias.Any(whiteSpace.Contains))
                throw new SyntaxErrorException($"Whitespace found in unwrapped Alias \"{_column.Alias}\"");

        ParityCheckCharacterPairs(openingCharacters, closingCharacters, _column.SelectSQL);
    }
}