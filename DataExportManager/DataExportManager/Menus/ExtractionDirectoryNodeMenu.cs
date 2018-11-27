using System.IO;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using DataExportLibrary.Providers.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractionDirectoryNodeMenu : RDMPContextMenuStrip
    {
        public ExtractionDirectoryNodeMenu(RDMPContextMenuStripArgs args, ExtractionDirectoryNode folder): base(args, folder)
        {
            ReBrandActivateAs("Open In Explorer",RDMPConcept.CatalogueFolder);
            
            Add(new ExecuteCommandSetProjectExtractionDirectory(_activator, folder.Project));
        }
    }
}