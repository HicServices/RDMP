// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Tutorials;

namespace ResearchDataManagementPlatform.Menus.MenuItems;

/// <summary>
/// Disables displaying Tutorials in RDMP
/// </summary>
[System.ComponentModel.DesignerCategory("")]
public sealed class DisableTutorialsMenuItem : ToolStripMenuItem
{
    private readonly TutorialTracker _tracker;

    public DisableTutorialsMenuItem(ToolStripMenuItem parent, TutorialTracker tracker)
    {
        parent.DropDownOpened += Parent_DropDownOpened;
        _tracker = tracker;
        Text = "Disable Tutorials";
    }

    private void Parent_DropDownOpened(object sender, EventArgs e)
    {
        Checked = UserSettings.DisableTutorials;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        _tracker.DisableAllTutorials();
    }
}