// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Menus.MenuItems;

/// <summary>
/// <see cref="ToolStripButton"/> depicting a single <see cref="IAtomicCommand"/>
/// </summary>
public class AtomicCommandToolStripItem : ToolStripButton
{
    private readonly IAtomicCommand _command;

    public AtomicCommandToolStripItem(IAtomicCommand command, IActivateItems activator)
    {
        _command = command;

        Image = command.GetImage(activator.CoreIconProvider)?.ImageToBitmap();
        Text = command.GetCommandName();
        Tag = command;

        //disable if impossible command
        Enabled = !command.IsImpossible;

        ToolTipText = command.IsImpossible ? command.ReasonCommandImpossible : command.GetCommandHelp();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        _command.Execute();
    }
}