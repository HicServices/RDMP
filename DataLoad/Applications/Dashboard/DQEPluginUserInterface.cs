using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using Dashboard.CommandExecution.AtomicCommands;
using Dashboard.Menus.MenuItems;

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
    }
}
