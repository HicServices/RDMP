using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandReOrderColumns:BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;

        public ExecuteCommandReOrderColumns(IActivateItems activator, Catalogue catalogue): base(activator)
        {
            _catalogue = catalogue;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ReOrder);
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<ReOrderCatalogueItems, Catalogue>(_catalogue);
        }
    }
}