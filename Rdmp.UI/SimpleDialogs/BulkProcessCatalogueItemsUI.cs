// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Sometimes you will be called upon to host datasets that are a mile wide (e.g. 200 columns) from which researchers only ever receive/care about 10 or 20.  In this case it can be
/// very useful to be able to bulk process CatalogueItem/ColumnInfo relationships and create/delete ExtractionInformation on mass.  This dialog lets you do that for a given Catalogue
/// (dataset).
/// 
/// <para>The starting point is to choose which CatalogueItems are to be bulk processed (Apply Transform To).  Either 'All CatalogueItems' or 'Only those matching paste list'.  If you choose
/// to paste in a list this is done in the left hand listbox.  The window is very flexible about what you can paste in such that you can for example 'Script Select Top 1000' in Microsoft
/// Sql Management Studio and paste the entire query in and it will work out the columns (it looks for the last bit of text on each line.</para>
/// 
/// <para>Once you have configured the bulk process target you can choose what operation to do.  These include:</para>
/// 
/// <para>Making all fields Extractable (with the given ExtractionCategory e.g. Core / Supplemental etc)</para>
/// 
/// <para>Make all fields Unextractable (Delete Extraction Information)</para>
/// 
/// <para>Delete all underlying ColumnInfos (useful if you are trying to migrate your descriptive metadata to a new underlying table in your database e.g. MyDb.Biochemistry to
/// MyDb.NewBiochemistry without losing CatalogueItem column descriptions and validation rules etc).</para>
/// 
/// <para>Guess New Associated Columns from a given TableInfo (stage 2 in the above example), which will try to match up descriptive CatalogueItems by name to a new underlying TableInfo</para>
/// 
/// <para> Delete All CatalogueItems (If you really want to nuke the lot of them!) </para>
/// </summary>
public partial class BulkProcessCatalogueItemsUI : BulkProcessCatalogueItems_Design
{
    private Catalogue _catalogue;

    public BulkProcessCatalogueItemsUI()
    {
        InitializeComponent();

        olvName.ImageGetter += ImageGetter;

        ddExtractionCategory.DataSource = Enum.GetValues(typeof(ExtractionCategory));
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _catalogue = databaseObject;

        RefreshUIFromDatabase();
    }

    private void RefreshUIFromDatabase()
    {
        _catalogue.ClearAllInjections();

        olvCatalogueItems.ClearObjects();
        olvCatalogueItems.AddObjects(_catalogue.CatalogueItems);

        cbTableInfos.Items.Clear();
        cbTableInfos.Items.AddRange(_catalogue.GetTableInfoList(true));
    }

    private Bitmap ImageGetter(object rowObject) => Activator.CoreIconProvider.GetImage(rowObject).ImageToBitmap();

    private void groupBox2_Enter(object sender, EventArgs e)
    {
    }

    private void lbPastedColumns_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.V when e.Control:
                lbPastedColumns.Items.AddRange(
                    UsefulStuff.GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()).ToArray());
                UpdateFilter();
                break;
            case Keys.Delete when lbPastedColumns.SelectedItem != null:
                lbPastedColumns.Items.Remove(lbPastedColumns.SelectedItem);
                UpdateFilter();
                break;
        }
    }

    private void olvCatalogueItems_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.C when e.Control:
                StringBuilder sb = new StringBuilder();
                foreach (var item in olvCatalogueItems.SelectedItems.Cast<OLVListItem>())
                {
                    sb.AppendLine(item.RowObject.ToString());
                }

                Clipboard.SetDataObject(sb.ToString());
                break;

        }
    }

    private void btnApplyTransform_Click(object sender, EventArgs e)
    {
        if (rbDelete.Checked)
            if (MessageBox.Show("Are you sure you want to delete?", "Confirm delete", MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

        ColumnInfo[] guessPoolColumnInfo = null;

        if (rbGuessNewAssociatedColumns.Checked)
        {
            if (cbTableInfos.SelectedItem is not TableInfo tableInfo)
            {
                MessageBox.Show("You must select a TableInfo from the dropdown first");
                return;
            }

            guessPoolColumnInfo = tableInfo.ColumnInfos.ToArray();
        }


        var deleteCount = 0;
        var countExtractionInformationsCreated = 0;
        var countOfColumnInfoAssociationsCreated = 0;

        foreach (CatalogueItem catalogueItem in olvCatalogueItems.Objects)
            if (ShouldTransformCatalogueItem(catalogueItem))
            {
                //bulk operation is delete
                if (rbDelete.Checked)
                {
                    catalogueItem.DeleteInDatabase();
                    deleteCount++;
                }

                //delete relationship between columnInfo and CatalogueItem (IMPORTANT: this does not delete either the ColumnInfo - which could be used by other Catalogues or the CatalogueItem)
                if (rbDeleteAssociatedColumnInfos.Checked)
                    if (catalogueItem.ColumnInfo_ID != null)
                    {
                        deleteCount++;
                        catalogueItem.SetColumnInfo(null);
                    }

                //delete extraction information only, this leaves the underlying relationship between the columnInfo and the CatalogueItem (which must exist in the first place before ExtractionInformation could have been configured) intact
                if (rbDeleteExtrctionInformation.Checked)
                    if (catalogueItem.ExtractionInformation != null)
                    {
                        catalogueItem.ExtractionInformation.DeleteInDatabase();
                        deleteCount++;
                    }

                //user wants to guess ColumnInfo associations between the supplied catalogue and underlying table (and the column doesnt have any existing ones already
                if (rbGuessNewAssociatedColumns.Checked && catalogueItem.ColumnInfo_ID == null)
                {
                    var guesses = catalogueItem.GuessAssociatedColumn(guessPoolColumnInfo).ToArray();

                    //exact matches are straight up accepted
                    if (guesses.Length == 1)
                    {
                        catalogueItem.SetColumnInfo(guesses[0]);
                        countOfColumnInfoAssociationsCreated++;
                    }
                    else
                    {
                        //multiple matches so ask the user what one he wants
                        foreach (var guess in guesses.Where(guess => Activator.YesNo(
                                     $"Found multiple matches, approve match?:{Environment.NewLine}{catalogueItem.Name}{Environment.NewLine}{guess}",
                                     "Multiple matched guesses")))
                        {
                            catalogueItem.SetColumnInfo(guess);
                            countOfColumnInfoAssociationsCreated++;
                            break;
                        }
                    }
                }

                //user wants to mark existing associated columns as extractable (will be created with the default SELECT transformation which is verbatim, no changes)
                if (rbMarkExtractable.Checked)
                {
                    //get the associated columns
                    var col = catalogueItem.ColumnInfo;

                    //do not try to mark missing column info as extractable
                    if (col == null)
                        continue;

                    //column already has ExtractionInformation configured for it so ignore it
                    if (catalogueItem.ExtractionInformation != null)
                    {
                        //unless user wants to do reckless recategorisation
                        if (cbRecategorise.Checked)
                        {
                            var ei = catalogueItem.ExtractionInformation;
                            ei.ExtractionCategory = (ExtractionCategory)ddExtractionCategory.SelectedItem;
                            ei.SaveToDatabase();
                        }

                        continue;
                    }

                    //we got to here so we have a legit 1 column info to cataitem we can enable for extraction
                    var created = new ExtractionInformation((CatalogueRepository)catalogueItem.Repository,
                        catalogueItem, col, null);

                    if (ddExtractionCategory.SelectedItem != null)
                    {
                        created.ExtractionCategory = (ExtractionCategory)ddExtractionCategory.SelectedItem;
                        created.SaveToDatabase();
                    }

                    countExtractionInformationsCreated++;
                }
            }

        var message = "";

        if (deleteCount != 0)
            message += $"Performed {deleteCount} delete operations{Environment.NewLine}";

        if (countExtractionInformationsCreated != 0)
            message += $"Created  {countExtractionInformationsCreated} ExtractionInformations{Environment.NewLine}";

        if (countOfColumnInfoAssociationsCreated != 0)
            message +=
                $"Created  {countOfColumnInfoAssociationsCreated} associations between CatalogueItems and ColumnInfos{Environment.NewLine}";

        if (!string.IsNullOrWhiteSpace(message))
            MessageBox.Show(message);

        Publish(_catalogue);

        RefreshUIFromDatabase();
    }


    private bool ShouldTransformCatalogueItem(CatalogueItem catalogueItem) =>
        olvCatalogueItems.FilteredObjects.Cast<CatalogueItem>().Contains(catalogueItem);

    private void btnClear_Click(object sender, EventArgs e)
    {
        lbPastedColumns.Items.Clear();
        UpdateFilter();
    }

    private void ddExtractionCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        rbMarkExtractable.Checked = true;
    }

    private void cbTableInfos_SelectedIndexChanged(object sender, EventArgs e)
    {
        rbGuessNewAssociatedColumns.Checked = true;
    }

    private void rbMarkExtractable_CheckedChanged(object sender, EventArgs e)
    {
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        UpdateFilter();
    }

    private void UpdateFilter()
    {
        var filters = new List<IModelFilter>();
        var orFilters = new List<IModelFilter>();

        foreach (var item in lbPastedColumns.Items)
        {
            var filter = new TextMatchFilter(olvCatalogueItems, item.ToString(),
                StringComparison.CurrentCultureIgnoreCase);

            if (cbExactMatch.Checked)
            {
                filter.RegexOptions = RegexOptions.IgnoreCase;
                filter.RegexStrings = new List<string> { $"^{item}$" };
            }

            orFilters.Add(filter);
        }


        filters.Add(new TextMatchFilter(olvCatalogueItems, tbFilter.Text, StringComparison.CurrentCultureIgnoreCase));

        if (orFilters.Any())
            filters.Add(new CompositeAnyFilter(orFilters));

        olvCatalogueItems.ModelFilter = new CompositeAllFilter(filters);
        olvCatalogueItems.UseFiltering = true;
    }

    private void cbExactMatch_CheckedChanged(object sender, EventArgs e)
    {
        UpdateFilter();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<BulkProcessCatalogueItems_Design, UserControl>))]
public class BulkProcessCatalogueItems_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}