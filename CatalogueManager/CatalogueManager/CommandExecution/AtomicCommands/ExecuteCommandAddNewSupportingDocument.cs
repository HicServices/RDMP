using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewSupportingDocument : BasicUICommandExecution,IAtomicCommand
    {
        private IActivateItems _activator;
        private readonly Catalogue _catalogue;

        public ExecuteCommandAddNewSupportingDocument(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
             _activator = activator;
            _catalogue = catalogue;
        }

        public override void Execute()
        {
            base.Execute();

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var files = fileDialog.FileNames.Select(f => new FileInfo(f)).Where(fi => fi.Exists).ToArray();

                if (files.Any())
                {

                    var execution = new ExecuteCommandAddFilesAsSupportingDocuments(_activator,new FileCollectionCommand(files), _catalogue);
                    if (execution.IsImpossible)
                        WideMessageBox.Show(execution.ReasonCommandImpossible);
                    else
                        execution.Execute();
                }
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SupportingDocument, OverlayKind.Add);
        }
    }
}