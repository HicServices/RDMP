using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandChooseHICProjectDirectory : BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly LoadMetadata _loadMetadata;

        public ExecuteCommandChooseHICProjectDirectory(IActivateItems activator, LoadMetadata loadMetadata)
        {
            _activator = activator;
            _loadMetadata = loadMetadata;
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new ChooseHICProjectDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _loadMetadata.LocationOfFlatFiles = dialog.Result.RootPath.FullName;
                _loadMetadata.SaveToDatabase();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_loadMetadata));
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.HICProjectDirectoryNode);
        }
    }
}