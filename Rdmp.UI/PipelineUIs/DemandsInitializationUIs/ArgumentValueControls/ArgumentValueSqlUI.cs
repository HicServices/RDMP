// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.SqlDialogs;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are of Type string but expect SQL code.  Clicking the button will launch an SQL editor with syntax highlighting.</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueSqlUI : UserControl, IArgumentValueUI
{
    private ArgumentValueUIArgs _args;

    /// <summary>
    /// Helper function to remove extra whitespace in SQL query so it looks nicer in a single line textbox
    /// </summary>
    /// <param name="sqlText"></param>
    /// <returns></returns>
    private string FormatSqlForTextbox(object sqlText)
    {
        string sqlTextboxPretty = sqlText == null ? "" : sqlText.ToString();
        sqlTextboxPretty = new Regex(@"\s+").Replace(sqlTextboxPretty, " ");
        return sqlTextboxPretty;
    }

    public ArgumentValueSqlUI()
    {
        InitializeComponent();
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        _args = args;

        tbSql.Text = FormatSqlForTextbox(args.InitialValue);
        lblSqlClause.Text = args.ContextText;
    }

    private void btnSetSQL_Click(object sender, System.EventArgs e)
    {

        SetSQLDialog dialog = new SetSQLDialog((string)_args.InitialValue, new RDMPCombineableFactory());
        DialogResult d = dialog.ShowDialog();

        if (d == DialogResult.OK)
        {
            _args.Setter(_args.InitialValue = dialog.Result);
            tbSql.Text = FormatSqlForTextbox(_args.InitialValue);
        }
    }
}