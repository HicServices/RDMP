using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.SimpleDialogs.Reports;
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
        }

    }
}
