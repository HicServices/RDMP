using CatalogueManager.Menus;
using DataExportLibrary.Providers.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    class ExtractionConfigurationsNodeMenu:RDMPContextMenuStrip
    {
        public ExtractionConfigurationsNodeMenu(RDMPContextMenuStripArgs args, ExtractionConfigurationsNode extractionConfigurationsNode): base(args, extractionConfigurationsNode)
        {
            Add(new ExecuteCommandCreateNewExtractionConfigurationForProject(_activator, extractionConfigurationsNode.Project));
        }
    }
}
