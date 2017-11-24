using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewCatalogueItem : BasicUICommandExecution,IAtomicCommand
    {
        private IActivateItems _activator;
        private Catalogue _catalogue;

        public ExecuteCommandAddNewCatalogueItem(IActivateItems activator, Catalogue catalogue):base(activator)
        {
            _activator = activator;
            _catalogue = catalogue;
        }

        public override void Execute()
        {
            base.Execute();
        
            MessageBox.Show("Select which column the new CatalogueItem will describe/extract", "Choose underlying Column");

            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.CoreChildProvider.AllColumnInfos, true, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var colInfo = dialog.Selected as ColumnInfo;
                
                var ci = new CatalogueItem(_activator.RepositoryLocator.CatalogueRepository, _catalogue, "New CatalogueItem " + Guid.NewGuid());

                if (colInfo != null)
                {
                    var textTyper = new TypeTextOrCancelDialog("Name", "Type a name for the new CatalogueItem", 500, colInfo.GetRuntimeName());
                    if (textTyper.ShowDialog() == DialogResult.OK)
                    {

                        ci.Name = textTyper.ResultText;
                        ci.SaveToDatabase();
                    }

                    ci.SetColumnInfo(colInfo);
                }

                Publish(_catalogue);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Add);
        }
    }
}