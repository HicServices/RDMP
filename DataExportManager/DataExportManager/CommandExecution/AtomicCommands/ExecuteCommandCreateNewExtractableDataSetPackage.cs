using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExtractableDataSetPackage:BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewExtractableDataSetPackage(IActivateItems activator) : base(activator)
        {
            if(Activator.RepositoryLocator.DataExportRepository == null)
                SetImpossible("Data export database is not setup");
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
