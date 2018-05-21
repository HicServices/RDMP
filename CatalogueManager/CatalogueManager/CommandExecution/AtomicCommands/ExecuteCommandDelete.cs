using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDelete : BasicUICommandExecution, IAtomicCommand
    {
        private readonly IDeleteable _deletable;

        public ExecuteCommandDelete(IActivateItems activator, IDeleteable deletable) : base(activator)
        {
            _deletable = deletable;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            base.Execute();
            
            try
            {
                Activator.DeleteWithConfirmation(this, _deletable);
            }
            catch (SqlException e)
            {
                Regex fk = new Regex("FK_([A-Za-z_]*)");
                var match = fk.Match(e.Message);

                if(match.Success)
                {
                    var helpDict = Activator.RepositoryLocator.CatalogueRepository.HelpText;

                    if(helpDict != null && helpDict.ContainsKey(match.Value))
                    {
                        ExceptionViewer.Show("Cannot delete because of:" + match.Value + Environment.NewLine + "Purpose:" + helpDict[match.Value],e);
                        return;
                    }

                    throw;
                }

                throw;
            }
        }
    }
}