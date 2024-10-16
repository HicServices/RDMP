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
            dt = _overview.GetCountsByMonth(pks[0].ColumnInfo);
            cbTimeColumns.SelectedIndex = _dateColumns.IndexOf(pks[0]);
        }
        else if (_dateColumns.Any())
        {
            dt = _overview.GetCountsByMonth(_dateColumns[0].ColumnInfo);
            cbTimeColumns.SelectedIndex = 0;
        }

    }

    private void cbTimeColumns_SelectedIndexChanged(object sender, EventArgs e)
    {
        var dt = _overview.GetCountsByMonth(_dateColumns[cbTimeColumns.SelectedIndex].ColumnInfo,tbMainWhere.Text);
        areaChart1.GenerateChart(dt, "Records per Month");
    }

    private Bitmap ImageGetter(object rowObject) => Activator.CoreIconProvider.GetImage(rowObject).ImageToBitmap();

    private void RefreshUIFromDatabase()
    {
        //CommonFunctionality.ResetChecks();

        //try
        //{
        //    if (bLoading)
        //        return;

        //    //only allow reordering when all are visible or only internal are visible otherwise user could select core only and do a reorder leaving supplemental columns as freaky orphans all down at the bottom fo the SQL!
        //    bLoading = true;

        //    var extractionInformations = GetSelectedExtractionInformations();

        //    //add the available filters
        //    SetupAvailableFilters(extractionInformations);

        //    //generate SQL -- only make it readonly after setting the .Text otherwise it ignores the .Text setting even though it is programatical
        //    QueryPreview.ReadOnly = false;


        //    var collection = GetCollection(extractionInformations);

        //    QueryPreview.Text = collection.GetSql();
        //    CommonFunctionality.ScintillaGoRed(QueryPreview, false);
        //    QueryPreview.ReadOnly = true;
        //}
        //catch (Exception ex)
        //{
        //    CommonFunctionality.ScintillaGoRed(QueryPreview, ex);
        //    CommonFunctionality.Fatal(ex.Message, ex);
        //}
        //finally
        //{
        //    bLoading = false;
        //}
    }


    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        _overview = new OverviewModel(activator, _catalogue);
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
        var dt = _overview.GetCountsByMonth(_dateColumns[cbTimeColumns.SelectedIndex].ColumnInfo, tbMainWhere.Text);
        areaChart1.GenerateChart(dt, "Records per Month");
    }
}
[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewCatalogueOverview_Design, UserControl>))]
public abstract class ViewCatalogueOverview_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}