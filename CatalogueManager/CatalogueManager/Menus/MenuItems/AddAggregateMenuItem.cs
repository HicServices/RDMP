using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AddAggregateMenuItem : ToolStripMenuItem
    {
        private readonly IActivateItems _activator;
        private readonly Catalogue _catalogue;

        public AddAggregateMenuItem(IActivateItems activator, Catalogue catalogue):base("Add New Aggregate Graph")
        {
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add);
            _activator = activator;
            _catalogue = catalogue;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            AddNewAggregate();
        }

        private void AddNewAggregate()
        {
            var newAggregate = new AggregateConfiguration(_activator.RepositoryLocator.CatalogueRepository,_catalogue,"New Aggregate " + Guid.NewGuid());
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_catalogue));
            _activator.ActivateAggregate(this,newAggregate);
        }
    }
}