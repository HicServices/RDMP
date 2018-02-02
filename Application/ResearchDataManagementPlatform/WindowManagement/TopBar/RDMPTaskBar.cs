using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Providers;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers.Filtering;
using CatalogueManager.DashboardTabs;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.SimpleDialogs.NavigateTo;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Events;
using ReusableLibraryCode.Icons.IconProvision;

namespace ResearchDataManagementPlatform.WindowManagement.TopBar
{
    /// <summary>
    /// Allows you to access the main object collections that make up the RDMP.  These include 
    /// </summary>
    public partial class RDMPTaskBar : UserControl
    {
        private ToolboxWindowManager _manager;


        private readonly List<DashboardLayoutUI> _visibleLayouts = new List<DashboardLayoutUI>();
        private DashboardLayoutUI _lastPoppedDashboard;

        public RDMPTaskBar()
        {
            InitializeComponent();
            btnHome.Image = FamFamFamIcons.application_home;
            btnCatalogues.Image = CatalogueIcons.Catalogue;
            btnCohorts.Image = CatalogueIcons.CohortIdentificationConfiguration;
            btnSavedCohorts.Image = CatalogueIcons.AllCohortsNode;
            btnDataExport.Image = CatalogueIcons.Project;
            btnTables.Image = CatalogueIcons.TableInfo;
            btnLoad.Image = CatalogueIcons.LoadMetadata;
            btnFavourites.Image = CatalogueIcons.Favourite;
        }

        public void SetWindowManager(ToolboxWindowManager manager)
        {
            _manager = manager;
            btnDataExport.Enabled = manager.RepositoryLocator.DataExportRepository != null;
            
            btnAddDashboard.Image = manager.ContentManager.CoreIconProvider.GetImage(RDMPConcept.DashboardLayout,OverlayKind.Add);
            ReCreateDashboardsDropDown();
        }
        

        private void ReCreateDashboardsDropDown()
        {
            const int xPaddingForComboText = 10;

            if (cbxDashboards.ComboBox == null)
                throw new Exception("Expected combo box!");

            cbxDashboards.ComboBox.Items.Clear();

            var dashboards = _manager.RepositoryLocator.CatalogueRepository.GetAllObjects<DashboardLayout>();

            cbxDashboards.ComboBox.Items.Add("");

            //minimum size that it will be (same width as the combo box)
            int proposedComboBoxWidth = cbxDashboards.Width - xPaddingForComboText;

            foreach (DashboardLayout dashboard in dashboards)
            {
                //add dropdown item
                cbxDashboards.ComboBox.Items.Add(dashboard);
                
                //will that label be too big to fit in text box? if so expand the max width
                proposedComboBoxWidth = Math.Max(proposedComboBoxWidth,TextRenderer.MeasureText(dashboard.Name, cbxDashboards.Font).Width);
            }

            cbxDashboards.DropDownWidth = Math.Min(400, proposedComboBoxWidth + xPaddingForComboText);
            cbxDashboards.ComboBox.SelectedItem = "";
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            _manager.CloseAllToolboxes();
            _manager.CloseAllWindows();
            _manager.PopHome();
        }

        private void ToolboxButtonClicked(object sender, EventArgs e)
        {
            RDMPCollection collection = ButtonToEnum(sender);

            if (_manager.IsVisible(collection))
                _manager.Pop(collection);
            else
                _manager.Create(collection);
        }

        private void OnFormClosed(object sender, FormClosedEventArgs formClosedEventArgs)
        {
            var layoutUI = sender as DashboardLayoutUI;

            if (layoutUI != null)
            {
                _visibleLayouts.Remove(layoutUI);

                //if it closed because it was deleted
                if(!layoutUI.DatabaseObject.Exists())
                    ReCreateDashboardsDropDown();
            }
        }

        private RDMPCollection ButtonToEnum(object button)
        {
            RDMPCollection collectionToToggle;

            if (button == btnCatalogues)
                collectionToToggle = RDMPCollection.Catalogue;
            else
            if (button == btnCohorts)
                collectionToToggle = RDMPCollection.Cohort;
            else
            if (button == btnDataExport)
                collectionToToggle = RDMPCollection.DataExport;
            else
            if (button == btnTables)
                collectionToToggle = RDMPCollection.Tables;
            else
            if (button == btnLoad)
                collectionToToggle = RDMPCollection.DataLoad;
            else if (button == btnSavedCohorts)
                collectionToToggle = RDMPCollection.SavedCohorts;
            else if (button == btnFavourites)
                collectionToToggle = RDMPCollection.Favourites;
            else
                throw new ArgumentOutOfRangeException();

            return collectionToToggle;
        }

        private ToolStripButton EnumToButton(RDMPCollection collection)
        {
            switch (collection)
            {
                case RDMPCollection.None:
                    return null;
                case RDMPCollection.Tables:
                    return btnTables;
                case RDMPCollection.Catalogue:
                    return btnCatalogues;
                case RDMPCollection.DataExport:
                    return btnDataExport;
                case RDMPCollection.Cohort:
                    return btnCohorts;
                case RDMPCollection.DataLoad:
                    return btnLoad;
                case RDMPCollection.SavedCohorts:
                    return btnSavedCohorts;
                case RDMPCollection.Favourites:
                    return btnFavourites;
                default:
                    throw new ArgumentOutOfRangeException("collection");
            }
        }

        private void cbxDashboards_DropDownClosed(object sender, EventArgs e)
        {
            var layoutToOpen = cbxDashboards.SelectedItem as DashboardLayout;

            //if the ui selection hasn't changed from the last one the user selected and that ui is still open
            if (_lastPoppedDashboard != null && _lastPoppedDashboard.DatabaseObject.Equals(layoutToOpen) && _visibleLayouts.Any(ui => ui.DatabaseObject.Equals(layoutToOpen)))
                return;
            
            
            if (layoutToOpen != null)
            {
                var ui = _manager.ContentManager.ActivateDashboard(this, layoutToOpen);
                ui.ParentForm.FormClosed += (s, ev) => OnFormClosed(ui, ev);
                _visibleLayouts.Add(ui);
                _lastPoppedDashboard = ui;
            }
        }

        private void btnAddDashboard_Click(object sender, EventArgs e)
        {
            var layout = new DashboardLayout(_manager.RepositoryLocator.CatalogueRepository, "NewLayout " + Guid.NewGuid());
            var ui = _manager.ContentManager.ActivateDashboard(this, layout);

            _visibleLayouts.Add(ui);
            ui.ParentForm.FormClosed += (s,ev)=>OnFormClosed(ui,ev);

            ReCreateDashboardsDropDown();
        }

        public void InjectButton(ToolStripButton button)
        {
            toolStrip1.Items.Add(button);
        }
    }
}
