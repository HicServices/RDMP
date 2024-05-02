// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Checks;

/// <summary>
///     Lists all plugin/dll load exceptions generated during Startup (when MEF is processed).  Also checks that all Types
///     declared as ICheckable
///     can be constructed
/// </summary>
public class BadAssembliesChecker : ICheckable
{
    /// <summary>
    ///     Prepares to check the currently loaded assemblies defined in the MEF (Call CatalogueRepository.MEF to get the MEF),
    ///     call Check to start the checking process
    /// </summary>
    public BadAssembliesChecker()
    {
    }

    /// <summary>
    ///     Lists assembly load errors and attempts to construct instances of all Types declared as Exports (which are
    ///     ICheckable)
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        foreach (var badAssembly in MEF.ListBadAssemblies())
            notifier.OnCheckPerformed(new CheckEventArgs($"Could not load assembly {badAssembly.Key}", CheckResult.Fail,
                badAssembly.Value));

        foreach (var t in MEF.GetAllTypes())
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Found Type {t}", CheckResult.Success));

            if (typeof(ICheckable).IsAssignableFrom(t))
                try
                {
                    MEF.CreateA<ICheckable>(t.FullName);
                }
                catch (Exception ex)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Class {t.FullName} implements ICheckable but could not be created as an ICheckable.",
                        CheckResult.Warning, ex));
                }
        }
    }
}