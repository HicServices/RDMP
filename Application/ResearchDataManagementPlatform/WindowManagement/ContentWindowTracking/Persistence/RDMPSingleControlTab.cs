using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.Reports;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// TECHNICAL: Base class for all dockable tabs that host a single control 
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    [TechnicalUI]
    public abstract class RDMPSingleControlTab:DockContent,IRefreshBusSubscriber
    {
        protected RDMPSingleControlTab(RefreshBus refreshBus)
        {
            refreshBus.Subscribe(this);
            FormClosed += (s, e) => refreshBus.Unsubscribe(this);
        }

        public abstract Control GetControl();
        public abstract void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e);
        public abstract void HandleUserRequestingTabRefresh(IActivateItems activator);

        public abstract void HandleUserRequestingEmphasis(IActivateItems activator);
    }
}