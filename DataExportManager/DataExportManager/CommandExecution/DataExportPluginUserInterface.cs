using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;

namespace DataExportManager.CommandExecution
{
    public class DataExportPluginUserInterface:PluginUserInterface
    {
        
        public DataExportPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {
            
        }
        
        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            var information = o as ExtractionInformation;
            
            if (information != null)
                return
                    GetMenuArray(new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator, information));

            return null;
        }
    }
}
