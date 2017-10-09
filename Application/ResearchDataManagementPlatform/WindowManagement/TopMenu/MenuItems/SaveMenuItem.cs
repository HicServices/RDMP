using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;

namespace ResearchDataManagementPlatform.WindowManagement.TopMenu.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class SaveMenuItem : ToolStripMenuItem
    {
        private readonly IActivateItems _activator;
        public ISaveableUI Saveable { get; set; }

        public SaveMenuItem() : base("Save")
        {
            ShortcutKeys = (Keys.Control | Keys.S);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Saveable.GetObjectSaverButton().Save();
        }
    }
}
