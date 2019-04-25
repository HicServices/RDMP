// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.CommandExecution.AtomicCommands;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.UI.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace Rdmp.UI.CommandExecution.AtomicCommands.UIFactory
{
    /// <summary>
    /// Provides access to an IAtomicCommand which takes as input a database object e.g. Edit a Catalogue command in which you must fist select which Catalogue you want to edit from a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TechnicalUI]
    public partial class AtomicCommandWithTargetUI<T> : UserControl
    {
        private readonly IAtomicCommandWithTarget _command;
        private T[] _selection;
        public AtomicCommandWithTargetUI(IIconProvider iconProvider, IAtomicCommandWithTarget command, IEnumerable<T> selection, Func<T, string> propertySelector)
        {
            _command = command;
            InitializeComponent();

            _selection = selection.ToArray();

            pbCommandIcon.Image = command.GetImage(iconProvider);
            helpIcon1.BackgroundImage = iconProvider.GetImage(RDMPConcept.Help);

            string name = command.GetCommandName();
            lblName.Text = name;
            
            helpIcon1.SetHelpText(_command.GetCommandName(),_command.GetCommandHelp());

            selectIMapsDirectlyToDatabaseTableComboBox1.SetUp(_selection.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
            selectIMapsDirectlyToDatabaseTableComboBox1.SelectedItemChanged += suggestComboBox1_SelectedIndexChanged;
            lblGo.Enabled = false;

            Enabled = _selection.Any();
        }

        private void lblGo_Click(object sender, EventArgs e)
        {
            if (_command.IsImpossible)
            {
                MessageBox.Show(_command.ReasonCommandImpossible,"Could not complete command");
                return;
            }
            
            _command.Execute();
        }

        private void lblGo_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void lblGo_MouseLeave(object sender, EventArgs e)
        {

            Cursor = DefaultCursor;
        }

        private void suggestComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = (DatabaseEntity) selectIMapsDirectlyToDatabaseTableComboBox1.SelectedItem;

            if(t != null)
                _command.SetTarget(t);

            lblGo.Enabled = t != null;
        }

    }
}
