// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.PieCharts;

/// <summary>
/// Part of OverviewScreen, shows a pie chart showing how many columns there are which do not yet have associated datasets in the Data Catalogue Database (See CatalogueItemUI)
/// 
/// <para>Each of these can either be displayed for a single catalogue or as a combined total across all active catalogues (not deprecated / internal etc)</para>
/// 
/// </summary>
public partial class CatalogueToDatasetLinkagePieChartUI : RDMPUserControl, IDashboardableControl
{
    private readonly ToolStripButton _btnSingleCatalogue = new("Single", CatalogueIcons.Catalogue.ImageToBitmap())
    { Name = "btnSingleCatalogue" };

    private readonly ToolStripButton _btnAllCatalogues =
        new("All", CatalogueIcons.AllCataloguesUsedByLoadMetadataNode.ImageToBitmap()) { Name = "btnAllCatalogues" };

    private readonly ToolStripButton _btnRefresh = new("Refresh", FamFamFamIcons.text_list_bullets.ImageToBitmap())
    { Name = "btnRefresh" };

    private readonly ToolStripLabel _toolStripLabel1 = new("Type:") { Name = "toolStripLabel1" };

    private readonly ToolStripButton _btnShowLabels = new("Labels", FamFamFamIcons.text_align_left.ImageToBitmap())
    { Name = "btnShowLabels", CheckOnClick = true };

    private readonly List<ToolStripMenuItem> _flagOptions = new();

    public CatalogueToDatasetLinkagePieChartUI()
    {
        InitializeComponent();

        btnViewDataTable.Image = CatalogueIcons.TableInfo.ImageToBitmap();

        _btnAllCatalogues.Click += btnAllCatalogues_Click;
        _btnSingleCatalogue.Click += btnSingleCatalogue_Click;
        _btnShowLabels.CheckStateChanged += btnShowLabels_CheckStateChanged;
        _btnRefresh.Click += btnRefresh_Click;

        //put edit mode on for the designer
        NotifyEditModeChange(false);
    }

    private void SetupFlags()
    {
        if (!_firstTime)
            return;

        AddFlag("Non Extractable Catalogues", c => c.IncludeNonExtractableCatalogues,
            (c, r) => c.IncludeNonExtractableCatalogues = r);
        AddFlag("Deprecated Catalogues", c => c.IncludeDeprecatedCatalogues,
            (c, r) => c.IncludeDeprecatedCatalogues = r);
        AddFlag("Internal Catalogues", c => c.IncludeInternalCatalogues, (c, r) => c.IncludeInternalCatalogues = r);
        AddFlag("Project Specific Catalogues", c => c.IncludeProjectSpecificCatalogues,
            (c, r) => c.IncludeProjectSpecificCatalogues = r);

        AddFlag("Non Extractable CatalogueItems", c => c.IncludeNonExtractableCatalogueItems,
            (c, r) => c.IncludeNonExtractableCatalogueItems = r);
        AddFlag("Internal Catalogue Items", c => c.IncludeInternalCatalogueItems,
            (c, r) => c.IncludeInternalCatalogueItems = r);
        AddFlag("Deprecated Catalogue Items", c => c.IncludeDeprecatedCatalogueItems,
            (c, r) => c.IncludeDeprecatedCatalogueItems = r);
        _firstTime = false;
    }

    private void AddFlag(string caption, Func<GoodBadCataloguePieChartObjectCollection, bool> getProp,
        Action<GoodBadCataloguePieChartObjectCollection, bool> setProp)
    {
        var btn = new ToolStripMenuItem(caption)
        {
            Checked = getProp(_collection)
        };
        btn.CheckedChanged += (sender, e) => { setProp(_collection, ((ToolStripMenuItem)sender).Checked); };
        btn.CheckedChanged += (s, e) => GenerateChart();
        btn.CheckOnClick = true;
        _flagOptions.Add(btn);
    }

    private DashboardControl _dashboardControlDatabaseRecord;
    private GoodBadCataloguePieChartObjectCollection _collection;

    private void GenerateChart()
    {
        chart1.Visible = false;
        lblNoIssues.Visible = false;

        gbWhatThisIs.Text = _collection.IsSingleCatalogueMode
            ? $"Column Descriptions in {_collection.GetSingleCatalogueModeCatalogue()}"
            : "Column Descriptions";

        PopulateAsEmptyDescriptionsChart();
    }

    private void PopulateAsEmptyDescriptionsChart()
    {
        try
        {
            var catalogueItems = GetCatalogueItems();

            if (!catalogueItems.Any())
            {
                chart1.DataSource = null;
                chart1.Visible = false;
                lblNoIssues.Visible = true;

                return;
            }

            var countPopulated = 0;
            var countNotPopulated = 0;
            foreach (var ci in catalogueItems)
                if (ci.ColumnInfo.Dataset_ID is null)
                    countNotPopulated++;
                else
                    countPopulated++;

            var dt = new DataTable();
            dt.Columns.Add("Count");
            dt.Columns.Add("State");

            dt.Rows.Add(new object[] { countNotPopulated, $"Missing ({countNotPopulated})" });
            dt.Rows.Add(new object[] { countPopulated, $"Populated ({countPopulated})" });

            chart1.Series[0].XValueMember = dt.Columns[1].ColumnName;
            chart1.Series[0].YValueMembers = dt.Columns[0].ColumnName;

            chart1.DataSource = dt;
            chart1.DataBind();
            chart1.Visible = true;
            lblNoIssues.Visible = false;
        }
        catch (Exception e)
        {
            ExceptionViewer.Show($"{GetType().Name} failed to load data", e);
        }
    }

    private CatalogueItem[] GetCatalogueItems()
    {
        if (!_collection.IsSingleCatalogueMode)
        {
            var catalogues = Activator.CoreChildProvider.AllCatalogues
                .Where(c => _collection.Include(c, Activator.RepositoryLocator.DataExportRepository)).ToArray();

            //if there are some
            return catalogues.Any()
                ? catalogues.SelectMany(c => c.CatalogueItems).Where(ci => _collection.Include(ci)).ToArray()
                : //get the extractable columns
                Array.Empty<CatalogueItem>(); //there weren't any so Catalogues so won't be any ExtractionInformationsEither
        }

        return _collection.GetSingleCatalogueModeCatalogue().CatalogueItems;
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    private bool _bLoading;
    private bool _firstTime = true;

    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        _bLoading = true;
        SetItemActivator(activator);

        _collection = (GoodBadCataloguePieChartObjectCollection)collection;

        if (_firstTime)
            SetupFlags();

        _btnAllCatalogues.Checked = !_collection.IsSingleCatalogueMode;
        _btnSingleCatalogue.Checked = _collection.IsSingleCatalogueMode;
        _btnShowLabels.Checked = _collection.ShowLabels;

        CommonFunctionality.Add(_btnAllCatalogues);
        CommonFunctionality.Add(_toolStripLabel1);
        CommonFunctionality.Add(_btnAllCatalogues);
        CommonFunctionality.Add(_btnSingleCatalogue);
        CommonFunctionality.Add(_btnShowLabels);
        CommonFunctionality.Add(_btnRefresh);

        foreach (var mi in _flagOptions)
            CommonFunctionality.AddToMenu(mi);

        GenerateChart();
        _bLoading = false;
    }

    public IPersistableObjectCollection GetCollection() => _collection;

    public string GetTabName() => Text;

    public string GetTabToolTip() => null;

    public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
    {
        _dashboardControlDatabaseRecord = databaseRecord;

        return new GoodBadCataloguePieChartObjectCollection();
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

    private void btnAllCatalogues_Click(object sender, EventArgs e)
    {
        _btnAllCatalogues.Checked = true;
        _btnSingleCatalogue.Checked = false;
        _collection.SetAllCataloguesMode();
        GenerateChart();
        SaveCollectionChanges();
    }

    private void btnSingleCatalogue_Click(object sender, EventArgs e)
    {
        if (!Activator.SelectObject(new DialogArgs
        {
            TaskDescription = "Which Catalogue should the graph depict?"
        }, Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), out var selected)) return;
        _collection.SetSingleCatalogueMode(selected);

        _btnAllCatalogues.Checked = false;
        _btnSingleCatalogue.Checked = true;

        SaveCollectionChanges();
        GenerateChart();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        GenerateChart();
    }

    private void SaveCollectionChanges()
    {
        if (_bLoading)
            return;

        _dashboardControlDatabaseRecord.SaveCollectionState(_collection);
    }

    private void btnShowLabels_CheckStateChanged(object sender, EventArgs e)
    {
        _collection.ShowLabels = _btnShowLabels.Checked;

        chart1.Series[0]["PieLabelStyle"] = _collection.ShowLabels ? "Inside" : "Disabled";
        SaveCollectionChanges();
    }

    private void btnViewDataTable_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    "The following CatalogueItem(s) do not currently have descriptions. Select one to navigate to it"
        }, GetCatalogueItems().Where(ci => string.IsNullOrWhiteSpace(ci.Description)).ToArray(), out var selected))
        {
            var cmd = new ExecuteCommandShow(Activator, selected, 1);
            cmd.Execute();
        }
    }
}