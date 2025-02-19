// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

/// <summary>
/// Part of JoinConfiguration and LookupConfiguration, allows you to drop a ColumnInfo into it to declare it as a key in a relationship being built (either a Lookup or a JoinInfo). Clicking
/// the garbage can will clear the control.
/// </summary>
public partial class KeyDropLocationUI : UserControl
{
    private JoinKeyType _keyType;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnInfo SelectedColumn { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public JoinKeyType KeyType
    {
        get => _keyType;
        set
        {
            _keyType = value;
            label.Text = KeyType switch
            {
                JoinKeyType.PrimaryKey => "(Primary Key)",
                JoinKeyType.ForeignKey => "(Foreign Key)",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /// <summary>
    /// Set this to allow dragging only certain items onto the control.  Return true to allow drop and false to prevent it.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<ColumnInfo, bool> IsValidGetter { get; set; }

    public event Action SelectedColumnChanged;

    public KeyDropLocationUI()
    {
        InitializeComponent();
        btnClear.Image = FamFamFamIcons.delete.ImageToBitmap();
        btnClear.Enabled = false;
    }

    private void tbPk1_DragEnter(object sender, DragEventArgs e)
    {
        e.Effect = DragDropEffects.None;

        var col = GetColumnInfoOrNullFromDrag(e);

        if (col == null)
            return;

        if (IsValidGetter != null && !IsValidGetter(col))
            return;

        e.Effect = DragDropEffects.Copy;
    }

    private void tbPk1_DragDrop(object sender, DragEventArgs e)
    {
        SelectedColumn = GetColumnInfoOrNullFromDrag(e);
        tbPk1.Text = SelectedColumn.ToString();
        btnClear.Enabled = true;

        SelectedColumnChanged?.Invoke();
    }

    private static ColumnInfo GetColumnInfoOrNullFromDrag(DragEventArgs e)
    {
        if (e.Data is not OLVDataObject data)
            return null;

        if (data.ModelObjects.Count != 1)
            return null;

        return data.ModelObjects[0] is ExtractionInformation ei ? ei.ColumnInfo : data.ModelObjects[0] as ColumnInfo;
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        Clear();
    }

    public void Clear()
    {
        tbPk1.Text = "";
        SelectedColumn = null;
        btnClear.Enabled = false;

        SelectedColumnChanged?.Invoke();
    }
}

public enum JoinKeyType
{
    PrimaryKey,
    ForeignKey
}