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
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands.UIFactory
{
    /// <summary>
    /// Provides access to an IAtomicCommand including showing the command name, help text etc.  When the link label is clicked the command .Execute is run.
    /// </summary>
    [TechnicalUI]
    public partial class AtomicCommandLinkLabel : UserControl
    {
        private readonly IAtomicCommand _command;

        public AtomicCommandLinkLabel(IIconProvider iconProvider, IAtomicCommand command)
        {
            _command = command;
            InitializeComponent();

            pbCommandIcon.Image = command.GetImage(iconProvider);
            var name = command.GetCommandName();
            lblName.Text = name;
            lblName.Width = TextRenderer.MeasureText(name, lblName.Font).Width;

            helpIcon1.Left = lblName.Right;
            Width = helpIcon1.Right + 3;

            helpIcon1.SetHelpText(_command.GetCommandName(),command.GetCommandHelp());
        }
        
        private void label1_Click(object sender, EventArgs e)
        {
            _command.Execute();
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = DefaultCursor;
        }
    }
}
