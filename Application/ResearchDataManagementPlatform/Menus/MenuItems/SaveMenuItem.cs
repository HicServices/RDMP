using System;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;

namespace ResearchDataManagementPlatform.Menus.MenuItems
{
    /// <summary>
    /// Provides a shortcut to save the currently selected ISaveableUI.  This class requires that you track and regularly update the Saveable property to match
    /// the currently selected saveable tab
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class SaveMenuItem : ToolStripMenuItem
    {
        private ISaveableUI _saveable;

        public ISaveableUI Saveable
        {
            get { return _saveable; }
            set
            {
                _saveable = value;
                Enabled = value != null;
            }
        }

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
