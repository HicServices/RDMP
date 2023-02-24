// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.UI.Tutorials;

namespace ResearchDataManagementPlatform.Menus.MenuItems;

/// <summary>
/// Clears all user progress on Tutorials
/// </summary>
[System.ComponentModel.DesignerCategory("")]
public class ResetTutorialsMenuItem : ToolStripMenuItem
{
    private readonly TutorialTracker _tracker;

    public ResetTutorialsMenuItem(ToolStripMenuItem parent, TutorialTracker tracker)
    {
        _tracker = tracker;
        Text = "Reset Tutorials";
        parent.DropDownOpening += parent_DropDownOpening;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        _tracker.ClearCompleted();
    }

    private void parent_DropDownOpening(object sender, EventArgs e)
    {
        Enabled = _tracker.IsClearable();
    }


}