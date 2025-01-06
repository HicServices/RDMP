// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Modal dialog that prompts you to pick a database or table (<see cref="ServerDatabaseTableSelector"/>)
/// </summary>
public partial class ServerDatabaseTableSelectorDialog : Form
{
    public ServerDatabaseTableSelectorDialog(string taskDescription, bool includeTable, bool tableShouldBeNovel,
        IBasicActivateItems activator)
    {
        //start at cancel so if they hit the X nothing is selected
        DialogResult = DialogResult.Cancel;

        InitializeComponent();

        lblTaskDescription.Text = taskDescription;

        if (!includeTable)
            serverDatabaseTableSelector1.HideTableComponents();

        serverDatabaseTableSelector1.TableShouldBeNovel = tableShouldBeNovel;
        serverDatabaseTableSelector1.SetItemActivator(activator);
    }

    public DiscoveredDatabase SelectedDatabase => serverDatabaseTableSelector1.GetDiscoveredDatabase();
    public DiscoveredTable SelectedTable => serverDatabaseTableSelector1.GetDiscoveredTable();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowTableValuedFunctionSelection
    {
        get => serverDatabaseTableSelector1.AllowTableValuedFunctionSelection;
        set => serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = value;
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
        try
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            if (db == null)
            {
                MessageBox.Show("You must specify a database name");
                return;
            }

            if (!db.Exists())
            {
                db.Create();
                MessageBox.Show("Database Created");
            }
            else
            {
                MessageBox.Show("Database already exists");
            }
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }

    public void LockDatabaseType(DatabaseType databaseType)
    {
        serverDatabaseTableSelector1.LockDatabaseType(databaseType);
    }
}