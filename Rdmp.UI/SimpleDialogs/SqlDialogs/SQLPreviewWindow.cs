// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.UI.ScintillaHelper;

namespace Rdmp.UI.SimpleDialogs.SqlDialogs;

/// <summary>
/// Shows you some SQL the system is about to execute with a description of what it is trying to achieve.  You can choose either 'Ok' to execute the SQL and carry on with the rest
/// of the ongoing procedure or Cancel (the SQL will not run and the procedure will be abandoned).
/// </summary>
public partial class SQLPreviewWindow : Form
{
    public SQLPreviewWindow(string title, string msg, string sql)
    {
        InitializeComponent();

        lblMessage.Text = msg;
        Text = title;

        var designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        if (designMode) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        var queryEditor = new ScintillaTextEditorFactory().Create();
        queryEditor.Text = sql;

        queryEditor.ReadOnly = true;

        panel1.Controls.Add(queryEditor);
        btnOk.Select();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool YesToAll { get; set; }

    private void btnOk_Click(object sender, EventArgs e)
    {
        YesToAll = sender == btnOkToAll;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    public static DialogResult Show(string title, string message, string sql)
    {
        var dialog = new SQLPreviewWindow(title, message, sql);
        dialog.ShowDialog();

        return dialog.DialogResult;
    }
}