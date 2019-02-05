using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewSupportingSqlTable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;

        public ExecuteCommandAddNewSupportingSqlTable(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
        }

        public override string GetCommandHelp()
        {
            return "Allows you to specify some freeform SQL that helps understand / interact with a dataset.  Optionally this SQL can be run and the results provided in project extractions.";
        }

        public override void Execute()
        {
            base.Execute();

            var newSqlTable = new SupportingSQLTable((ICatalogueRepository)_catalogue.Repository, _catalogue, "New Supporting SQL Table " + Guid.NewGuid());

            Activate(newSqlTable);
            Publish(_catalogue);
        }
        
        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SupportingSQLTable, OverlayKind.Add);
        }
    }
}