using System;
using System.Windows.Forms;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

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
