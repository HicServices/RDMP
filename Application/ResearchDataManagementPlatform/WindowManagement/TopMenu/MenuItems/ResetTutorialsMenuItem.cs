using System;
using System.Windows.Forms;
using CatalogueManager.Tutorials;

namespace ResearchDataManagementPlatform.WindowManagement.TopMenu.MenuItems
{
    public class ResetTutorialsMenuItem : ToolStripMenuItem
    {
        private readonly TutorialTracker _tracker;

        public ResetTutorialsMenuItem(ToolStripMenuItem parent, TutorialTracker tracker)
        {
            _tracker = tracker;
            Text = "Reset Tutorials";
            parent.DropDownOpening += parent_DropDownOpening;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            _tracker.ClearCompleted();
        }

        void parent_DropDownOpening(object sender, EventArgs e)
        {
            Enabled = _tracker.IsClearable();
        }


    }
}