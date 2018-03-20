using System;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.ChecksUI;
using CatalogueLibrary.Data;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CatalogueMenu:RDMPContextMenuStrip
    {
        public CatalogueMenu(RDMPContextMenuStripArgs args, CatalogueFolder folder): base(args, folder)
        {
            AddImportOptions();
        }

        public CatalogueMenu(RDMPContextMenuStripArgs args, Catalogue catalogue):base(args,catalogue)
        {
            //create right click context menu
            Add(new ExecuteCommandViewCatalogueExtractionSql(_activator).SetTarget(catalogue));

            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks(catalogue));

            Items.Add(new ToolStripSeparator());

            var addItem = new ToolStripMenuItem("Add", null);
            Add(new ExecuteCommandAddNewSupportingSqlTable(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewSupportingDocument(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewAggregateGraph(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, catalogue,null), Keys.None, addItem);
            Add(new ExecuteCommandAddNewCatalogueItem(_activator, catalogue), Keys.None, addItem);

            Items.Add(addItem);

            Items.Add(new ToolStripSeparator());
            
            Add(new ExecuteCommandCreateANOVersion(_activator, catalogue));

            /////////////////////////////////////////////////////////////Catalogue Items sub menu///////////////////////////
            Items.Add(new ToolStripSeparator());

            AddImportOptions();
        }

        private void AddImportOptions()
        {
            //Things that are always visible regardless
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator));
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, true));
            Add(new ExecuteCommandCreateNewEmptyCatalogue(_activator));
        }

        
        public void PopupChecks(ICheckable checkable)
        {
            var popupChecksUI = new PopupChecksUI("Checking " + checkable, false);
            popupChecksUI.StartChecking(checkable);
        }
    }
}
