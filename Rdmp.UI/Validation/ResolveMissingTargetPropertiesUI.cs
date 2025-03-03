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
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Validation;

namespace Rdmp.UI.Validation;

/// <summary>
/// Allows you to recover from a mismatch in columns in a Catalogue when validation rules were originally written for it and the state it is in now.  You will automatically see this
/// form when editing the Validation rules on a Catalogue that has had CatalogueItems that previously had validation removed/renamed.  The Form prompts you to drag and drop matching columns
/// to indicate whether any new columns are semantically the same as the old ones that had disappeared (e.g. where a column has changed names).  Also allows you to delete the orphans (validation
/// rules for columns that are no longer there/extractable).
/// </summary>
public partial class ResolveMissingTargetPropertiesUI : Form
{
    private string[] AvailableColumns { get; set; }

    public ResolveMissingTargetPropertiesUI(Validator validator, ExtractionInformation[] availableColumns)
    {
        if (validator == null && availableColumns == null)
            return;

        AvailableColumns = availableColumns.Select(e => e.GetRuntimeName()).ToArray();

        AdjustedValidator = validator;
        InitializeComponent();

        lbAvailableColumns.Items.AddRange(GetUnReferencedColumns(validator, AvailableColumns).ToArray());
        lbMissingReferences.Items.AddRange(GetMissingReferences(validator, availableColumns).ToArray());
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Validator AdjustedValidator { get; set; }

    public static IEnumerable<string> GetUnReferencedColumns(Validator v, IEnumerable<string> columns)
    {
        return columns.Where(col => !v.ItemValidators.Any(iv => iv.TargetProperty.Equals(col)));
    }

    public static IEnumerable<ItemValidator> GetMissingReferences(Validator v,
        IEnumerable<ExtractionInformation> columns)
    {
        return v.ItemValidators.Where(iv => !columns.Any(c => c.GetRuntimeName().Equals(iv.TargetProperty)));
    }

    private void lbMissingReferences_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete) DeleteSelectedReferences();
    }

    private void DeleteSelectedReferences()
    {
        var itemValidators = lbMissingReferences.SelectedItems.Cast<ItemValidator>().ToArray();

        if (itemValidators.Length > 0)
            foreach (var iv in itemValidators)
            {
                AdjustedValidator.RemoveItemValidator(iv.TargetProperty);
                lbMissingReferences.Items.Remove(iv);
                tbOperations.AppendText($"Deleted Missing Reference:{Environment.NewLine}");
                tbOperations.AppendText(iv.TargetProperty + Environment.NewLine);
            }
    }

    #region drag and drop

    private ItemValidator _dragTarget;

    private void lbMissingReferences_MouseDown(object sender, MouseEventArgs e)
    {
        var indexFromPoint = lbMissingReferences.IndexFromPoint(e.X, e.Y);

        if (indexFromPoint != ListBox.NoMatches)
            _dragTarget = lbMissingReferences.Items[indexFromPoint] as ItemValidator;
    }

    private void lbMissingReferences_MouseUp(object sender, MouseEventArgs e)
    {
        _dragTarget = null;
    }


    private void lbMissingReferences_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragTarget != null && MouseButtons == MouseButtons.Left)
            DoDragDrop(_dragTarget, DragDropEffects.Link);
    }

    private void lbAvailableColumns_DragEnter(object sender, DragEventArgs e)
    {
        e.Effect = e.Data.GetDataPresent(typeof(ItemValidator)) ? DragDropEffects.Link : DragDropEffects.None;
    }

    private void lbAvailableColumns_DragDrop(object sender, DragEventArgs e)
    {
        var indexFromPoint = lbAvailableColumns.IndexFromPoint(lbAvailableColumns.PointToClient(new Point(e.X, e.Y)));

        if (indexFromPoint != ListBox.NoMatches &&
            e.Data?.GetData(typeof(ItemValidator)) is ItemValidator missingReference)
        {
            var oldName = missingReference.TargetProperty;
            var newName = (string)lbAvailableColumns.Items[indexFromPoint];

            AdjustedValidator.RenameColumn(oldName, newName);

            tbOperations.AppendText($"Renamed Missing Reference:{Environment.NewLine}");
            tbOperations.AppendText(oldName + Environment.NewLine);
            tbOperations.AppendText($"To:{Environment.NewLine}");
            tbOperations.AppendText(newName + Environment.NewLine);

            lbMissingReferences.Items.Remove(missingReference);
            lbAvailableColumns.Items.Remove(newName);
        }
    }

    private static void ResolveMissingReferenceAs(ItemValidator missingReference, string newTarget)
    {
        missingReference.TargetProperty = newTarget;
    }

    #endregion

    private void cbSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        for (var i = 0; i < lbMissingReferences.Items.Count; i++)
            lbMissingReferences.SetSelected(i, cbSelectAll.Checked);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        AdjustedValidator = null;
        Close();
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }
}