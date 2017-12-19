using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandChooseHICProjectDirectory : BasicUICommandExecution, IAtomicCommand
    {
        private readonly LoadMetadata _loadMetadata;

        public ExecuteCommandChooseHICProjectDirectory(IActivateItems activator, LoadMetadata loadMetadata) : base(activator)
        {
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
                Publish(_loadMetadata);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.HICProjectDirectoryNode);
        }
    }
}