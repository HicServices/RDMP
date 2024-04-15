// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// One big potential sources of data error in clinical datasets is duplication.  In the worst examples this is exact 100% duplication! for example a data provider loads the same data
/// twice, a data entry clerk hits the submit button twice in a poorly written piece of clinical software etc.  The RDMP attempts to eliminate/reduce the potential for duplication by
/// requiring that data loaded through the Data Load Engine (DLE) require that all tables being loaded have a Primary Key which comes from the source data (no autonums!).
/// 
/// <para>Because primary keys cannot contain NULL values you are forced to create sensible primary keys (for example a Hospital Admissions dataset might have a primary key
/// 'PatientIdentifier' and 'AdmissionDateTime').  By putting a primary key on the dataset we ensure that there cannot be duplicate data load replication (loading same record twice)
///   and also ensure that there cannot be unlinkable records in the database (records where no 'Patient Identifier' exists or when we don't know what date the admission was on).</para>
/// 
/// <para>When primary key collisions occur in a data load it becomes necessary to evaluate the cause (Done by evaluating RAW - see UserManual.md Load Bubbles).  For example we might
/// determine that the data provider is sending us 2 records for the same patient on the same day, the records are identical except for a field 'DataAge'.  Rather than adding this
/// to the primary key it would make sense instead to discard the older record on load.</para>
/// 
/// <para>This dialog (in combination with PrimaryKeyCollisionResolverMutilation - See UserManual.md) lets you delete records out of RAW such that the remaining data matches the datasets
///  primary key (obviously this is incredibly dangerous!).  This is done by applying a column order (with a direction for each column).  The dataset is subsetted by primary key with
/// each set ordered by the resolution order of the columns and the top record taken.</para>
/// 
/// <para>In the above example we would put 'DataAge' as the first column in the resolution order and set it to descending (prefer records with a larger date i.e. newer records).  Direction
/// is obvious in the case of dates/numbers (ascending = prefer the lowest, descending = prefer the highest) but in the case of strings the length of the string is used with (DBNull
/// being 0 length).</para>
/// 
/// <para>Only use PrimaryKeyCollisionResolverMutilation (and this dialog) if you are CERTAIN you have the right primary key for the data/your researchers.</para>
/// </summary>
public partial class ConfigurePrimaryKeyCollisionResolverUI : RDMPForm
{
    private readonly TableInfo _table;

    private ScintillaNET.Scintilla QueryEditor;

    public ConfigurePrimaryKeyCollisionResolverUI(TableInfo table, IActivateItems activator) : base(activator)
    {
        _table = table;
        InitializeComponent();

        if (VisualStudioDesignMode ||
            table == null) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        QueryEditor = new ScintillaTextEditorFactory().Create();
        QueryEditor.ReadOnly = false;

        splitContainer1.Panel2.Controls.Add(QueryEditor);

        RefreshUIFromDatabase();
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void RefreshUIFromDatabase()
    {
        lbPrimaryKeys.Items.Clear();
        lbConflictResolutionColumns.Items.Clear();

        foreach (var pkCol in _table.ColumnInfos.Where(col => col.IsPrimaryKey))
        {
            //primary keys are not used to resolve duplication of primary key values (obviously!)
            if (pkCol.DuplicateRecordResolutionOrder != null)
            {
                //unset any that have accidentally gained an order e.g. if user set an order then made a new column a PK
                pkCol.DuplicateRecordResolutionOrder = null;
                pkCol.SaveToDatabase();
            }


            lbPrimaryKeys.Items.Add(pkCol);
        }

        var resolvers = new List<IResolveDuplication>();
        resolvers.AddRange(_table.ColumnInfos.Where(col => !col.IsPrimaryKey));
        resolvers.AddRange(_table.PreLoadDiscardedColumns);

        //if there is no order yet
        if (resolvers.All(r => r.DuplicateRecordResolutionOrder == null))
            for (var i = 0; i < resolvers.Count; i++)
            {
                //set one up
                resolvers[i].DuplicateRecordResolutionOrder = i;
                resolvers[i].SaveToDatabase();
            }


        foreach (var resolver in resolvers.OrderBy(o => o.DuplicateRecordResolutionOrder).ToArray())
            //if it starts with hic_
            if (SpecialFieldNames.IsHicPrefixed(resolver))
            {
                //do not use it for duplication resolution
                resolver.DuplicateRecordResolutionOrder = null;
                resolver.DuplicateRecordResolutionIsAscending = false; //default to descending
                resolver.SaveToDatabase();
                resolvers.Remove(resolver);
            }

        foreach (var resolver in resolvers.OrderBy(c => c.DuplicateRecordResolutionOrder))
            lbConflictResolutionColumns.Items.Add(resolver);

        QueryEditor.ReadOnly = false;

        try
        {
            //this is used only to generate the SQL preview of how to resolve primary key collisions so no username/password is required - hence the null,null
            var resolver = new PrimaryKeyCollisionResolver(_table);
            QueryEditor.Text = resolver.GenerateSQL();
            CommonFunctionality.ScintillaGoRed(QueryEditor, false);
        }
        catch (Exception e)
        {
            CommonFunctionality.ScintillaGoRed(QueryEditor, e);
        }

        QueryEditor.ReadOnly = true;
    }

    #region Drag and Drop reordering

    private void listBox1_MouseDown(object sender, MouseEventArgs e)
    {
        if (lbConflictResolutionColumns.SelectedItem == null) return;

        if (e.Button == MouseButtons.Left)
            lbConflictResolutionColumns.DoDragDrop(lbConflictResolutionColumns.SelectedItem,
                DragDropEffects.Move);
    }

    private Point draggingOldLeftPoint;
    private Point draggingOldRightPoint;

    private void listBox1_DragOver(object sender, DragEventArgs e)
    {
        e.Effect = DragDropEffects.Move;
        var idxHoverOver =
            lbConflictResolutionColumns.IndexFromPoint(lbConflictResolutionColumns.PointToClient(new Point(e.X, e.Y)));

        var g = lbConflictResolutionColumns.CreateGraphics();

        var top = lbConflictResolutionColumns.Font.Height * idxHoverOver;

        top += lbConflictResolutionColumns.AutoScrollOffset.Y;

        //this seems to count up the number of items that have been skipped rather than the pixels... weird
        var barpos = GetScrollPos(lbConflictResolutionColumns.Handle, Orientation.Vertical);
        barpos *= lbConflictResolutionColumns.Font.Height;
        top -= barpos;


        //calculate where we should be drawing our horizontal line
        var left = new Point(0, top);
        var right = new Point(lbConflictResolutionColumns.Width, top);

        //draw over the old one in the background colour (in case it has moved) - we don't want to leave trails
        g.DrawLine(new Pen(lbConflictResolutionColumns.BackColor, 2), draggingOldLeftPoint,
            draggingOldRightPoint);
        g.DrawLine(new Pen(Color.Black, 2), left, right);

        draggingOldLeftPoint = left;
        draggingOldRightPoint = right;
    }

    private void listBox1_DragDrop(object sender, DragEventArgs e)
    {
        var point = lbConflictResolutionColumns.PointToClient(new Point(e.X, e.Y));
        var index = lbConflictResolutionColumns.IndexFromPoint(point);

        //if they are dragging it way down the bottom of the list
        if (index < 0)
            index = lbConflictResolutionColumns.Items.Count;

        //get the thing they are dragging
        var data =
            (IResolveDuplication)
            (e.Data.GetData(typeof(ColumnInfo)) ?? e.Data.GetData(typeof(PreLoadDiscardedColumn)));

        //find original index because if we are dragging down then we will want to adjust the index so that insert point is correct even after removing the object further up the list
        var originalIndex = lbConflictResolutionColumns.Items.IndexOf(data);

        lbConflictResolutionColumns.Items.Remove(data);

        if (originalIndex < index)
            lbConflictResolutionColumns.Items.Insert(Math.Max(0, index - 1), data);
        else
            lbConflictResolutionColumns.Items.Insert(index, data);

        SaveOrderIntoDatabase();
    }


    private void SaveOrderIntoDatabase()
    {
        for (var i = 0; i < lbConflictResolutionColumns.Items.Count; i++)
        {
            var extractionInformation = (IResolveDuplication)lbConflictResolutionColumns.Items[i];
            extractionInformation.DuplicateRecordResolutionOrder = i;
            extractionInformation.SaveToDatabase();
        }

        RefreshUIFromDatabase();
    }

    #endregion

    private void btnClose_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void lbConflictResolutionColumns_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            var indexFromPoint = lbConflictResolutionColumns.IndexFromPoint(e.Location);

            if (indexFromPoint != -1)
            {
                var RightClickMenu = new ContextMenuStrip();

                var resolver =
                    (IResolveDuplication)lbConflictResolutionColumns.Items[indexFromPoint];

                var target = resolver.DuplicateRecordResolutionIsAscending ? "DESC" : "ASC";
                var currently = resolver.DuplicateRecordResolutionIsAscending ? "ASC" : "DESC";

                RightClickMenu.Items.Add(
                    $"Set {resolver.GetRuntimeName()} to {target} (Currently resolution order is {currently})", null,
                    delegate
                    {
                        //flip its bit
                        resolver.DuplicateRecordResolutionIsAscending =
                            !resolver.DuplicateRecordResolutionIsAscending;
                        //save it to database
                        resolver.SaveToDatabase();
                        //refresh UI
                        RefreshUIFromDatabase();
                    });

                RightClickMenu.Show(lbConflictResolutionColumns.PointToScreen(e.Location));
            }
        }
    }


    private void tbHighlight_TextChanged(object sender, EventArgs e)
    {
        lbConflictResolutionColumns.Refresh();
    }

    private void lbConflictResolutionColumns_DrawItem(object sender, DrawItemEventArgs e)
    {
        e.DrawBackground();
        var g = e.Graphics;

        // draw the background color you want
        // mine is set to olive, change it to whatever you want
        g.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);

        // draw the text of the list item, not doing this will only show
        // the background color
        // you will need to get the text of item to display

        if (e.Index != -1)
        {
            var toDisplay = (sender as ListBox).Items[e.Index].ToString();

            //if it matches filter
            if (toDisplay.ToLower().Contains(tbHighlight.Text.ToLower())
                &&
                //and filter isn't blank
                !string.IsNullOrWhiteSpace(tbHighlight.Text))
                g.DrawString(toDisplay, new Font(e.Font, FontStyle.Regular), new SolidBrush(Color.HotPink),
                    new PointF(e.Bounds.X, e.Bounds.Y));
            else
                g.DrawString(toDisplay, new Font(e.Font, FontStyle.Regular), new SolidBrush(Color.Black),
                    new PointF(e.Bounds.X, e.Bounds.Y));
        }

        e.DrawFocusRectangle();
    }

    private void btnSelectToClipboard_Click(object sender, EventArgs e)
    {
        try
        {
            //this is used only to generate the SQL preview of how to resolve primary key collisions so no username/password is required - hence the null,null
            var resolver = new PrimaryKeyCollisionResolver(_table);

            if (sender == btnCopyPreview)
                Clipboard.SetText(resolver.GeneratePreviewSQL());

            if (sender == btnCopyDetection)
                Clipboard.SetText(resolver.GenerateCollisionDetectionSQL());
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void btnSetAllAscending_Click(object sender, EventArgs e)
    {
        if (!AllAreSameDirection())
            if (!Activator.YesNo(
                    "Are you sure you want to override the current resolution direction(s) with ASC , You will loose any currently configured column specific directions.",
                    "Override current directions?"))
                return;

        foreach (IResolveDuplication item in lbConflictResolutionColumns.Items)
        {
            item.DuplicateRecordResolutionIsAscending = true;
            item.SaveToDatabase();
        }

        RefreshUIFromDatabase();
    }

    private void btnSetAllDescending_Click(object sender, EventArgs e)
    {
        if (!AllAreSameDirection())
            if (!Activator.YesNo(
                    "Are you sure you want to override the current resolution direction(s) with DESC, You will loose any currently configured column specific directions.",
                    "Override current directions?"))
                return;

        foreach (IResolveDuplication item in lbConflictResolutionColumns.Items)
        {
            item.DuplicateRecordResolutionIsAscending = false;
            item.SaveToDatabase();
        }

        RefreshUIFromDatabase();
    }

    private bool AllAreSameDirection()
    {
        if (lbConflictResolutionColumns.Items.Count <= 1)
            return true;
        return
            lbConflictResolutionColumns.Items.Cast<IResolveDuplication>()
                .Select(resolver => resolver.DuplicateRecordResolutionOrder)
                .Distinct()
                .Count() == 1; //if count of distinct directions is 1 then they are all in the same direction
    }

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetScrollPos(IntPtr hWnd, Orientation nBar);
}