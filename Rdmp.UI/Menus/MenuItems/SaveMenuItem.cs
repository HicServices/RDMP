// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.Menus.MenuItems;

/// <summary>
/// Provides a shortcut to save the currently selected ISaveableUI.  This class requires that you track and regularly update the Saveable property to match
/// the currently selected saveable tab
/// </summary>
[DesignerCategory("")]
public class SaveMenuItem : ToolStripMenuItem
{
    private ISaveableUI _saveable;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISaveableUI Saveable
    {
        get => _saveable;
        set
        {
            _saveable = value;
            Enabled = value != null;
        }
    }

    public SaveMenuItem() : base("Save")
    {
        ShortcutKeys = Keys.Control | Keys.S;
    }

    public SaveMenuItem(ISaveableUI saveable) : this()
    {
        Saveable = saveable;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Saveable.GetObjectSaverButton().Save();
    }
}