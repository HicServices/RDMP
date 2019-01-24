using System;
using System.Windows.Forms;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.PluginChildProvision;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class PluginUserInterfaceCrashedMenuItem : ToolStripMenuItem
    {
        private Exception _exception;

        public PluginUserInterfaceCrashedMenuItem(IPluginUserInterface plugin, Exception exception)
        {
            Text = plugin.GetType().Name + " Crashed";
            Image = CatalogueIcons.Failed;
            _exception = exception;
        }

        protected override void OnClick(EventArgs e)
        {
            ExceptionViewer.Show(_exception);
        }
    }
}