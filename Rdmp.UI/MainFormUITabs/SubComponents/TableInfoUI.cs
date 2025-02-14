// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Sharing.Refactoring;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.MainFormUITabs.SubComponents;

/// <summary>
/// Allows you to change a table reference (TableInfo) to point at a new location.  This should only be used when you have moved a dataset to a new database or server and you should select
/// 'Synchronize' after you make this change.
/// 
/// <para>The 'Synchronize' button will connect to the referenced server/database and check that it exists and that the columns in the database match the ColumnInfo collection in the Catalogue
/// database.  Synchronization happens automatically within the RDMP at some points (e.g. data load) but it is useful to manually do it sometimes if you know you have made a change to your
/// database schema and want to update the Catalogue.</para>
/// 
/// <para>If your TableInfo is pointed at a Table-valued Function then you can select 'Default Table Valued Function Parameters...' to launch a ParameterCollectionUI which contains all the defaults
/// that the Catalogue will use when invoking your SQL function.  The RDMP requires (and will automatically create) an SQL parameter (e.g. @myExcitingParameter) for each argument taken by
/// your Table-valued function of a matching datatype and name to the argument as declared in your database.  In practice these default parameter values will usually be overridden at a higher
/// level (e.g. during cohort identification).</para>
/// 
/// <para>This interface also allows you to mark a TableInfo 'Is Primary Extraction Table' which means that the QueryBuilder will start JOIN statements with this table where it is part of a complex
/// multi table query.</para>
/// </summary>
public partial class TableInfoUI : TableInfoUI_Design, ISaveableUI
{
    private TableInfo _tableInfo;

    public TableInfoUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.Tables;
        ObjectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;
    }

    public override void SetDatabaseObject(IActivateItems activator, TableInfo databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _tableInfo = databaseObject;

        ragSmiley1.StartChecking(_tableInfo);

        tbTableInfoID.Text = _tableInfo.ID.ToString();
        cbIsPrimaryExtractionTable.Checked = _tableInfo.IsPrimaryExtractionTable;
        tbTableInfoName.Text = _tableInfo.Name;
        tbTableInfoDatabaseAccess.Text = _tableInfo.Server;
        tbTableInfoDatabaseName.Text = _tableInfo.Database;
        tbSchema.Text = _tableInfo.Schema;

        btnParameters.Enabled = _tableInfo.IsTableValuedFunction;
        cbIsView.Checked = _tableInfo.IsView;

        //if it's a Lookup table, don't let them try to make it IsPrimaryExtractionTable (but let them disable that if they have already made that mistake somehow)
        if (_tableInfo.IsLookupTable())
            if (!cbIsPrimaryExtractionTable.Checked)
                cbIsPrimaryExtractionTable.Enabled = false;
    }


    private void cbIsPrimaryExtractionTable_CheckedChanged(object sender, EventArgs e)
    {
        _tableInfo.IsPrimaryExtractionTable = cbIsPrimaryExtractionTable.Checked;
        _tableInfo.SaveToDatabase();
    }

    private void cbIsView_CheckedChanged(object sender, EventArgs e)
    {
        _tableInfo.IsView = cbIsView.Checked;
        _tableInfo.SaveToDatabase();
    }


    private bool objectSaverButton1_BeforeSave(DatabaseEntity arg)
    {
        //do not mess with the table name if it is a table valued function
        if (_tableInfo.IsTableValuedFunction)
            return true;
        try
        {
            var newName = _tableInfo.GetFullyQualifiedName();
            _tableInfo.Name = newName;

            var oldName = _tableInfo.Repository.GetObjectByID<TableInfo>(_tableInfo.ID).GetFullyQualifiedName();

            if (oldName != newName &&
                Activator.YesNo(
                    "You have just renamed a TableInfo, would you like to refactor your changes into ExtractionInformations?",
                    "Apply Code Refactoring?"))
                DoRefactoring(oldName, newName);
        }
        catch (Exception)
        {
            // could not refactor or other problem
            // just give up on trying to be helpful
            return true;
        }

        return true;
    }

    private void DoRefactoring(string toReplace, string toReplaceWith)
    {
        var updatesMade = SelectSQLRefactorer.RefactorTableName(_tableInfo, toReplace, toReplaceWith);

        MessageBox.Show($"Made {updatesMade} replacements in ExtractionInformation/ColumnInfos.");
    }

    private void tbTableInfoName_TextChanged(object sender, EventArgs e)
    {
        _tableInfo.Name = tbTableInfoName.Text;
    }

    private void tbTableInfoDatabaseAccess_TextChanged(object sender, EventArgs e)
    {
        _tableInfo.Server = tbTableInfoDatabaseAccess.Text;
    }

    private void tbTableInfoDatabaseName_TextChanged(object sender, EventArgs e)
    {
        _tableInfo.Database = ((TextBox)sender).Text;
    }

    private void tbSchema_TextChanged(object sender, EventArgs e)
    {
        _tableInfo.Schema = ((TextBox)sender).Text;
    }

    private void btnParameters_Click(object sender, EventArgs e)
    {
        ParameterCollectionUI.ShowAsDialog(Activator, ParameterCollectionUIOptionsFactory.Create(_tableInfo));
    }

    private void btnSynchronize_Click(object sender, EventArgs e)
    {
        try
        {
            var isSync =
                new TableInfoSynchronizer(_tableInfo).Synchronize(new MakeChangePopup(new YesNoYesToAllDialog()));

            if (isSync)
                MessageBox.Show("TableInfo is synchronized");
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }
}