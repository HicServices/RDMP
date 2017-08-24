using System;
using System.Windows.Forms;
using CatalogueManager.Tutorials;

namespace ResearchDataManagementPlatform.WindowManagement.TopMenu.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class DisableTutorialsMenuItem : ToolStripMenuItem
    {
        private readonly TutorialTracker _tracker;

        public DisableTutorialsMenuItem(ToolStripMenuItem parent, TutorialTracker tracker)
        {
            parent.DropDownOpened += parent_DropDownOpened;
            _tracker = tracker;
            Text = "Disable Tutorials";
        }

        void parent_DropDownOpened(object sender, EventArgs e)
        {
            Checked = _tracker.IsDisableAllTutorialsOn();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _tracker.DisableAllTutorials();
        }
    }
}