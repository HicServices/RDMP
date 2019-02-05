using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class CatalogueFolderMenu : RDMPContextMenuStrip
    {
        public CatalogueFolderMenu(RDMPContextMenuStripArgs args, CatalogueFolder folder) : base(args, folder)
        {
            //Things that are always visible regardless
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator));
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, true));
            Add(new ExecuteCommandCreateNewEmptyCatalogue(_activator));
        }
    }
}