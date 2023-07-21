// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Menus.MenuItems;

[System.ComponentModel.DesignerCategory("")]
public abstract class RDMPToolStripMenuItem : ToolStripMenuItem
{
    protected AtomicCommandUIFactory AtomicCommandUIFactory;
    protected IActivateItems _activator;

    protected RDMPToolStripMenuItem(IActivateItems activator,string text):base(text)
    {
        _activator = activator;
        AtomicCommandUIFactory = new AtomicCommandUIFactory(activator);
    }
        
    protected void Activate(DatabaseEntity o)
    {
        var cmd = new ExecuteCommandActivate(_activator, o);
        cmd.Execute();
    }

    protected void Publish(DatabaseEntity o)
    {
        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(o));
    }

    /// <summary>
    /// Adds the given command to the drop down item list of this tool strip menu item
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="shortcutKey"></param>
    /// <returns></returns>
    protected ToolStripMenuItem Add(IAtomicCommand cmd, Keys shortcutKey = Keys.None)
    {
        var mi = AtomicCommandUIFactory.CreateMenuItem(cmd);

        if (shortcutKey != Keys.None)
            mi.ShortcutKeys = shortcutKey;

        DropDownItems.Add(mi);
        return mi;
    }
}