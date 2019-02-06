using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExtractableDataSetPackage:BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewExtractableDataSetPackage(IActivateItems activator) : base(activator)
        {
            if(Activator.RepositoryLocator.DataExportRepository == null)
                SetImpossible("Data export database is not setup");

            UseTripleDotSuffix = true;
        }

        public override string GetCommandHelp()
        {
            return "Creates a new grouping of dataset which are commonly extracted together e.g. 'Core datasets on offer'";
        }

        public override void Execute()
        {
            base.Execute();
            var dialog = new TypeTextOrCancelDialog("Name for package", "Name", 500);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var p = new ExtractableDataSetPackage(Activator.RepositoryLocator.DataExportRepository, dialog.ResultText);
                Publish(p);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Add);
        }
    }
}
