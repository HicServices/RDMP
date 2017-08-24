using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class AddSupportingDocumentMenuItem : ToolStripMenuItem
    {
        private IActivateItems _activator;
        private readonly Catalogue _catalogue;

        public AddSupportingDocumentMenuItem(IActivateItems activator, Catalogue catalogue):base("Add New Supporting Document")
        {
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.SupportingDocument, OverlayKind.Add);
            _activator = activator;
            _catalogue = catalogue;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            AddSupportingDocument();
        }

        private void AddSupportingDocument()
        {
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
    }
}