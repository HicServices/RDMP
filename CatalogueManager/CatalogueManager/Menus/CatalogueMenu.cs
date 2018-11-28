using System;
using System.Windows.Forms;
using System.Xml;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
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

            Items.Add(new ToolStripSeparator());

            var addItem = new ToolStripMenuItem("Add", null);
                Add(new ExecuteCommandAddNewSupportingSqlTable(_activator, catalogue), Keys.None, addItem);
                Add(new ExecuteCommandAddNewSupportingDocument(_activator, catalogue), Keys.None, addItem);
                Add(new ExecuteCommandAddNewAggregateGraph(_activator, catalogue), Keys.None, addItem);
                Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, catalogue,null), Keys.None, addItem);
                Add(new ExecuteCommandAddNewCatalogueItem(_activator, catalogue), Keys.None, addItem);
                Items.Add(addItem);


            Items.Add(new ToolStripSeparator());
            Add(new ExecuteCommandChangeExtractability(_activator, catalogue));
            Add(new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(catalogue));
            Add(new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, catalogue));
            
            Items.Add(new ToolStripSeparator());


            var extract = new ToolStripMenuItem("Import/Export");

            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new[] {catalogue}),Keys.None,extract);
            Add(new ExecuteCommandImportCatalogueDescriptionsFromShare(_activator, catalogue),Keys.None,extract);
            Add(new ExecuteCommandExportInDublinCoreFormat(_activator, catalogue),Keys.None,extract);
            Add(new ExecuteCommandImportDublinCoreFormat(_activator, catalogue), Keys.None, extract);

            Items.Add(extract);

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
    }
}
