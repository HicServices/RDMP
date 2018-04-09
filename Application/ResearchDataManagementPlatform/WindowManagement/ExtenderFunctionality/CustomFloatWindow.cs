using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.Menus.MenuItems;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality
{
    /// <summary>
    /// Determines the window style of tabs dragged out of the main RDMPMainForm window to create new windows of that tab only.  Currently the only change is to allow the user to resize
    ///  and maximise new tab windows
    /// </summary>
    [TechnicalUI]
    [System.ComponentModel.DesignerCategory("")]
    public class CustomFloatWindow:FloatWindow
    {
        protected internal CustomFloatWindow(DockPanel dockPanel, DockPane pane) : base(dockPanel, pane)
        {
            Initialize();
            
        }
        protected internal CustomFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds): base(dockPanel, pane, bounds)
        {
            Initialize();
        }

        private void Initialize()
        {
            FormBorderStyle = FormBorderStyle.Sizable;

            var saveToolStripMenuItem = new SaveMenuItem();
            var singleObjectControlTab = this.DockPanel.ActiveDocument as RDMPSingleControlTab;

            if (singleObjectControlTab == null)
            {
                saveToolStripMenuItem.Saveable = null;
                return;
            }

            var saveable = singleObjectControlTab.GetControl() as ISaveableUI;
            saveToolStripMenuItem.Saveable = saveable;
        }
    }
}
