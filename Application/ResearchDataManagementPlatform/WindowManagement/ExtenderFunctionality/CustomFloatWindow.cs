// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;
using Rdmp.UI;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.SimpleControls;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;

/// <summary>
/// Determines the window style of tabs dragged out of the main RDMPMainForm window to create new windows of that tab only.  Currently the only change is to allow the user to resize
///  and maximise new tab windows
/// </summary>
[TechnicalUI]
[System.ComponentModel.DesignerCategory("")]
public class CustomFloatWindow:FloatWindow
{
    protected internal CustomFloatWindow(DockPanel dockPanel, DockPane pane) : base(dockPanel, pane)
    {
        Initialize();
            
    }
    protected internal CustomFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds): base(dockPanel, pane, bounds)
    {
        Initialize();
    }

    private void Initialize()
    {
        FormBorderStyle = FormBorderStyle.Sizable;

        var saveToolStripMenuItem = new SaveMenuItem();
        var singleObjectControlTab = DockPanel.ActiveDocument as RDMPSingleControlTab;

        if (singleObjectControlTab == null)
        {
            saveToolStripMenuItem.Saveable = null;
            return;
        }

        var saveable = singleObjectControlTab.Control as ISaveableUI;
        saveToolStripMenuItem.Saveable = saveable;
    }
}