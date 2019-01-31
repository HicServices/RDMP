using System.Drawing;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandEditLoadMetadataDescription : BasicUICommandExecution,IAtomicCommand
    {
        private LoadMetadata _loadMetadata;

        public ExecuteCommandEditLoadMetadataDescription(IActivateItems activator, LoadMetadata loadMetadata):base(activator)
        {
            _loadMetadata = loadMetadata;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadMetadata,OverlayKind.Edit);
        }

        public override string GetCommandName()
        {
            return "Edit Description";
        }

        public override void Execute()
        {
            Activator.Activate<LoadMetadataUI, LoadMetadata>(_loadMetadata);
        }
    }
}