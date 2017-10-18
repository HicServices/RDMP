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
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

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

        public AtomicCommandWithTargetUI(IIconProvider iconProvider, IAtomicCommandWithTarget command, IEnumerable<T> selection, Func<T, string> propertySelector)
        {
            _command = command;
            InitializeComponent();

            pbCommandIcon.Image = command.GetImage(iconProvider);

            string name = command.GetCommandName();
            lblName.Text = name;
            lblName.Width = TextRenderer.MeasureText(name, lblName.Font).Width;
            suggestComboBox1.Left = lblName.Right;
            lblGo.Left = suggestComboBox1.Right;
            helpIcon1.Left = lblGo.Right;
            Width = helpIcon1.Right + 3;

            helpIcon1.SetHelpText(_command.GetCommandName(),_command.GetCommandHelp());
            
            suggestComboBox1.DataSource = selection;
            suggestComboBox1.PropertySelector = sel => sel.Cast<T>().Select(propertySelector);
            suggestComboBox1.SelectedIndex = -1;

            
            lblGo.Enabled = false;
            
        }

        private void lblGo_Click(object sender, EventArgs e)
        {
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
    }
}
