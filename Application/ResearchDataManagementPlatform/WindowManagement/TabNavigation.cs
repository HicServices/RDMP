// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement;

/// <summary>
/// Records the fact that the user visited a specific <see cref="DockContent"/> (Tab)
/// </summary>
public class TabNavigation : INavigation
{
    public DockContent Tab { get; }

    public bool IsAlive => Tab.ParentForm != null;

    public TabNavigation(DockContent tab)
    {
        Tab = tab;
    }

    public void Activate(ActivateItems activateItems)
    {
        Tab.Activate();
    }

    public void Close()
    {
        Tab.Close();
    }

    public override string ToString() => Tab.TabText;

    public override bool Equals(object obj) => obj.GetType() == typeof(TabNavigation) && EqualityComparer<DockContent>.Default.Equals(Tab, ((TabNavigation)obj).Tab);

    public override int GetHashCode()
    {
        unchecked
        {
            return -2031380020 + EqualityComparer<DockContent>.Default.GetHashCode(Tab);
        }
    }
}