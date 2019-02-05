using System;
using System.Text;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.Reports;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
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

        public void ShowHelp(IActivateItems activator)
        {


            var typeDocs = activator.RepositoryLocator.CatalogueRepository.CommentStore;

            StringBuilder sb = new StringBuilder();

            string firstMatch = null;

            foreach (var c in Controls)
                if (typeDocs.ContainsKey(c.GetType().Name))
                {
                    if (firstMatch == null)
                        firstMatch = c.GetType().Name;

                    sb.AppendLine(typeDocs.GetDocumentationIfExists(c.GetType().Name,false,true));
                    sb.AppendLine();
                }

            if (sb.Length > 0)
                WideMessageBox.Show(firstMatch, sb.ToString(),Environment.StackTrace,  true,  firstMatch,WideMessageBoxTheme.Help);
        }
    }
}