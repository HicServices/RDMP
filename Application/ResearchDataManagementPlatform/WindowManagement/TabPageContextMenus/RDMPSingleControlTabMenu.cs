using System.Windows.Forms;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

namespace ResearchDataManagementPlatform.WindowManagement.TabPageContextMenus
{
    /// <summary>
    /// Right click menu for the top tab section of a docked tab in RDMP main application.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class RDMPSingleControlTabMenu : ContextMenuStrip
    {
        private readonly IActivateItems _activator;
        private readonly RDMPSingleControlTab _tab;

        public RDMPSingleControlTabMenu(IActivateItems activator, RDMPSingleControlTab tab, ContentWindowTracker windowTracker)
        {
            _activator = activator;
            _tab = tab;
            Items.Add("Close All Tabs", null, (s, e) => windowTracker.CloseAllWindows());
            Items.Add("Close All But This", null, (s, e) => windowTracker.CloseAllButThis(tab));

            Items.Add("Show", null, (s, e) => tab.HandleUserRequestingEmphasis(activator));
            Items.Add("Refresh", FamFamFamIcons.arrow_refresh, (s, e) => _tab.HandleUserRequestingTabRefresh(_activator));
        }
    }
}
