using System.IO;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using DataExportManager.Collections.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtractionDirectoryNodeMenu : RDMPContextMenuStrip
    {
        public ExtractionDirectoryNodeMenu(IActivateItems activator, ExtractionDirectoryNode folder)
            : base(activator, null)
        {
            if (folder.Project.ExtractionDirectory != null)
                Add(new ExecuteCommandOpenInExplorer(activator, new DirectoryInfo(folder.Project.ExtractionDirectory)));
            
            Add(new ExecuteCommandSetProjectExtractionDirectory(_activator, folder.Project));
        }
    }
}