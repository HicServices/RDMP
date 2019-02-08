// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using FAnsi;
using FAnsi.Discovery;
using ReusableLibraryCode;

namespace ReusableUIComponents
{
    /// <summary>
    /// Modal dialog that prompts you to pick a database or table (<see cref="ServerDatabaseTableSelector"/>)
    /// </summary>
    public partial class ServerDatabaseTableSelectorDialog : Form
    {
        public ServerDatabaseTableSelectorDialog(string taskDescription, bool includeTable, bool tableShouldBeNovel)
        {
            InitializeComponent();

            lblTaskDescription.Text = taskDescription;
            
            if(!includeTable)
                serverDatabaseTableSelector1.HideTableComponents();

            serverDatabaseTableSelector1.TableShouldBeNovel = tableShouldBeNovel;

            serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;
        }

        void serverDatabaseTableSelector1_SelectionChanged()
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            if (db != null && !db.Server.Exists())
            {
                btnCreate.Enabled = false;
                return;
            }

            //novel db name
            btnCreate.Enabled = db != null && !db.Exists();
        }

        public DiscoveredDatabase SelectedDatabase { get { return serverDatabaseTableSelector1.GetDiscoveredDatabase(); } }
        public DiscoveredTable SelectedTable { get { return serverDatabaseTableSelector1.GetDiscoveredTable(); }}

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
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            if(!db.Exists())
            {
                db.Create();
                MessageBox.Show("Database Created");
            }

        }

        public void LockDatabaseType(DatabaseType databaseType)
        {
            serverDatabaseTableSelector1.LockDatabaseType(databaseType);
        }
    }
}
