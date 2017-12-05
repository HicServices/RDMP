using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality
{
    /// <summary>
    /// Factory that creates custom Forms when a docked tab is dragged out into a new window (See CustomFloatWindow for implementation)
    /// </summary>
    public class CustomFloatWindowFactory: DockPanelExtender.IFloatWindowFactory
    {
        public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane)
        {
            return new CustomFloatWindow(dockPanel,pane);
        }

        public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
        {
            return new CustomFloatWindow(dockPanel,pane,bounds);
        }
    }
}
