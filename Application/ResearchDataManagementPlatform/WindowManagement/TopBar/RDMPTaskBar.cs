using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DashboardTabs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Theme;
using CohortManager.Collections;
using DataExportManager.Collections;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.WindowManagement.HomePane;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.TopBar
{
    /// <summary>
    /// Allows you to access the main object collections that make up the RDMP.  These include 
    /// </summary>
    public partial class RDMPTaskBar : UserControl
    {
        private WindowManager _manager;
        
        private const string CreateNewDashboard = "<<New Dashboard>>";
        private const string CreateNewLayout = "<<New Layout>>";

        public RDMPTaskBar()
        {
            InitializeComponent();
            BackColorProvider provider = new BackColorProvider();

            btnHome.Image = FamFamFamIcons.application_home;
            btnCatalogues.Image = CatalogueIcons.Catalogue;
            btnCatalogues.BackgroundImage = provider.GetBackgroundImage(btnCatalogues.Size, RDMPCollection.Catalogue);

            btnCohorts.Image = CatalogueIcons.CohortIdentificationConfiguration;
            btnCohorts.BackgroundImage = provider.GetBackgroundImage(btnCohorts.Size, RDMPCollection.Cohort);

            btnSavedCohorts.Image = CatalogueIcons.AllCohortsNode;
            btnSavedCohorts.BackgroundImage = provider.GetBackgroundImage(btnSavedCohorts.Size, RDMPCollection.SavedCohorts);

            btnDataExport.Image = CatalogueIcons.Project;
            btnDataExport.BackgroundImage = provider.GetBackgroundImage(btnDataExport.Size, RDMPCollection.DataExport);

            btnTables.Image = CatalogueIcons.TableInfo;
            btnTables.BackgroundImage = provider.GetBackgroundImage(btnTables.Size, RDMPCollection.Tables);

            btnLoad.Image = CatalogueIcons.LoadMetadata;
            btnLoad.BackgroundImage = provider.GetBackgroundImage(btnLoad.Size, RDMPCollection.DataLoad);
            
            btnFavourites.Image = CatalogueIcons.Favourite;
        }

        public void SetWindowManager(WindowManager manager)
        {
            _manager = manager;
            _manager.TabChanged += _manager_TabChanged;
            btnDataExport.Enabled = manager.RepositoryLocator.DataExportRepository != null;
            
            ReCreateDropDowns();
            
            SetupToolTipText();

            _manager.ActivateItems.Theme.ApplyTo(toolStrip1);
        }

        void _manager_TabChanged(object sender, IDockContent newTab)
        {
            btnBack.Enabled = _manager.Navigation.CanBack();
            btnForward.Enabled = _manager.Navigation.CanForward();
        }


        private void SetupToolTipText()
        {
            int maxCharactersForButtonTooltips = 200;

            try
            {
                var store = _manager.ActivateItems.RepositoryLocator.CatalogueRepository.CommentStore;

                btnHome.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(HomeUI));
                btnCatalogues.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(CatalogueCollectionUI));
                btnCohorts.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(CohortIdentificationCollectionUI));
                btnSavedCohorts.ToolTipText =  store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(SavedCohortsCollectionUI));
                btnDataExport.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(DataExportCollectionUI));
                btnTables.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(TableInfoCollectionUI));
                btnLoad.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(LoadMetadataCollectionUI));
                btnFavourites.ToolTipText = store.GetTypeDocumentationIfExists(maxCharactersForButtonTooltips, typeof(FavouritesCollectionUI)); 

            }
            catch (Exception e)
            {
                _manager.ActivateItems.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to setup tool tips", CheckResult.Fail, e));
            }

        }

        private void ReCreateDropDowns()
        {
            CreateDropDown<DashboardLayout>(cbxDashboards, CreateNewDashboard);
            CreateDropDown<WindowLayout>(cbxLayouts, CreateNewLayout);
        }

        private void CreateDropDown<T>(ToolStripComboBox cbx, string createNewDashboard) where T:IMapsDirectlyToDatabaseTable, INamed
        {
            const int xPaddingForComboText = 10;

            if (cbx.ComboBox == null)
                throw new Exception("Expected combo box!");
            
            cbx.ComboBox.Items.Clear();

            var objects = _manager.RepositoryLocator.CatalogueRepository.GetAllObjects<T>();

            cbx.ComboBox.Items.Add("");

            //minimum size that it will be (same width as the combo box)
            int proposedComboBoxWidth = cbx.Width - xPaddingForComboText;

            foreach (T o in objects)
            {
                //add dropdown item
                cbx.ComboBox.Items.Add(o);

                //will that label be too big to fit in text box? if so expand the max width
                proposedComboBoxWidth = Math.Max(proposedComboBoxWidth, TextRenderer.MeasureText(o.Name, cbx.Font).Width);
            }

            cbx.DropDownWidth = Math.Min(400, proposedComboBoxWidth + xPaddingForComboText);
            cbx.ComboBox.SelectedItem = "";

            cbx.Items.Add(createNewDashboard);
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

        
        private void cbx_DropDownClosed(object sender, EventArgs e)
        {
            var cbx = (ToolStripComboBox)sender;
            var toOpen = cbx.SelectedItem as INamed;

            if (ReferenceEquals(cbx.SelectedItem, CreateNewDashboard))
                AddNewDashboard();

            if (ReferenceEquals(cbx.SelectedItem, CreateNewLayout))
                AddNewLayout();

            if (toOpen != null)
            {
                var cmd = new ExecuteCommandActivate(_manager.ActivateItems, toOpen);
                cmd.Execute();
            }

            UpdateButtonEnabledness();
        }



        private void cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonEnabledness();
        }

        private void UpdateButtonEnabledness()
        {
            btnSaveWindowLayout.Enabled = cbxLayouts.SelectedItem is WindowLayout;
            btnDeleteLayout.Enabled = cbxLayouts.SelectedItem is WindowLayout;
            btnDeleteDash.Enabled = cbxDashboards.SelectedItem is DashboardLayout;
        }

        private void AddNewLayout()
        {
            string xml = _manager.MainForm.GetCurrentLayoutXml();

            var dialog = new TypeTextOrCancelDialog("Layout Name", "Name", 100, null, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var layout = new WindowLayout(_manager.RepositoryLocator.CatalogueRepository, dialog.ResultText,xml);

                var cmd = new ExecuteCommandActivate(_manager.ActivateItems, layout);
                cmd.Execute();

                ReCreateDropDowns();
            }
        }

        private void AddNewDashboard()
        {
            var dialog = new TypeTextOrCancelDialog("Dashboard Name", "Name", 100, null, false);
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                var dash = new DashboardLayout(_manager.RepositoryLocator.CatalogueRepository, dialog.ResultText);
                
                var cmd = new ExecuteCommandActivate(_manager.ActivateItems, dash);
                cmd.Execute();

                ReCreateDropDowns();
            }
        }

        public void InjectButton(ToolStripButton button)
        {
            toolStrip1.Items.Add(button);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            ToolStripComboBox cbx;
            if (sender == btnDeleteDash)
                cbx = cbxDashboards;
            else if (sender == btnDeleteLayout)
                cbx = cbxLayouts;
            else
                throw new Exception("Unexpected sender");

            var d = cbx.SelectedItem as IDeleteable;
            if (d != null)
            {
                _manager.ActivateItems.DeleteWithConfirmation(this, d);
                ReCreateDropDowns();
            }
        }

        private void btnSaveWindowLayout_Click(object sender, EventArgs e)
        {
            var layout = cbxLayouts.SelectedItem as WindowLayout;
            if(layout != null)
            {
                string xml = _manager.MainForm.GetCurrentLayoutXml();

                layout.LayoutData = xml;
                layout.SaveToDatabase();
            }
        }

        private void btnBack_ButtonClick(object sender, EventArgs e)
        {
            _manager.Navigation.Back(true);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            _manager.Navigation.Forward(true);
        }

        private void btnBack_DropDownOpening(object sender, EventArgs e)
        {
            btnBack.DropDownItems.Clear();

            int backIndex = 1;

            foreach (DockContent history in _manager.Navigation.GetHistory(16))
            {
                var i = backIndex++;
                btnBack.DropDownItems.Add(history.TabText,null,(a,b)=>_manager.Navigation.Back(i,true));
            }
        }
    }
}
