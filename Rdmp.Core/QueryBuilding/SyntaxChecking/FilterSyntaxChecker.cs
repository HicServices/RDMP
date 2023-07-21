// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding.SyntaxChecking;

/// <summary>
///     Checks bracket parity of where SQL of IFilter and syntax validity of parameters which are char based
/// </summary>
public class FilterSyntaxChecker : SyntaxChecker
{
    private readonly IFilter _filter;

    /// <summary>
    ///     Prepares the checker to check the IFilter supplied
    /// </summary>
    /// <param name="filter"></param>
    public FilterSyntaxChecker(IFilter filter)
    {
        _filter = filter;
    }

    /// <summary>
    ///     Checks to see if the WhereSQL contains a closing bracket for every opening bracket (see ParityCheckCharacterPairs
    ///     for more detail) and also checks the syntax validity of each parameter if it is char based (see CheckSyntax for
    ///     more detail)
    /// </summary>
    /// <param name="notifier"></param>
    public override void Check(ICheckNotifier notifier)
    {
        try
        {
            ParityCheckCharacterPairs(new[] { '(', '[' }, new[] { ')', ']' }, _filter.WhereSQL);
        }
        catch (SyntaxErrorException exception)
        {
            throw new SyntaxErrorException($"Failed to validate the bracket parity of filter {_filter}", exception);
        }

        foreach (var parameter in _filter.GetAllParameters())
            CheckSyntax(parameter);
    }
}