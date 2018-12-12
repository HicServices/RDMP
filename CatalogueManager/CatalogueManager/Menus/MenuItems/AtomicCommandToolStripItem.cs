using System;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus.MenuItems
{
    public class AtomicCommandToolStripItem : ToolStripButton
    {
        private readonly IAtomicCommand _command;

        public AtomicCommandToolStripItem(IAtomicCommand command, IActivateItems activator)
        {
            _command = command;

            Image = command.GetImage(activator.CoreIconProvider);
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
}