// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding.SyntaxChecking;

/// <summary>
///     Checks syntax validity of ISqlParameter
/// </summary>
public class ParameterSyntaxChecker : SyntaxChecker
{
    private readonly ISqlParameter _parameter;

    /// <summary>
    ///     Prepares the checker to check the ISqlParameter supplied
    /// </summary>
    /// <param name="parameter"></param>
    public ParameterSyntaxChecker(ISqlParameter parameter)
    {
        _parameter = parameter;
    }

    /// <summary>
    ///     Checks to see if the syntax of char based parameters is valid (see CheckSyntax for more details)
    /// </summary>
    /// <param name="notifier"></param>
    public override void Check(ICheckNotifier notifier)
    {
        CheckSyntax(_parameter);
    }
}