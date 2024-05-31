// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.SimpleDialogs.NavigateTo;

/// <summary>
/// Allows you to search through and run any command (<see cref="IAtomicCommand"/>) in RDMP and lets you pick which object(s) to apply it to.
/// </summary>
public partial class RunUI : RDMPForm
{
    private readonly Dictionary<string, Type> _commandsDictionary;

    private readonly CommandInvoker _commandCaller;

    public RunUI(IActivateItems activator) : base(activator)
    {
        InitializeComponent();

        _commandsDictionary = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);

        _commandCaller = new CommandInvoker(activator);
        _commandCaller.CommandImpossible += (s, e) => MessageBox.Show(e.Command.ReasonCommandImpossible);
        _commandCaller.CommandCompleted += (s, e) => Close();

        var commands = _commandCaller.GetSupportedCommands();

        foreach (var c in commands)
        {
            var name = BasicCommandExecution.GetCommandName(c.Name);

            _commandsDictionary.TryAdd(name, c);
        }

        comboBox1.Items.AddRange(_commandsDictionary.Keys.ToArray());
    }

    public static void OnCommandExecutionException(IAtomicCommand instance, Exception exception)
    {
        ExceptionViewer.Show(exception);
    }

    private void comboBox1_KeyUp(object sender, KeyEventArgs e)
    {
        var key = (string)comboBox1.SelectedItem;

        if (key == null)
            return;

        if (e.KeyCode != Keys.Enter || !_commandsDictionary.TryGetValue(key, out var type)) return;
        try
        {
            _commandCaller.ExecuteCommand(type, null);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation Cancelled");
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }
}