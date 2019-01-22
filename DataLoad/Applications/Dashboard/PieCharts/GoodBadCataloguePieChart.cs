using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.SingleControlForms;

namespace Dashboard.PieCharts
{
    /// <summary>
    /// Part of OverviewScreen, shows a pie chart describing how good or bad the situation is with respect to one of the following:
    /// 
    /// <para>Issues - How many outstanding issues are there (See IssueUI)</para>
    /// 
    /// <para>Empty Descriptions - How many extractable columns are there which do not yet have descriptions in the Data Catalogue Database (See CatalogueItemTab)</para>
    /// 
    /// <para>Each of these can either be displayed for a single catalogue or as a combined total across all active catalogues (not deprecated / internal etc)</para>
    /// 
    /// </summary>
    public partial class GoodBadCataloguePieChart : UserControl,IDashboardableControl
    {
        public GoodBadCataloguePieChart()
        {
            InitializeComponent();
            ddChartType.ComboBox.DataSource = Enum.GetValues(typeof (CataloguePieChartType));
            ddChartType.ComboBox.SelectionChangeCommitted += ComboBox_SelectionChangeCommitted;
            
            btnRefresh.Image = FamFamFamIcons.arrow_refresh;
            btnShowLabels.Image = FamFamFamIcons.text_list_bullets;
            
            //put edit mode on for the designer
            NotifyEditModeChange(false);
        }
        
        private DashboardControl _dashboardControlDatabaseRecord;
        private GoodBadCataloguePieChartObjectCollection _collection;
        private IActivateItems _activator;
        
        private void GenerateChart()
        {
            chart1.Visible = false;
            lblNoIssues.Visible = false;
            pbLoading.Visible = true;
            switch (_collection.PieChartType)
            {
                case CataloguePieChartType.Issues:

                    if (_collection.IsSingleCatalogueMode)
                        gbWhatThisIs.Text = "Issues in " + _collection.GetSingleCatalogueModeCatalogue();
                    else
                        gbWhatThisIs.Text = "All Issues";

                    PopulateAsIssueChartAsync();
                    break;
                case CataloguePieChartType.EmptyDescriptions:

                    if (_collection.IsSingleCatalogueMode)
                        gbWhatThisIs.Text = "Column Descriptions in " + _collection.GetSingleCatalogueModeCatalogue();
                    else
                        gbWhatThisIs.Text = "Column Descriptions";

                    PopulateAsEmptyDescriptionsChartAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("pieType");
            }
        }
        
        private void PopulateAsEmptyDescriptionsChartAsync()
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    ExtractionInformation[] allExtractionInformation;
                    if (!_collection.IsSingleCatalogueMode)
                    {
                        //get the active (non depricated etc) Catalogues
                        var activeCatalogues = _activator.CoreChildProvider.AllCatalogues.Where(ShouldHaveDescription).ToArray();
                        
                        //if there are some
                        if(activeCatalogues.Any())
                            allExtractionInformation = activeCatalogues.SelectMany(c=>c.GetAllExtractionInformation(ExtractionCategory.Any)).ToArray();//get the extractable columns
                        else
                            allExtractionInformation = new ExtractionInformation[0];//there weren't any so Catalogues so wont be any ExtractionInformationsEither
                    }
                    else
                        allExtractionInformation = _collection.GetSingleCatalogueModeCatalogue().GetAllExtractionInformation(ExtractionCategory.Any);

                    if (!allExtractionInformation.Any())
                    {
                        //form was closed while we were loading data
                        if (IsDisposed || !IsHandleCreated)
                            return;

                        this.Invoke(new MethodInvoker(() =>
                        {
                            chart1.DataSource = null;
                            chart1.Visible = false;
                            lblNoIssues.Visible = true;
                            pbLoading.Visible = false;
                        }));
                    
                        return;
                    }

                    int countPopulated = 0;
                    int countNotPopulated = 0;
                    
                    foreach (ExtractionInformation information in allExtractionInformation)
                        if (string.IsNullOrWhiteSpace(information.CatalogueItem.Description))
                            countNotPopulated++;
                        else
                            countPopulated++;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Count");
                    dt.Columns.Add("State");


                    dt.Rows.Add(new object[] { countNotPopulated, "Missing (" + countNotPopulated + ")" });
                    dt.Rows.Add(new object[] { countPopulated, "Populated (" + countPopulated + ")" });


                    if (IsDisposed || !IsHandleCreated)
                        return;
                    this.Invoke(new MethodInvoker(() =>
                    {
                        if (IsDisposed || !IsHandleCreated)
                            return;

                        chart1.Series[0].XValueMember = dt.Columns[1].ColumnName;
                        chart1.Series[0].YValueMembers = dt.Columns[0].ColumnName;

                        chart1.DataSource = dt;
                        chart1.DataBind();
                        chart1.Visible = true;
                        lblNoIssues.Visible = false;
                        pbLoading.Visible = false;
                    }));
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(this.GetType().Name + " failed to load data", e);
                }
            });

            t.Start();
          
        }

        public bool ShouldHaveDescription(Catalogue c)
        {
            return !c.IsColdStorageDataset && !c.IsInternalDataset && !c.IsDeprecated && !c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository);
        }

        private void PopulateAsIssueChartAsync()
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    CatalogueItemIssue[] Issues;

                    if (!_collection.IsSingleCatalogueMode)
                    {
                        var cataRepo = _activator.RepositoryLocator.CatalogueRepository;
                        Issues = cataRepo.GetAllObjects<CatalogueItemIssue>().ToArray();
                        
                    }
                    else
                        Issues = _collection.GetSingleCatalogueModeCatalogue().GetAllIssues();

                    if (Issues.Any())
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("Count");
                        dt.Columns.Add("State");

                        int countOutstanding = Issues.Count(i => i.Status != IssueStatus.Resolved);
                        int countResolved = Issues.Count(i => i.Status == IssueStatus.Resolved);


                        dt.Rows.Add(new object[] {countOutstanding, "Outstanding (" + countOutstanding + ")"});
                        dt.Rows.Add(new object[] {countResolved, "Resolved (" + countResolved + ")"});

                        if (IsDisposed || !IsHandleCreated)
                            return;

                        this.Invoke(new MethodInvoker(() =>
                        {

                            chart1.Series[0].XValueMember = dt.Columns[1].ColumnName;
                            chart1.Series[0].YValueMembers = dt.Columns[0].ColumnName;
                        
                            chart1.DataSource = dt;
                            chart1.DataBind();
                            chart1.Visible = true;
                            lblNoIssues.Visible = false;
                            pbLoading.Visible = false;
                        }));

                    }
                    else
                    {
                        if (IsDisposed || !IsHandleCreated)
                            return;

                        this.Invoke(new MethodInvoker(() =>
                        {
                            chart1.DataSource = null;
                            chart1.Visible = false;
                            lblNoIssues.Visible = true;
                            pbLoading.Visible = false;
                        }));
                    }
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(this.GetType().Name + " failed to load data", e);
                }
                
            });

            t.Start();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        bool _bLoading;
        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _bLoading = true;
            _activator = activator;
            _collection = (GoodBadCataloguePieChartObjectCollection)collection;

            btnAllCatalogues.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.CatalogueItemsNode);
            btnSingleCatalogue.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Link);

            ddChartType.SelectedItem = _collection.PieChartType;
            btnAllCatalogues.Checked = !_collection.IsSingleCatalogueMode;
            btnSingleCatalogue.Checked = _collection.IsSingleCatalogueMode;
            btnShowLabels.Checked = _collection.ShowLabels;

            activator.Theme.ApplyTo(toolStrip1);

            GenerateChart();
            _bLoading = false;
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public string GetTabName()
        {
            return Text;
        }


        public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
        {
            _dashboardControlDatabaseRecord = databaseRecord;

            return new GoodBadCataloguePieChartObjectCollection();
        }

        public void NotifyEditModeChange(bool isEditModeOn)
        {
            var l = new Point(Margin.Left,Margin.Right);
            var s = new Size(Width - Margin.Horizontal, Height - Margin.Vertical);

            if (isEditModeOn)
            {
                Controls.Add(toolStrip1);
                gbWhatThisIs.Location = new Point(l.X, l.Y + 25);//move it down 25 to allow space for tool bar
                gbWhatThisIs.Size = new Size(s.Width, s.Height - 25);//and adjust height accordingly
            }
            else
            {
                Controls.Remove(toolStrip1);
                gbWhatThisIs.Location = l;
                gbWhatThisIs.Size = s;
            }
        }

        private void btnAllCatalogues_Click(object sender, EventArgs e)
        {
            btnAllCatalogues.Checked = true;
            btnSingleCatalogue.Checked = false;
            _collection.SetAllCataloguesMode();
            GenerateChart();
            SaveCollectionChanges();
        }

        private void btnSingleCatalogue_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.RepositoryLocator.CatalogueRepository.GetAllCatalogues(), false,false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var c = (Catalogue) dialog.Selected;
                _collection.SetSingleCatalogueMode(c);
                
                btnAllCatalogues.Checked = false;
                btnSingleCatalogue.Checked = true;

                SaveCollectionChanges();
                GenerateChart();
            }
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GenerateChart();
        }
        void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if(ddChartType.SelectedItem == null || _bLoading)
                return;

            var newType = (CataloguePieChartType) ddChartType.SelectedItem;

            //no change
            if(newType == _collection.PieChartType)
                return;
            
            _collection.PieChartType = newType;
            GenerateChart();
            SaveCollectionChanges();
        }

        private void SaveCollectionChanges()
        {
            if (_bLoading)
                return;

            _dashboardControlDatabaseRecord.SaveCollectionState(_collection);
        }
        
        private void btnShowLabels_CheckStateChanged(object sender, EventArgs e)
        {
            _collection.ShowLabels = btnShowLabels.Checked;

            chart1.Series[0]["PieLabelStyle"] = _collection.ShowLabels ? "Inside" : "Disabled";
            SaveCollectionChanges();
        }

        private void btnViewDataTable_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Catalogue");
            dt.Columns.Add("Count Missing Descriptions");
            dt.Columns.Add("Missing List");
            
            foreach (IGrouping<Catalogue, ExtractionInformation> g in _activator.CoreChildProvider.AllExtractionInformations.GroupBy(ei=>ei.CatalogueItem.Catalogue))
            {
                if (!ShouldHaveDescription(g.Key))
                    continue;
                
                var missing = g.Where(ei => string.IsNullOrWhiteSpace(ei.CatalogueItem.Description)).ToArray();
                dt.Rows.Add(g.Key.Name, missing.Count(), string.Join(",",missing.Select(m=>m.CatalogueItem.Name)));
            }

            DataTableViewer dtv = new DataTableViewer(dt,"Catalogue Items Missing Descriptions");

            var form = new SingleControlForm(dtv, true);
            form.Show();
        }
    }

    public enum CataloguePieChartType
    {
        Issues,
        EmptyDescriptions
    }
}
