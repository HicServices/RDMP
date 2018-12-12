using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractionInformationMenu : RDMPContextMenuStrip
    {
        public ExtractionInformationMenu(RDMPContextMenuStripArgs args, ExtractionInformation extractionInformation): base(args,extractionInformation)
        {
            Add(new ExecuteCommandCreateNewFilter(args.ItemActivator,new ExtractionFilterFactory(extractionInformation)));
        }
    }
}
