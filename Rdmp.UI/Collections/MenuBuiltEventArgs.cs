// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;

namespace Rdmp.UI.Collections;

/// <summary>
/// Event args for the when a context menu is finished building
/// </summary>
public class MenuBuiltEventArgs : EventArgs
{
    /// <summary>
    /// The right click context menu that has just been built
    /// </summary>
    public ContextMenuStrip Menu { get; }

    /// <summary>
    /// The object for which the <see cref="Menu"/> was built
    /// </summary>
    public object Obj { get; }

    public MenuBuiltEventArgs(ContextMenuStrip menu, object obj)
    {
        Menu = menu;
        Obj = obj;
    }
}