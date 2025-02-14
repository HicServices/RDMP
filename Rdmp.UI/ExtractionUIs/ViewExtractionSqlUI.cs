// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
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
public partial class ViewExtractionSqlUI : ViewExtractionSql_Design
{
    private Catalogue _catalogue;

    private ToolStripButton rbCore = new("Core");
    private ToolStripButton rbSupplemental = new("Supplemental") { Checked = true };
    private ToolStripButton rbSpecialApproval = new("Special Approval");
    private ToolStripButton rbInternal = new("Internal");
    private ToolStripButton btnRun = new("Run", CatalogueIcons.ExecuteArrow.ImageToBitmap());

    private Scintilla QueryPreview;

    public ViewExtractionSqlUI()
    {
        InitializeComponent();

        #region Query Editor setup

        QueryPreview = new ScintillaTextEditorFactory().Create();
        QueryPreview.ReadOnly = true;

        scMainLeftRightSplit.Panel2.Controls.Add(QueryPreview);
        bLoading = false;

        #endregion

        AssociatedCollection = RDMPCollection.Catalogue;

        olvColumn1.ImageGetter += ImageGetter;
        olvFilterName.ImageGetter += ImageGetter;

        rbCore.Click += rb_Click;
        rbSupplemental.Click += rb_Click;
        rbSpecialApproval.Click += rb_Click;
        rbInternal.Click += rb_Click;
        btnRun.Click += BtnRun_Click;
    }

    private void rb_Click(object sender, EventArgs e)
    {
        //treat as radio button
        foreach (var item in new[] { rbCore, rbSupplemental, rbSpecialApproval, rbInternal })
            item.Checked = item == sender;

        RefreshUIFromDatabase();
    }

    private Bitmap ImageGetter(object rowObject) => Activator.CoreIconProvider.GetImage(rowObject).ImageToBitmap();

    private bool bLoading;


    private void RefreshUIFromDatabase()
    {
        CommonFunctionality.ResetChecks();

        try
        {
            if (bLoading)
                return;

            //only allow reordering when all are visible or only internal are visible otherwise user could select core only and do a reorder leaving supplemental columns as freaky orphans all down at the bottom fo the SQL!
            bLoading = true;

            var extractionInformations = GetSelectedExtractionInformations();

            //add the available filters
            SetupAvailableFilters(extractionInformations);

            //generate SQL -- only make it readonly after setting the .Text otherwise it ignores the .Text setting even though it is programatical
            QueryPreview.ReadOnly = false;


            var collection = GetCollection(extractionInformations);

            QueryPreview.Text = collection.GetSql();
            CommonFunctionality.ScintillaGoRed(QueryPreview, false);
            QueryPreview.ReadOnly = true;
        }
        catch (Exception ex)
        {
            CommonFunctionality.ScintillaGoRed(QueryPreview, ex);
            CommonFunctionality.Fatal(ex.Message, ex);
        }
        finally
        {
            bLoading = false;
        }
    }

    private void SetupAvailableFilters(List<ExtractionInformation> extractionInformations)
    {
        var filters = extractionInformations.SelectMany(ei => ei.ExtractionFilters).ToArray();

        //remove deleted ones
        if (olvFilters.Objects != null)
            foreach (var f in olvFilters.Objects.Cast<ExtractionFilter>().Except(filters).ToArray())
                olvFilters.RemoveObject(f);

        //add new ones
        foreach (var f in filters)
            if (olvFilters.IndexOf(f) == -1)
                olvFilters.AddObject(f);
    }

    private List<ExtractionInformation> GetSelectedExtractionInformations()
    {
        var extractionInformations = new List<ExtractionInformation>();

        if (rbInternal.Checked)
        {
            extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Internal));
        }
        else
        {
            //always add the project specific ones
            extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific));
            extractionInformations.AddRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Core));

            if (rbSupplemental.Checked || rbSpecialApproval.Checked)
                extractionInformations.AddRange(
                    _catalogue.GetAllExtractionInformation(ExtractionCategory.Supplemental));

            if (rbSpecialApproval.Checked)
                extractionInformations.AddRange(
                    _catalogue.GetAllExtractionInformation(ExtractionCategory.SpecialApprovalRequired));
        }

        //sort by Default Order
        extractionInformations.Sort();

        //add to listbox
        olvExtractionInformations.ClearObjects();
        olvExtractionInformations.AddObjects(extractionInformations.ToArray());

        return extractionInformations;
    }

    private ViewCatalogueDataCollection GetCollection(List<ExtractionInformation> extractionInformations)
    {
        var collection = new ViewCatalogueDataCollection(_catalogue);
        collection.DatabaseObjects.AddRange(olvFilters.CheckedObjects.OfType<IFilter>());
        collection.DatabaseObjects.AddRange(extractionInformations);

        return collection;
    }

    private void BtnRun_Click(object sender, EventArgs e)
    {
        Activator.ShowData(GetCollection(GetSelectedExtractionInformations()));
    }


    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        RefreshUIFromDatabase();

        rbCore.Image = CatalogueIcons.ExtractionInformation.ImageToBitmap();
        rbSupplemental.Image = CatalogueIcons.ExtractionInformation_Supplemental.ImageToBitmap();
        rbSpecialApproval.Image = CatalogueIcons.ExtractionInformation_SpecialApproval.ImageToBitmap();
        rbInternal.Image = activator.CoreIconProvider
            .GetImage(SixLabors.ImageSharp.Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_SpecialApproval),
                OverlayKind.Internal).ImageToBitmap();

        CommonFunctionality.Add(rbCore);
        CommonFunctionality.Add(rbSupplemental);
        CommonFunctionality.Add(rbSpecialApproval);
        CommonFunctionality.Add(rbInternal);
        CommonFunctionality.Add(btnRun);

        CommonFunctionality.AddToMenu(new ExecuteCommandReOrderColumns(Activator, _catalogue));
    }

    public override string GetTabName() => $"{base.GetTabName()}(SQL)";

    private void olv_ItemActivate(object sender, EventArgs e)
    {
        if (((ObjectListView)sender).SelectedObject is IMapsDirectlyToDatabaseTable o)
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(o) { ExpansionDepth = 1 });
    }

    private void olvFilters_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        RefreshUIFromDatabase();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewExtractionSql_Design, UserControl>))]
public abstract class ViewExtractionSql_Design : RDMPSingleDatabaseObjectControl<Catalogue>;