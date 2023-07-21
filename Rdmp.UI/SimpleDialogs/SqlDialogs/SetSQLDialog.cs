// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.ScintillaHelper;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs.SqlDialogs;

/// <summary>
/// Allows the user to view and edit some SQL they have written.  Basically the same as ShowSQL but this window expects you to have populated some meaningful SQL that the
/// caller will store/use somehow.
/// </summary>
public partial class SetSQLDialog : Form
{   
    public Scintilla QueryEditor;
    private bool _designMode;

    public string Result => QueryEditor.Text;

    public SetSQLDialog(string originalSQL, ICombineableFactory commandFactory)
    {
        InitializeComponent();
            
        _designMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        if (_designMode) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        QueryEditor = new ScintillaTextEditorFactory().Create(commandFactory);
        QueryEditor.Text = originalSQL;
            
        panel1.Controls.Add(QueryEditor);
        
    }

    private void button1_Click(object sender, EventArgs e)
    {

        DialogResult = DialogResult.OK;
        Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}