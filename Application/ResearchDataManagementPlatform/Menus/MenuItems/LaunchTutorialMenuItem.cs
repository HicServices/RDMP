// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Tutorials;

namespace ResearchDataManagementPlatform.Menus.MenuItems;

/// <summary>
/// Launches the given Tutorial, Tutorials which the user has already been exposed to will be marked (Seen)
/// </summary>
[System.ComponentModel.DesignerCategory("")]
public class LaunchTutorialMenuItem : ToolStripMenuItem
{
    private readonly Tutorial _tutorial;
    private readonly TutorialTracker _tracker;

    public LaunchTutorialMenuItem(ToolStripMenuItem parent,IActivateItems activator, Tutorial tutorial, TutorialTracker tracker)
    {
        parent.DropDownOpening += parent_DropDownOpening;
        _tutorial = tutorial;
        _tracker = tracker;

        UpdateText();
    }

    private void parent_DropDownOpening(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        Text = _tutorial.Name;

        if (_tracker.HasSeen(_tutorial))
            Text += " (Seen)";
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
            
        _tracker.ClearCompleted(_tutorial);
        _tracker.LaunchTutorial(_tutorial);
    }
}