using NPOI.OpenXmlFormats.Dml;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.PieCharts;

public partial class CatalogueToDatasetLinkagePieChart : RDMPUserControl, IDashboardableControl
{

    private DashboardControl _dashboardControlDatabaseRecord;
    private CatalogueToDatasetLinkageObjectCollection _collection;
    public CatalogueToDatasetLinkagePieChart()
    {
        InitializeComponent();
        btnViewDataTable.Click += btnAllCatalogues_Click;
    }

    public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
    {
        _dashboardControlDatabaseRecord = databaseRecord;

        return new CatalogueToDatasetLinkageObjectCollection();
    }

    public IPersistableObjectCollection GetCollection()
    {
        return _collection;
    }

    public string GetTabName()
    {
        return Text;
    }

    public string GetTabToolTip()
    {
        return null;
    }

    public void NotifyEditModeChange(bool isEditModeOn)
    {
        var l = new Point(Margin.Left, Margin.Right);
        var s = new Size(Width - Margin.Horizontal, Height - Margin.Vertical);

        CommonFunctionality.ToolStrip.Visible = isEditModeOn;

        if (isEditModeOn)
        {
            gbWhatThisIs.Location = l with { Y = l.Y + 25 }; //move it down 25 to allow space for tool bar
            gbWhatThisIs.Size = s with { Height = s.Height - 25 }; //and adjust height accordingly
        }
        else
        {
            gbWhatThisIs.Location = l;
            gbWhatThisIs.Size = s;
        }
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    private void SetupFlags() { }

    private void GenerateChart()
    {
        chart1.Visible = false;
        lblNoIssues.Visible = false;

        gbWhatThisIs.Text = "TODO!";

        PopulateAsEmptyDescriptionsChart();
    }

    private void PopulateAsEmptyDescriptionsChart()
    {
        var dt = new DataTable();
        dt.Columns.Add("Count");
        dt.Columns.Add("State");

        dt.Rows.Add(new object[] { 100, $"Missing ({100})" });
        dt.Rows.Add(new object[] { 100, $"Populated ({100})" });

        chart1.Series[0].XValueMember = dt.Columns[1].ColumnName;
        chart1.Series[0].YValueMembers = dt.Columns[0].ColumnName;

        chart1.DataSource = dt;
        chart1.DataBind();
        chart1.Visible = true;
        lblNoIssues.Visible = false;
    }
    private void btnAllCatalogues_Click(object sender, EventArgs e)
    {
        //btnAllCatalogues.Checked = true;
        //btnSingleCatalogue.Checked = false;
        //_collection.SetAllCataloguesMode();
        //GenerateChart();``
        SaveCollectionChanges();
    }

    private void SaveCollectionChanges()
    {
        if (_bLoading)
            return;

        _dashboardControlDatabaseRecord.SaveCollectionState(_collection);
    }

    private bool _bLoading;
    private bool firstTime = true;
    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        _bLoading = true;
        SetItemActivator(activator);

        _collection = (CatalogueToDatasetLinkageObjectCollection)collection;

        if (firstTime)
            SetupFlags();

        //btnAllCatalogues.Checked = !_collection.IsSingleCatalogueMode;
        //btnSingleCatalogue.Checked = _collection.IsSingleCatalogueMode;
        //btnShowLabels.Checked = _collection.ShowLabels;

        //CommonFunctionality.Add(btnAllCatalogues);
        //CommonFunctionality.Add(toolStripLabel1);
        //CommonFunctionality.Add(btnAllCatalogues);
        //CommonFunctionality.Add(btnSingleCatalogue);
        //CommonFunctionality.Add(btnShowLabels);
        //CommonFunctionality.Add(btnRefresh);

        //foreach (var mi in _flagOptions)
        //    CommonFunctionality.AddToMenu(mi);

        GenerateChart();
        _bLoading = false;
    }
}
