using System;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.Tutorials;

namespace ResearchDataManagementPlatform.WindowManagement.TopMenu.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public class LaunchTutorialMenuItem : ToolStripMenuItem
    {
        private IActivateItems _activator;
        private readonly Tutorial _tutorial;
        private readonly TutorialTracker _tracker;

        public LaunchTutorialMenuItem(ToolStripMenuItem parent,IActivateItems activator, Tutorial tutorial, TutorialTracker tracker)
        {
            parent.DropDownOpening += parent_DropDownOpening; 
            _activator = activator;
            _tutorial = tutorial;
            _tracker = tracker;

            UpdateText();
        }

        void parent_DropDownOpening(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            Text = _tutorial.Name;

            if (_tracker.HasSeen(_tutorial))
                Text += " (Seen)";
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            
            _tracker.ClearCompleted(_tutorial);
            _tracker.LaunchTutorial(_tutorial);
        }
    }
}