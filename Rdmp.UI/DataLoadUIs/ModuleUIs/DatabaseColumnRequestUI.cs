// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using FAnsi.Discovery;
using TypeGuesser;

namespace Rdmp.UI.DataLoadUIs.ModuleUIs;

public partial class DatabaseColumnRequestUI : UserControl
{
    private readonly DatabaseColumnRequest _column;
    private bool bLoaded = false;

    public DatabaseColumnRequestUI(DatabaseColumnRequest column)
    {
        _column = column;
        InitializeComponent();

        lblColumnName.Text = column.ColumnName;

        ddManagedType.DataSource = DatabaseTypeRequest.PreferenceOrder;

        var request = column.TypeRequested;
        if (request != null)
        {
            if (request.CSharpType != null)
                ddManagedType.SelectedItem = request.CSharpType;

            if (request.Size != null)
                nBeforeDecimal.Value = request.Size.NumbersBeforeDecimalPlace;

            if (request.Size != null)
                nAfterDecimal.Value = request.Size.NumbersAfterDecimalPlace;

            if (request.Width.HasValue)
                nLength.Value = request.Width.Value;
        }

        tbExplicitDbType.Text = column.ExplicitDbType;
            
        bLoaded = true;
        ResetVisibility();
    }

    private void ResetVisibility()
    {
        if (!string.IsNullOrEmpty(tbExplicitDbType.Text))
        {
            ddManagedType.Visible = false;
            label2.Visible = false;
            nLength.Visible = false;
            label3.Visible = false;
            nBeforeDecimal.Visible = false;
            label4.Visible = false;
            nAfterDecimal.Visible = false;
            label5.Visible = false;
            return;
        }

        ddManagedType.Visible = true;
        label2.Visible = true;

        var type = ddManagedType.SelectedItem as Type;
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.String:
                nLength.Visible = true;
                nBeforeDecimal.Visible = false;
                nAfterDecimal.Visible = false;
                label3.Visible = true;
                label4.Visible = false;
                label5.Visible = false;
                return;
            case TypeCode.Decimal:
                nLength.Visible = false;
                nBeforeDecimal.Visible = true;
                nAfterDecimal.Visible = true;
                label3.Visible = false;
                label4.Visible = true;
                label5.Visible = true;
                return;
            default:
                nLength.Visible = false;
                nBeforeDecimal.Visible = false;
                nAfterDecimal.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                return;
        }
    }

    private void tbExplicitDbType_TextChanged(object sender, System.EventArgs e)
    {
        ResetVisibility();
            
        if (!bLoaded)
            return;

        _column.ExplicitDbType = tbExplicitDbType.Text;
    }

    private void ddManagedType_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        ResetVisibility();

        if (!bLoaded)
            return;

        if(_column.TypeRequested != null)
            _column.TypeRequested.CSharpType = (Type) ddManagedType.SelectedItem;
    }

    private void n_ValueChanged(object sender, EventArgs e)
    {
        ResetVisibility();

        if (!bLoaded)
            return;

        var n = (NumericUpDown) sender;

        if (_column.TypeRequested != null)
        {
            if(n == nLength)
                _column.TypeRequested.Width = (int)n.Value;

            if (n == nAfterDecimal)
                _column.TypeRequested.Size.NumbersAfterDecimalPlace = (int)n.Value;

            if (n == nBeforeDecimal)
                _column.TypeRequested.Size.NumbersBeforeDecimalPlace = (int)n.Value;
        }
    }
}