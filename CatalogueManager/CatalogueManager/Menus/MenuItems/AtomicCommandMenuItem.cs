using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class AtomicCommandMenuItem : ToolStripMenuItem
    {
        private readonly IAtomicCommand _command;
        private readonly IActivateItems _activator;

        public AtomicCommandMenuItem(IAtomicCommand command,IActivateItems activator)
        {
            _command = command;
            _activator = activator;

            Text = command.GetCommandName();
            Tag = command;
            Image = command.GetImage(activator.CoreIconProvider);
            
            //disable if impossible command
            Enabled = !command.IsImpossible;

            if (command.IsImpossible)
                ToolTipText = command.ReasonCommandImpossible;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            try
            {
                _command.Execute();
            }
            catch (Exception ex)
            {
                var sqlException = ex.GetExceptionIfExists<SqlException>();

                if (sqlException != null)
                {
                    Regex fk = new Regex("FK_([A-Za-z_]*)");
                    var match = fk.Match(sqlException.Message);

                    if (match.Success)
                    {
                        var helpDict = _activator.RepositoryLocator.CatalogueRepository.HelpText;

                        if (helpDict != null && helpDict.ContainsKey(match.Value))
                        {
                            ExceptionViewer.Show("Command blocked by:" + match.Value + Environment.NewLine + "Purpose:" + helpDict[match.Value], ex);
                            return;
                        }
                    }
                }

                ExceptionViewer.Show("Failed to execute command '" + _command.GetCommandName() + "' (Type was '" + _command.GetType().Name + "')", ex);
            }
        }
    }
}