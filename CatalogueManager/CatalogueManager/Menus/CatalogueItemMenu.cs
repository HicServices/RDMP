using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CatalogueItemMenu : RDMPContextMenuStrip
    {
        private readonly CatalogueItem _catalogueItem;

        public CatalogueItemMenu(RDMPContextMenuStripArgs args, CatalogueItem catalogueItem): base(args, catalogueItem)
        {
            _catalogueItem = catalogueItem;

            Items.Add("Add Issue", _activator.CoreIconProvider.GetImage(RDMPConcept.CatalogueItemIssue,OverlayKind.Add),(s,e)=> AddIssue());

            Add(new ExecuteCommandLinkCatalogueItemToColumnInfo(_activator, catalogueItem));

            //it does not yet have extractability
            Add(new ExecuteCommandMakeCatalogueItemExtractable(_activator, catalogueItem));

            Add(new ExecuteCommandImportCatalogueItemDescription(_activator,_catalogueItem),Keys.Control | Keys.I);
        }

        private void AddIssue()
        {
            var newIssue = new CatalogueItemIssue(RepositoryLocator.CatalogueRepository, _catalogueItem);
            Activate(newIssue);
            Publish(_catalogueItem);
        }
    }
}