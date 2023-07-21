// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     Implementation of <see cref="ICheckNotifier" /> which prompts Yes/No for fixes and reports errors through modal
///     call <see cref="IBasicActivateItems.Show(string)" />
/// </summary>
public class FromActivateItemsToCheckNotifier : ICheckNotifier
{
    private readonly IBasicActivateItems basicActivator;

    public FromActivateItemsToCheckNotifier(IBasicActivateItems basicActivator)
    {
        this.basicActivator = basicActivator;
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        if (args.ProposedFix != null)
            return basicActivator.YesNo(args.ProposedFix, "Apply fix?");

        if (args.Result >= CheckResult.Fail)
        {
            if (args.Ex == null)
                basicActivator.Show(args.Message);
            else
                basicActivator.ShowException(args.Message, args.Ex);
        }

        return false;
    }
}