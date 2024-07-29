// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

namespace ResearchDataManagementPlatform.WindowManagement.TabPageContextMenus;

/// <summary>
/// Right click menu for the top tab section of a docked tab in RDMP main application.
/// </summary>
[System.ComponentModel.DesignerCategory("")]
public class RDMPSingleControlTabMenu : ContextMenuStrip
{
    public RDMPSingleControlTabMenu(IActivateItems activator, RDMPSingleControlTab tab, WindowManager windowManager)
    {
        var tab1 = tab;
        Items.Add("Close Tab", null, (s, e) => tab.Close());
        Items.Add("Close All Tabs", null, (s, e) => windowManager.CloseAllWindows(tab));
        Items.Add("Close All But This", null, (s, e) => windowManager.CloseAllButThis(tab));

        Items.Add("Show", null, (s, e) => tab.HandleUserRequestingEmphasis(activator));

        if (tab is PersistableSingleDatabaseObjectDockContent single)
        {
            var uiFactory = new AtomicCommandUIFactory(activator);
            var builder = new GoToCommandFactory(activator);

            var gotoMenu = new ToolStripMenuItem(AtomicCommandFactory.GoTo) { Enabled = false };
            Items.Add(gotoMenu);
            foreach (var cmd in builder.GetCommands(single.DatabaseObject).OfType<ExecuteCommandShow>())
            {
                gotoMenu.DropDownItems.Add(uiFactory.CreateMenuItem(cmd));
                gotoMenu.Enabled = true;
            }

            RDMPContextMenuStrip.RegisterFetchGoToObjecstCallback(gotoMenu);
        }

        Items.Add("Refresh", FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            (s, e) => tab1.HandleUserRequestingTabRefresh(activator));

        var help = new ToolStripMenuItem("Help", FamFamFamIcons.help.ImageToBitmap(),
            (s, e) => tab1.ShowHelp(activator))
        {
            ShortcutKeys = Keys.F1
        };
        Items.Add(help);
    }
}