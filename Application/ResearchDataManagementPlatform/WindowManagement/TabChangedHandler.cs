using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Occurs when user changes which tab has focus
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="newTab">The newly focused tab</param>
    public delegate void TabChangedHandler(object sender, DockContent newTab);
}