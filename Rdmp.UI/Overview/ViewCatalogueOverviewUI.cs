// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Overview;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.CatalogueSummary.DataQualityReporting;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Overview;

/// <summary>
/// Allows you to view summary data of a catalogue
/// </summary>
public partial class ViewCatalogueOverviewUI : ViewCatalogueOverviewUI_Design
{
    private Catalogue _catalogue;
    private OverviewModel _overview;
    private AreaChartUI areaChartUI;
    public ViewCatalogueOverviewUI()
    {
        InitializeComponent();
        areaChartUI = new AreaChartUI(OnTabChange);
        areaChartUI.Location = new System.Drawing.Point(4, 0);
        areaChartUI.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        areaChartUI.Name = "areaChart1";
        areaChartUI.Size = new System.Drawing.Size(1075, 400);
        areaChartUI.TabIndex = 0;
        splitContainer1.Panel2.Controls.Add(areaChartUI);
    }

    private void UpdateCatalogueData()
    {

        if (!_overview.HasDQEEvaluation())
        {
            areaChartUI.Visible = false;
            lblNoDQE.Visible = true;
        }
        else
        {
            areaChartUI.Visible = true;
            lblNoDQE.Visible = false;
        }
        lblName.Text = _catalogue.Name;
        lblDescription.Text = _catalogue.Description;

        var latestDataLoad = _overview.GetLatestDataLoad();
        lblLastDataLoad.Text = latestDataLoad ?? "No Successful Data Loads";
        var extraction = _overview.GetLatestExtraction();
        lblLatestExtraction.Text = extraction ?? "Catalogue has not been extracted";
        lblRecords.Text = $"{_overview.GetNumberOfRecords().ToString()} Records";
        var dates = _overview.GetStartEndDates();
        var startDate = dates.Item1 != null ? ((DateTime)dates.Item1).ToString("dd/MM/yyyy") : null;
        var endDate = dates.Item2 != null ? ((DateTime)dates.Item2).ToString("dd/MM/yyyy") : null;
        if (startDate == null && endDate == null)
        {
            lblDateRange.Text = "No Dates Found";
        }
        else
        {
            lblDateRange.Text = $"{startDate} - {endDate}";
        }

        areaChartUI.GenerateChart(_overview.GetTableData(), $"Records per month");

        var syntaxHelper = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false)?.GetQuerySyntaxHelper();
        var dateTypeString = syntaxHelper.TypeTranslater.GetSQLDBTypeForCSharpType(new TypeGuesser.DatabaseTypeRequest(typeof(DateTime)));
        var dateColumns = _catalogue.CatalogueItems.Where(ci => ci.ColumnInfo.Data_type == dateTypeString).ToList();

    }

    private int OnTabChange(int tabIndex)
    {
        return 1;
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        _overview = new OverviewModel(activator, _catalogue);
        UpdateCatalogueData();
    }

    public override string GetTabName() => $"{_catalogue.Name} Overview";

    private void btnSettings_Click(object sender, EventArgs e)
    {
        Activator.Activate<CatalogueUI, Catalogue>(_catalogue);
    }

    private void btnGenerate_Click(object sender, EventArgs e)
    {
        //var dateColumnID = ((CatalogueItem)cbTimeColumns.SelectedItem).ID;
        //Task.Run(() =>
        //{
        //    _overview.Generate(dateColumnID);
        //}).ContinueWith((task) =>
        //{
        //    UpdateCatalogueData();
        //}, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void lblNoDQE_Click(object sender, EventArgs e)
    {

    }
}
[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewCatalogueOverviewUI_Design, UserControl>))]
public abstract class ViewCatalogueOverviewUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}