// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Overview;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.ExtractionUIs;

/// <summary>
/// Allows you to view the Extraction SQL that is built by the QueryBuilder when extracting or running data quality engine against a dataset (Catalogue).  Includes options for
/// you to display only Core extraction fields or also supplemental / special approval.
/// 
/// <para>If you have an ExtractionFilters configured on your Catalogue then you can tick them to view their SQL implementation.  Because these are master filters and this dialog
/// is for previewing only, no AND/OR container trees are included in the WHERE logic (See ExtractionFilterUI for more info about filters).</para>
/// 
/// <para>If for some reason you see an error instead of your extraction SQL then read the description and take the steps it suggests (e.g. if it is complaining about not knowing
/// how to JOIN two tables then configure an appropriate JoinInfo - See JoinConfiguration). </para>
/// </summary>
public partial class ViewCatalogueOverviewUI : ViewExtractionSql_Design
{
    private Catalogue _catalogue;
    private OverviewModel _overview;
    private List<CatalogueItem> _dateColumns;
    private static readonly string[] frequencies = new[] { "Day", "Month", "Year" };

    public ViewCatalogueOverviewUI()
    {
        InitializeComponent();
    }

    private void UpdateCatalogueData()
    {
        lblName.Text = _catalogue.Name;
        lblDescription.Text = _catalogue.Description;

        var latestDataLoad = _overview.GetMostRecentDataLoad();
        if (latestDataLoad != null)
        {
            lblLastDataLoad.Text = latestDataLoad.Rows[0][3].ToString();
        }
        else
        {
            lblLastDataLoad.Text = "No Successful DataLoads";
        }
        var extractions = _overview.GetExtractions();
        if (extractions.Any())
        {
            var latestExtractionDate = extractions.AsEnumerable().Select(r => r.DateOfExtraction).Distinct().Max();
            lblLatestExtraction.Text = latestExtractionDate.ToString();
        }
        else
        {
            lblLatestExtraction.Text = "Catalogue has not been extracted";
        }
        var syntaxHelper = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).GetQuerySyntaxHelper();
        var dateTypeString = syntaxHelper.TypeTranslater.GetSQLDBTypeForCSharpType(new TypeGuesser.DatabaseTypeRequest(typeof(DateTime)));

        _dateColumns = _catalogue.CatalogueItems.Where(ci => ci.ColumnInfo.Data_type == dateTypeString).ToList();
        cbTimeColumns.Items.Clear();
        cbTimeColumns.Items.AddRange(_dateColumns.ToArray());
        var pks = _dateColumns.Where(ci => ci.ColumnInfo.IsPrimaryKey).ToList();

        DataTable dt = new();
        if (pks.Any())
        {
            dt = _overview.GetCountsByDatePeriod(pks[0].ColumnInfo,cbFrequency.SelectedItem.ToString());
            cbTimeColumns.SelectedIndex = _dateColumns.IndexOf(pks[0]);
        }
        else if (_dateColumns.Any())
        {
            dt = _overview.GetCountsByDatePeriod(_dateColumns[0].ColumnInfo, cbFrequency.SelectedItem.ToString());
            cbTimeColumns.SelectedIndex = 0;
        }
        lblRecords.Text = _overview.GetNumberOfRecords().ToString();
        var dates = _overview.GetStartEndDates(_dateColumns[cbTimeColumns.SelectedIndex].ColumnInfo);
        lblDateRange.Text = $"{dates.Item1} - {dates.Item2}";

    }

    private void cbFrequency_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCatalogueData();
    }

    private int OnTabChange(int tabIndex)
    {
        if(tabIndex == 0)
        {
            tbMainWhere.Visible = true;
            lblWhere.Visible = true;
            cbTimeColumns.Visible = true;
            lblTime.Visible = true;
        }
        else
        {
            tbMainWhere.Visible = false;
            lblWhere.Visible = false;
            cbTimeColumns.Visible = false;
            lblTime.Visible = false;
        }
        return 1;
    }

    private void cbTimeColumns_SelectedIndexChanged(object sender, EventArgs e)
    {
        var dt = _overview.GetCountsByDatePeriod(_dateColumns[cbTimeColumns.SelectedIndex].ColumnInfo, cbFrequency.SelectedItem.ToString(),tbMainWhere.Text);
        areaChart1.GenerateChart(dt, "Records per Month");
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        _overview = new OverviewModel(activator, _catalogue);
        cbFrequency.Items.Clear();
        cbFrequency.Items.AddRange(frequencies);
        cbFrequency.SelectedIndex = 1;
        UpdateCatalogueData();
    }

    public override string GetTabName() => $"{_catalogue.Name} Overview";

    private void lblLastDataLoad_Click(object sender, EventArgs e)
    {

    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void lblDescription_Click(object sender, EventArgs e)
    {

    }

    private void lblLatestExtraction_Click(object sender, EventArgs e)
    {

    }

    private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }

    private void areaChart1_Load(object sender, EventArgs e)
    {

    }

    private void tbMainWhere_TextChanged(object sender, EventArgs e)
    {
        //var text = tbMainWhere.Text;
        var dt = _overview.GetCountsByDatePeriod(_dateColumns[cbTimeColumns.SelectedIndex].ColumnInfo, cbFrequency.SelectedItem.ToString(),tbMainWhere.Text);
        areaChart1.GenerateChart(dt, "Records per Month");
    }
}
[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewCatalogueOverview_Design, UserControl>))]
public abstract class ViewCatalogueOverview_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}