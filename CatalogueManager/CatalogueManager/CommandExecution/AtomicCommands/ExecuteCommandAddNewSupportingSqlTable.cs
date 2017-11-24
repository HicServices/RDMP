using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewSupportingSqlTable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly Catalogue _catalogue;

        public ExecuteCommandAddNewSupportingSqlTable(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _activator = activator;
            _catalogue = catalogue;
        }

        public override void Execute()
        {
            base.Execute();

            var newSqlTable = new SupportingSQLTable((ICatalogueRepository)_catalogue.Repository, _catalogue, "New Supporting SQL Table " + Guid.NewGuid());
            _activator.ActivateSupportingSqlTable(this, newSqlTable);

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_catalogue));
        }


        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SupportingSQLTable, OverlayKind.Add);
        }
    }
}