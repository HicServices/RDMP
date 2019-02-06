// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands.UIFactory
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
            
            suggestComboBox1.DataSource = selection;
            suggestComboBox1.PropertySelector = sel => sel.Cast<T>().Select(propertySelector);
            suggestComboBox1.SelectedIndex = -1;

            
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
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                lblGo.Enabled = comboBox.SelectedIndex != -1;
                _command.SetTarget((DatabaseEntity) comboBox.SelectedItem);
            }
        }

        private void lPick_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_selection.Cast<IMapsDirectlyToDatabaseTable>(), false, false);
            if (dialog.ShowDialog() == DialogResult.OK)
                suggestComboBox1.SelectedItem = dialog.Selected;
        }
    }
}
