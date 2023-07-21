// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.UI.ScintillaHelper;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Allows you to view a given piece of SQL.  This dialog is used whenever the RDMP wants to show you some SQL and includes syntax highlighting.  It may be readonly or editable
/// depending on the context in which the dialog was launched.
/// </summary>
public partial class ShowSQLUI : Form
{
    public ShowSQLUI(string sql, bool isReadOnly = false)
    {
        InitializeComponent();

        var designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        if (designMode) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        var queryEditor = new ScintillaTextEditorFactory().Create();
        queryEditor.Text = sql;
        queryEditor.ReadOnly = isReadOnly;

        Controls.Add(queryEditor);
    }
}