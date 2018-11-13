using System.IO;
using CatalogueManager.CommandExecution.AtomicCommands;
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
            if (folder.Project.ExtractionDirectory != null)
                Add(new ExecuteCommandOpenInExplorer(_activator, new DirectoryInfo(folder.Project.ExtractionDirectory)));
            
            Add(new ExecuteCommandSetProjectExtractionDirectory(_activator, folder.Project));
        }
    }
}