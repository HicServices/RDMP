using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class AddCatalogueItemMenuItem : ToolStripMenuItem
    {
        private IActivateItems _activator;
        private Catalogue _catalogue;

        public AddCatalogueItemMenuItem(IActivateItems activator, Catalogue catalogue):base("Add New Catalogue Item")
        {
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Add);
            _activator = activator;
            _catalogue = catalogue;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
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

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_catalogue));
            }
        }
    }
}