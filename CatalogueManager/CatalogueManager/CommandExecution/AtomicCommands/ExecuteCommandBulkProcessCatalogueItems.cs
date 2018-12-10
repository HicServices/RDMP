using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandBulkProcessCatalogueItems : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;

        public ExecuteCommandBulkProcessCatalogueItems(IActivateItems activator, Catalogue catalogue):base(activator)
        {
            _catalogue = catalogue;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Edit);
        }

        public override void Execute()
        {
            Activator.Activate<BulkProcessCatalogueItems, Catalogue>(_catalogue);
        }
    }
}