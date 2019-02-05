using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using System.Windows.Forms;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CatalogueMenu:RDMPContextMenuStrip
    {
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


            var extractability = new ToolStripMenuItem("Extractability");
            Add(new ExecuteCommandChangeExtractability(_activator, catalogue),Keys.None,extractability);
            Add(new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(catalogue),Keys.None,extractability);
            Add(new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, catalogue),Keys.None,extractability);
            Items.Add(extractability);

            var extract = new ToolStripMenuItem("Import/Export Descriptions");
            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new[] {catalogue}),Keys.None,extract);
            Add(new ExecuteCommandImportCatalogueDescriptionsFromShare(_activator, catalogue),Keys.None,extract);
            Add(new ExecuteCommandExportInDublinCoreFormat(_activator, catalogue),Keys.None,extract);
            Add(new ExecuteCommandImportDublinCoreFormat(_activator, catalogue), Keys.None, extract);
            Items.Add(extract);


        }
    }
}
