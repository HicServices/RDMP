// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.Menus.MenuItems;

/// <summary>
///     <see cref="ToolStripMenuItem" /> depicting a single <see cref="IAtomicCommand" />
/// </summary>
[DesignerCategory("")]
public class AtomicCommandMenuItem : ToolStripMenuItem
{
    private readonly IActivateItems _activator;
    private readonly IAtomicCommand _command;

    public AtomicCommandMenuItem(IAtomicCommand command, IActivateItems activator)
    {
        _command = command;
        _activator = activator;

        Text = command.GetCommandName();
        Tag = command;
        Image = command.GetImage(activator.CoreIconProvider)?.ImageToBitmap();

        //disable if impossible command
        Enabled = !command.IsImpossible;

        ToolTipText = command.IsImpossible
            ? command.ReasonCommandImpossible
            : command.GetCommandHelp() ?? activator.GetDocumentation(command.GetType());
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        try
        {
            _command.Execute();
        }
        catch (ImpossibleCommandException ex)
        {
            WideMessageBox.Show("Command Impossible", ex.ReasonCommandImpossible);
        }
        catch (Exception ex)
        {
            var sqlException = ex.GetExceptionIfExists<SqlException>();

            if (sqlException != null)
            {
                var fk = new Regex("((FK_)|(ix_))([A-Za-z_]*)");
                var match = fk.Match(sqlException.Message);

                if (match.Success)
                {
                    var helpDict = _activator.RepositoryLocator.CatalogueRepository.CommentStore;

                    if (helpDict != null && helpDict.ContainsKey(match.Value))
                    {
                        ExceptionViewer.Show(
                            $"Rule Broken{Environment.NewLine}{helpDict[match.Value]}{Environment.NewLine}({match.Value})",
                            ex);
                        return;
                    }
                }
            }

            ExceptionViewer.Show(
                $"Failed to execute command '{_command.GetCommandName()}' (Type was '{_command.GetType().Name}')", ex);
        }
    }
}