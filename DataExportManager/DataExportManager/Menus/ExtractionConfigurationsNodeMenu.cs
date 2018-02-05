using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using DataExportManager.Collections.Nodes;
using DataExportManager.CommandExecution.AtomicCommands;

namespace DataExportManager.Menus
{
    class ExtractionConfigurationsNodeMenu:RDMPContextMenuStrip
    {
        public ExtractionConfigurationsNodeMenu(RDMPContextMenuStripArgs args, ExtractionConfigurationsNode extractionConfigurationsNode)
            : base(args, null)
        {
            Add(new ExecuteCommandCreateNewExtractionConfigurationForProject(_activator, extractionConfigurationsNode.Project));
        }
    }
}
