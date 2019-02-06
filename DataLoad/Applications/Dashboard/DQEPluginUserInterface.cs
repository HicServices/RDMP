using System.Collections.Generic;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataQualityUIs;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.LoadExecutionUIs;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Validation;
using Dashboard.CommandExecution.AtomicCommands;
using Dashboard.Menus.MenuItems;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Dashboard
{
    internal class DQEPluginUserInterface : PluginUserInterface
    {

        public DQEPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {

        }

        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            var c = o as Catalogue;

            if (c != null)
                return new[]{ new DQEMenuItem(ItemActivator,c)};

            var lmd = o as LoadMetadata;

            if (lmd != null)
                return GetMenuArray(
                    new ExecuteCommandViewLoadMetadataLogs(ItemActivator).SetTarget(lmd)
                    );

            return null;
        }

        public override IEnumerable<IAtomicCommand> GetAdditionalCommandsForControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity databaseEntity)
        {
            if(control is DQEExecutionControl)
                return new[] {new ExecuteCommandViewDQEResultsForCatalogue(ItemActivator){OverrideCommandName = "View Results..."}.SetTarget(databaseEntity)};

            if (control is ValidationSetupForm)
                return new[]
                {
                    new ExecuteCommandRunDQEOnCatalogue(ItemActivator).SetTarget(control.DatabaseObject),
                    new ExecuteCommandViewDQEResultsForCatalogue(ItemActivator) { OverrideCommandName = "View Results..." }.SetTarget(databaseEntity)
                
                };

            if (control is ExecuteLoadMetadataUI)
                return new[] {new ExecuteCommandViewLoadMetadataLogs(ItemActivator, (LoadMetadata) databaseEntity)};

            return base.GetAdditionalCommandsForControl(control, databaseEntity);
        }
    }
}
