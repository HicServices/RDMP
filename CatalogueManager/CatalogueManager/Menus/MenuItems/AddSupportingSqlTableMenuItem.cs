using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class AddSupportingSqlTableMenuItem : ToolStripMenuItem
    {
        private readonly IActivateItems _activator;
        private readonly Catalogue _catalogue;

        public AddSupportingSqlTableMenuItem(IActivateItems activator, Catalogue catalogue)
            : base("Add New Supporting SQL")
        {
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.SupportingSQLTable, OverlayKind.Add);
            _activator = activator;
            _catalogue = catalogue;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            AddSupportingSqlTable(_catalogue);
        }

        private void AddSupportingSqlTable(Catalogue catalogue)
        {
            var newSqlTable = new SupportingSQLTable((ICatalogueRepository)catalogue.Repository, catalogue, "New Supporting SQL Table " + Guid.NewGuid());
            _activator.ActivateSupportingSqlTable(this, newSqlTable);

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(catalogue));
        }
    }
}