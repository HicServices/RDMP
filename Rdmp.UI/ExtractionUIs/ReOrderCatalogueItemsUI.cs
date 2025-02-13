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
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.ExtractionUIs;

/// <summary>
/// This form allows partial or complete reordering of a datasets columns based on a list of column names
/// pasted into the Desired Order listbox.  All items are cleaned such that the user should be able to paste in
/// a list, the middle section of a SELECT statement or pretty much anything else.
/// 
/// <para>Once a desired order is entered the class will attempt to find the first item in the desired order.  Assuming
/// this item is found then the location of this field becomes the 'insertion' point for reordering and all fields
/// that the user pasted in are reordered into this point.</para>
/// 
/// <para>At any time you can look at the 'New Order' section to see the new order that columns will be in if you accept the
/// reordering.</para>
/// 
/// </summary>
public partial class ReOrderCatalogueItemsUI : ReOrderCatalogueItems_Design
{
    private Catalogue _catalogue;

    //the item in the original order that we want to start reordering at
    private int currentOrderStartReorderAtIndex = -1;

    /// <summary>
    /// This is a collection of all the items found in the desired order and their offset in the desired order relative to the first one
    /// </summary>
    private List<ExtractionInformation> itemsToReOrderAndOffsetRelativeToFirst;

    private List<int> desiredColumnIndexesNotFound;
    private int indexOfStartOfReordingInNewOrderListbox;

    public ReOrderCatalogueItemsUI()
    {
        InitializeComponent();
        splitContainer1.Panel2Collapsed = true;
        AssociatedCollection = RDMPCollection.Catalogue;
        helpIcon1.SetHelpText("Re Order",
            "Instructions: In simple mode you can view your columns and drag and drop them to reorder them.  In Advanced mode you can also Paste (Ctrl + V)  a list of column names into Desired Order (don't worry about trimming commas or table prefixes etc).  The first item in the list should be the point at which you want to start reordering at e.g. CHI or the last record if you want to move columns to the end then paste in the rest of the columns that you want to move after this first item."
        );

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvExtractionInformations, olvColumns,
            new Guid("35946a6e-ebe4-496a-a944-1ddb10b5f8c5"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvExtractionInformations, olvOrder,
            new Guid("d11d4b84-2464-4254-a3cb-b656c55dd0fc"));

        lbDesiredOrder.SelectedIndexChanged += (s, e) => lbDesiredOrder.Refresh();
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _catalogue = databaseObject;

        olvColumns.ImageGetter += s => activator.CoreIconProvider.GetImage(s).ImageToBitmap();
        olvExtractionInformations.RowHeight = 19;
        ((SimpleDropSink)olvExtractionInformations.DropSink).AcceptableLocations = DropTargetLocation.BetweenItems;
        RefreshUIFromDatabase();
    }

    private void RefreshUIFromDatabase()
    {
        olvExtractionInformations.ClearObjects();

        var info = new List<ExtractionInformation>(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
        info.Sort();

        olvExtractionInformations.AddObjects(info.ToArray());
    }

    private void lbDesiredOrder_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
            lock (_oDrawLock)
            {
                var toDelete = lbDesiredOrder.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToArray();

                foreach (var r in toDelete)
                    lbDesiredOrder.Items.RemoveAt(r);

                RecomputeOrderAndHighlight();
            }
        else if (e.KeyCode == Keys.V && e.Control)
            lock (_oDrawLock)
            {
                lbDesiredOrder.Items.AddRange(UsefulStuff
                    .GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()).ToArray());
                RecomputeOrderAndHighlight();
            }
    }

    private void RecomputeOrderAndHighlight()
    {
        WorkOutReOrderVariables();

        WorkOutNewOrderAndAddToNewOrderListbox();

        olvExtractionInformations.Invalidate();
    }

    /// <summary>
    /// Figures out what strings in the users desired order are actually in the extraction and computes
    /// currentOrderStartReorderAtIndex and itemsToReOrderAndOfsetRelativeToFirst which are used for
    /// highlighting and to WorkOutNewOrderAndAddToNewOrderListbox
    /// </summary>
    private void WorkOutReOrderVariables()
    {
        desiredColumnIndexesNotFound = new List<int>();
        currentOrderStartReorderAtIndex = -1;

        //find the first item on the list
        if (lbDesiredOrder.Items.Count == 0)
            return;

        var startReorderingHere = lbDesiredOrder.Items[0].ToString();

        //find the location of the first item in the desired order
        var extractionInformations = olvExtractionInformations.Objects.Cast<ExtractionInformation>().ToArray();

        for (var i = 0; i < extractionInformations.Length; i++)
        {
            var extractionInformation = extractionInformations[i];

            //if either the runtime name or the display (UI) name matches then it is found - may be both of these are the same value sometimes
            //but sometimes people give things wierdo names eh
            if (extractionInformation.GetRuntimeName().ToLower().Equals(startReorderingHere.ToLower()) ||
                extractionInformation.ToString().ToLower().Equals(startReorderingHere.ToLower()))
                currentOrderStartReorderAtIndex = i;
        }

        if (currentOrderStartReorderAtIndex == -1)
            desiredColumnIndexesNotFound.Add(0);

        //find the rest of the items in the desired order
        itemsToReOrderAndOffsetRelativeToFirst = new List<ExtractionInformation>();

        for (var i = 1; i < lbDesiredOrder.Items.Count; i++)
        {
            var bFound = false;

            foreach (ExtractionInformation info in olvExtractionInformations.Objects)
                //if either the runtime name or the display (UI) name matches then it is found - may be both of these are the same value sometimes
                //but sometimes people give things wierdo names eh
                if (info.GetRuntimeName().ToLower().Equals(lbDesiredOrder.Items[i].ToString().ToLower())
                    || info.ToString().ToLower().Equals(lbDesiredOrder.Items[i].ToString().ToLower()))
                {
                    bFound = true;
                    itemsToReOrderAndOffsetRelativeToFirst.Add(info);
                }

            if (!bFound)
                desiredColumnIndexesNotFound.Add(i);
        }
    }

    private void WorkOutNewOrderAndAddToNewOrderListbox()
    {
        lbNewOrder.Items.Clear();

        var extractionInformations = olvExtractionInformations.Objects.Cast<ExtractionInformation>().ToArray();

        //for all the things that appear above the thing the user wants first in his dream order
        for (var i = 0; i < currentOrderStartReorderAtIndex; i++)
        {
            var considerMoving = extractionInformations[i];

            //if it doesnt feature in the users dream list order then move it across
            if (!itemsToReOrderAndOffsetRelativeToFirst.Contains(considerMoving))
                lbNewOrder.Items.Add(considerMoving);
        }

        //could not find the users desired reordering startcolumn
        if (currentOrderStartReorderAtIndex == -1)
            return;

        //move the first one in the users dream order across
        lbNewOrder.Items.Add(extractionInformations[currentOrderStartReorderAtIndex]);
        //record the location of the 'start reordering at' item in the new listbox so we can highlight it in the draw method
        indexOfStartOfReordingInNewOrderListbox = lbNewOrder.Items.Count - 1;

        //move everything in the users dream list
        foreach (var extractionInformation in itemsToReOrderAndOffsetRelativeToFirst)
            lbNewOrder.Items.Add(extractionInformation);

        //move everything that doesnt feature in the users dream list (but that occurred after the first thing they wanted)

        //for all the things that appear above the thing the user wants first in his dream order
        for (var i = currentOrderStartReorderAtIndex + 1; i < extractionInformations.Length; i++)
        {
            var considerMoving = extractionInformations[i];

            //if it doesnt feature in the users dream list order then move it across
            if (!itemsToReOrderAndOffsetRelativeToFirst.Contains(considerMoving))
                lbNewOrder.Items.Add(considerMoving);
        }
    }

    private void lbCurrentOrder_DrawItem(object sender, DrawItemEventArgs e)
    {
        lock (_oDrawLock)
        {
            var listBox = sender as ListBox;

            if (e.Index == -1)
                return;

            /*var extractionInformations = olvCurrentOrder.Objects.Cast<ExtractionInformation>().ToArray();

            if (sender == olvCurrentOrder)
            {
                if (e.Index == currentOrderStartReorderAtIndex)
                    e.Graphics.FillRectangle(new SolidBrush(Color.LawnGreen), e.Bounds);
                else
                    if (itemsToReOrderAndOffsetRelativeToFirst != null && itemsToReOrderAndOffsetRelativeToFirst.Contains(olvCurrentOrder.Items[e.Index] as ExtractionInformation))
                        e.Graphics.FillRectangle(new SolidBrush(Color.Purple), e.Bounds);
                    else
                        e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);
            }
            */
            if (sender == lbNewOrder)
            {
                if (e.Index == indexOfStartOfReordingInNewOrderListbox)
                    e.Graphics.FillRectangle(new SolidBrush(Color.LawnGreen), e.Bounds);
                else if (itemsToReOrderAndOffsetRelativeToFirst != null
                         && e.Index <= itemsToReOrderAndOffsetRelativeToFirst.Count +
                         indexOfStartOfReordingInNewOrderListbox
                         && e.Index > indexOfStartOfReordingInNewOrderListbox)
                    e.Graphics.FillRectangle(new SolidBrush(Color.Purple), e.Bounds);
                else
                    e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);
            }

            e.Graphics.DrawString(listBox.Items[e.Index].ToString(), lbDesiredOrder.Font, new SolidBrush(Color.Black),
                e.Bounds);
        }
    }

    private readonly Lock _oDrawLock = new();


    private void lbDesiredOrder_DrawItem(object sender, DrawItemEventArgs e)
    {
        lock (_oDrawLock)
        {
            if (e.Index == -1)
                return;

            if (lbDesiredOrder.SelectedIndex == e.Index || lbDesiredOrder.SelectedIndices.Contains(e.Index) ||
                desiredColumnIndexesNotFound == null)
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
            else if (desiredColumnIndexesNotFound.Contains(e.Index))
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), e.Bounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(lbDesiredOrder.BackColor), e.Bounds);

            e.Graphics.DrawString(lbDesiredOrder.Items[e.Index] as string, lbDesiredOrder.Font,
                new SolidBrush(lbDesiredOrder.ForeColor), e.Bounds);
        }
    }

    private void btnSaveNewOrder_Click(object sender, EventArgs e)
    {
        if (lbNewOrder.Items.Count == 0)
        {
            MessageBox.Show("You must paste a list of columns into the Desired Order listbox first");
            return;
        }

        for (var i = 0; i < lbNewOrder.Items.Count; i++)
        {
            var info = (ExtractionInformation)lbNewOrder.Items[i];
            info.Order = i + 1;
            info.SaveToDatabase();
        }

        RefreshUIFromDatabase();

        MessageBox.Show("Reorder Applied");
        ClearAdvancedListboxes();
    }

    private void olvExtractionInformations_ModelCanDrop(object sender, ModelDropEventArgs e)
    {
        e.Handled = true;
        e.Effect = DragDropEffects.None;

        if (e.SourceModels.Count != 1)
        {
            e.InfoMessage = "Only drag one object at once";
            return;
        }

        if (e.DropTargetLocation != DropTargetLocation.AboveItem &&
            e.DropTargetLocation != DropTargetLocation.BelowItem) return;

        e.Effect = DragDropEffects.Move;
    }

    private void olvExtractionInformations_ModelDropped(object sender, ModelDropEventArgs e)
    {
        var beingDragged = e.SourceModels[0] as ExtractionInformation;
        var beingDroppedOnto = e.TargetModel as ExtractionInformation;

        var currentOrder = olvExtractionInformations.Objects.Cast<ExtractionInformation>().ToList();

        currentOrder.Remove(beingDragged);

        var idxDrop = currentOrder.IndexOf(beingDroppedOnto);

        if (idxDrop == -1)
            return;

        if (e.DropTargetLocation == DropTargetLocation.AboveItem)
            currentOrder.Insert(idxDrop, beingDragged);
        else if (e.DropTargetLocation == DropTargetLocation.BelowItem)
            currentOrder.Insert(idxDrop + 1, beingDragged);
        else
            throw new NotSupportedException();

        for (var i = 0; i < currentOrder.Count; i++)
        {
            currentOrder[i].Order = i;
            currentOrder[i].SaveToDatabase();
        }

        //actually we changed the childrens display order..
        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_catalogue));
        RefreshUIFromDatabase();
    }

    private void rbSimple_CheckedChanged(object sender, EventArgs e)
    {
        splitContainer1.Panel2Collapsed = rbSimple.Checked;
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        ClearAdvancedListboxes();
    }

    private void ClearAdvancedListboxes()
    {
        lbDesiredOrder.Items.Clear();
        lbNewOrder.Items.Clear();
    }

    private void olvExtractionInformations_ItemActivate(object sender, EventArgs e)
    {
        if (olvExtractionInformations.SelectedObject is IMapsDirectlyToDatabaseTable o)
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(o) { ExpansionDepth = 1 });
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ReOrderCatalogueItems_Design, UserControl>))]
public abstract class ReOrderCatalogueItems_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}