// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core;

/// <summary>
///     Interface for declaring plugins which interact with the RDMP user interface.  Supports injecting custom objects
///     into RDMPCollectionUI trees and inject new
///     menu items under existing objects e.g. add a new option to the Catalogue right click menu.  See the abstract base
///     for how to do this easily.
/// </summary>
public interface IPluginUserInterface : IChildProvider, IIconProvider
{
    /// <summary>
    ///     Return a list of new menu items that should appear under the given treeObject (that was right clicked in a
    ///     RDMPCollectionUI)
    /// </summary>
    /// <param name="treeObject"></param>
    /// <returns></returns>
    IEnumerable<IAtomicCommand> GetAdditionalRightClickMenuItems(object treeObject);

    /// <summary>
    ///     Implement to provide a custom user interface that should be shown when a given object
    ///     <paramref name="o" /> is activated.  Return false if you do not want to respond to the object
    ///     or its Type.
    /// </summary>
    /// <param name="o">The object being activated</param>
    /// <returns></returns>
    bool CustomActivate(IMapsDirectlyToDatabaseTable o);
}