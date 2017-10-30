using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using DataExportManager.Collections.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    public class ExtractionConfigurationsNodeMenu:RDMPContextMenuStrip
    {
        public ExtractionConfigurationsNodeMenu(IActivateItems activator, ExtractionConfigurationsNode extractionConfigurationsNode): base(activator, null)
        {
            Add(new ExecuteCommandCreateNewExtractionConfigurationForProject(_activator, extractionConfigurationsNode.Project));
        }
    }
}
