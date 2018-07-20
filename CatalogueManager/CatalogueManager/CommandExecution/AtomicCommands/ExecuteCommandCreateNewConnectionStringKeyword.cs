using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewConnectionStringKeyword : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandCreateNewConnectionStringKeyword(IActivateItems activator):base(activator)
        {
            
        }

        public override void Execute()
        {
            base.Execute();

            var keyword = new ConnectionStringKeyword(Activator.RepositoryLocator.CatalogueRepository, DatabaseType.MicrosoftSQLServer,"NewKeyword", "v");
            Publish(keyword);
            Activate(keyword);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ConnectionStringKeyword, OverlayKind.Add);
        }
    }
}