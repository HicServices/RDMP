// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// It is highly likely that you will have columns in different datasets which are conceptually the same (e.g. patient identifier).  Maintaining a central description of this concept is
/// important, it is no use having 10 slightly different descriptions of 'PatientCareNumber' for example.
/// 
/// <para>This dialog appears any time you save a description of a column/transform (CatalogueItem) and there is another column in any of your other datasets which has the same name.  It shows
/// you the other columns that share the same name and lets you view their descriptions and the differences between their descriptions and your new description.  To view the changes
/// select one of the properties you changed on the right listbox (e.g. Description) and then scroll through the objects on the left to view the differences in descriptions.</para>
/// 
/// <para>Next you must decide whether your new description applies to all the other objects too or whether the software made a mistake and actually you want to maintain the unique descriptions
/// (for example it is likely if you have a column EventDate it might have different descriptions in each dataset).</para>
/// 
/// <para>Select either:
/// Cancel - Nothing will be saved and your column description change will be lost
/// No (Save only this one) - Only the original column description you were modifying will be saved
/// Yes (Copy over changes) - The original column and ALL OTHER TICKED columns will all be set to have the same description (that you originally saved).</para>
/// </summary>
public partial class PropagateCatalogueItemChangesToSimilarNamedUI : RDMPForm
{
    private readonly CatalogueItem _catalogueItemBeingSaved;
    private Scintilla previewOldValue;
    private Scintilla previewNewValue;

    public PropagateCatalogueItemChangesToSimilarNamedUI(IActivateItems activator,
        CatalogueItem catalogueItemBeingSaved, out bool shouldDialogBeDisplayed) : base(activator)
    {
        _catalogueItemBeingSaved = catalogueItemBeingSaved;
        InitializeComponent();

        if (VisualStudioDesignMode || catalogueItemBeingSaved == null)
        {
            shouldDialogBeDisplayed = false;
            return;
        }

        olvCatalogueItemName.AspectGetter = CatalogueItemName_AspectGetter;
        olvCatalogueItemState.AspectGetter = CatalogueItemState_AspectGetter;
        olvCatalogueItemName.ImageGetter += ci => activator.CoreIconProvider.GetImage(ci).ImageToBitmap();

        var changedProperties = DetermineChangedProperties(catalogueItemBeingSaved);

        var otherCatalogueItemsThatShareName = GetAllCatalogueItemsSharingNameWith(catalogueItemBeingSaved);

        //if Name changed then they probably don't want to also rename all associated CatalogueItems
        shouldDialogBeDisplayed = !changedProperties.Any(prop => prop.Name.Equals("Name"));

        if (otherCatalogueItemsThatShareName.Length == 0)
            shouldDialogBeDisplayed = false;

        if (!changedProperties.Any())
            shouldDialogBeDisplayed = false;

        if (!shouldDialogBeDisplayed)
            return;

        previewOldValue = new ScintillaTextEditorFactory().Create();
        previewOldValue.ReadOnly = true;

        previewNewValue = new ScintillaTextEditorFactory().Create();
        previewNewValue.ReadOnly = true;

        splitContainer2.Panel1.Controls.Add(previewOldValue);
        splitContainer2.Panel2.Controls.Add(previewNewValue);

        olvProperties.AddObjects(changedProperties);
        if (changedProperties.Count == 1)
        {
            olvProperties.CheckAll();
            olvProperties.SelectedObject = changedProperties[0];
        }

        //Add the objects to the controls and set up default selection
        olvCatalogues.AddObjects(otherCatalogueItemsThatShareName);
        if (otherCatalogueItemsThatShareName.Length == 1)
        {
            olvCatalogues.CheckAll();
            olvCatalogues.SelectedObject = otherCatalogueItemsThatShareName[0];
        }

        olvCatalogues.CellRightClick += olvCatalogues_CellRightClick;

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvCatalogues, olvCatalogueItemName,
            new Guid("c5741da2-07d9-4bfb-952d-8b6df77271bf"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvCatalogues, olvCatalogueItemState,
            new Guid("fd7ad4a8-7448-4fff-8059-3759fe0c4d87"));

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvProperties, olvPropertyName,
            new Guid("b56adceb-2cd5-4f77-9be7-07fb38baad18"));
    }


    private void olvCatalogues_CellRightClick(object sender, CellRightClickEventArgs e)
    {
        if (olvCatalogues.SelectedObject is not CatalogueItem ci)
            return;

        var menu = new RDMPContextMenuStrip(new RDMPContextMenuStripArgs(Activator), ci);
        menu.Show(Cursor.Position);
    }

    private object CatalogueItemName_AspectGetter(object rowObject)
    {
        var ci = rowObject as CatalogueItem;
        return $"{ci.Catalogue.Name}.{ci.Name}";
    }


    private object CatalogueItemState_AspectGetter(object rowobject)
    {
        var pi = olvProperties.Objects.Cast<PropertyInfo>().ToArray();
        if (pi.Length == 1)
        {
            var r = pi[0].GetValue(rowobject);
            if (r == null || r == DBNull.Value || string.IsNullOrWhiteSpace(r.ToString()))
                return "Empty";

            var beingChanged = pi[0].GetValue(_catalogueItemBeingSaved);

            return beingChanged != null && r.Equals(beingChanged) ? "Identical" : (object)"Different";
        }

        return null;
    }

    public static List<PropertyInfo> DetermineChangedProperties(CatalogueItem newVersionInMemory)
    {
        return newVersionInMemory.HasLocalChanges().Differences.Select(d => d.Property).ToList();
    }

    private CatalogueItem[] GetAllCatalogueItemsSharingNameWith(CatalogueItem catalogueItemBeingSaved)
    {
        return Activator.CoreChildProvider.AllCatalogueItems
            .Value.Where(ci =>
                ci.Name.Equals(catalogueItemBeingSaved.Name, StringComparison.CurrentCultureIgnoreCase)
                && ci.ID != catalogueItemBeingSaved.ID)
            .ToArray();
    }

    private void clbCatalogues_SelectedIndexChanged(object sender, EventArgs e)
    {
        displayPreview();
    }

    private void clbChangedProperties_SelectedIndexChanged(object sender, EventArgs e)
    {
        displayPreview();
    }

    public void displayPreview()
    {
        var pi = olvProperties.SelectedObject as PropertyInfo;

        if (pi != null && olvCatalogues.SelectedObject is CatalogueItem ci)
        {
            previewOldValue.ReadOnly = false;
            previewOldValue.Text = Convert.ToString(pi.GetValue(ci, null));
            previewOldValue.ReadOnly = true;

            previewNewValue.ReadOnly = false;
            previewNewValue.Text = Convert.ToString(pi.GetValue(_catalogueItemBeingSaved, null));
            previewNewValue.ReadOnly = true;

            highlightDifferencesBetweenPreviewPanes();
        }
    }

    private void cbSelectAllCatalogues_CheckedChanged(object sender, EventArgs e)
    {
        if (cbSelectAllCatalogues.Checked)
            olvCatalogues.CheckAll();
        else
            olvCatalogues.UncheckAll();
    }

    private void cbSelectAllFields_CheckedChanged(object sender, EventArgs e)
    {
        if (cbSelectAllFields.Checked)
            olvProperties.CheckAll();
        else
            olvProperties.UncheckAll();
    }

    private void highlightDifferencesBetweenPreviewPanes()
    {
        var sOld = previewOldValue.Text;
        var sNew = previewNewValue.Text;

        ScintillaLineHighlightingHelper.ClearAll(previewNewValue);
        ScintillaLineHighlightingHelper.ClearAll(previewOldValue);

        foreach (var item in Diff.DiffText(sOld, sNew))
        {
            for (var i = item.StartA; i < item.StartA + item.deletedA; i++)
                ScintillaLineHighlightingHelper.HighlightLine(previewOldValue, i, Color.Pink);

            //if it is single line change
            for (var i = item.StartB; i < item.StartB + item.insertedB; i++)
                ScintillaLineHighlightingHelper.HighlightLine(previewNewValue, i, Color.LawnGreen);
        }
    }

    //yes = do save and do propogate
    private void btnYes_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Yes;


        foreach (CatalogueItem ci in olvCatalogues.CheckedObjects)
        {
            foreach (PropertyInfo p in olvProperties.CheckedObjects)
                p.SetValue(ci, p.GetValue(_catalogueItemBeingSaved, null), null);

            ci.SaveToDatabase();
        }

        Close();
    }

    //no = do save but don't propagate
    private void btnNo_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.No;
        Close();
    }

    //cancel = don't save this and don't propagate
    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void olv_ItemActivate(object sender, EventArgs e)
    {
        var olv = (ObjectListView)sender;
        if (olv.SelectedObject != null)
            olv.ToggleCheckObject(olv.SelectedObject);
    }
}