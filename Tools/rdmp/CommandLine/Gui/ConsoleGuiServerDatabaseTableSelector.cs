// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.CommandLine.Gui {
    using FAnsi;
    using FAnsi.Discovery;
    using Rdmp.Core.CommandExecution;
    using Rdmp.Core.Curation.Data;
    using ReusableLibraryCode;
    using ReusableLibraryCode.Settings;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using Terminal.Gui;


    public partial class ConsoleGuiServerDatabaseTableSelector {

        private readonly IBasicActivateItems _activator;
        public string Username => tbUsername.Text.ToString();

        public string Password => tbPassword.Text.ToString();

        public string Server => cbxServer.Text.ToString();
        public string Database => cbxDatabase.Text.ToString();
        public string Schema => tbSchema.Text.ToString();
        public string Table => cbxTable.Text.ToString();

        /// <summary>
        /// Returns the DatabaseType that is selected in the dropdown or 
        /// <see cref="DatabaseType.MicrosoftSQLServer"/> if none selected
        /// </summary>
        public DatabaseType DatabaseType => cbxDatabaseType.SelectedItem < 0 ? DatabaseType.MicrosoftSQLServer :
            (DatabaseType)cbxDatabaseType.Source.ToList()[cbxDatabaseType.SelectedItem];
        
        /// <summary>
        /// Returns the table type selected in the radio group or <see cref="TableType.Table"/> if none selected
        /// </summary>
        public TableType TableType =>
            rgTableType.SelectedItem switch
            {
                0 => TableType.Table,
                1 => TableType.View,
                2 => TableType.TableValuedFunction,
                _ => TableType.View
            };

        public bool OkClicked { get; private set; }

        
        public ConsoleGuiServerDatabaseTableSelector(IBasicActivateItems activator, string prompt, string okText, bool showTableComponents)
        {
            _activator = activator;

            InitializeComponent();
            btnUseExisting.Clicked += BtnPickCredentials_Clicked;

            // Fix frame view color scheme going dodgy when focused
            frameview1.ColorScheme = new ColorScheme
            {
                Normal = ColorScheme.Normal,
                Focus = ColorScheme.Normal,
                Disabled = ColorScheme.Disabled,
                HotFocus = ColorScheme.Normal,
            };
            tbUsername.ColorScheme = ColorScheme;
            tbPassword.ColorScheme = ColorScheme;
            btnUseExisting.ColorScheme = ColorScheme;

            cbxDatabaseType.SetSource(Enum.GetValues<DatabaseType>());
            
            cbxDatabase.Leave += CbxDatabase_Leave;

            cbxDatabaseType.AddKeyBinding(Key.CursorDown, Command.Expand);
            cbxTable.AddKeyBinding(Key.CursorDown, Command.Expand);

            // Same guid as used by the windows client but probably the apps have different UserSettings files
            // so sadly won't share one anothers recent histories
            cbxServer.SetSource(UserSettings.GetHistoryForControl(new Guid("01ccc304-0686-4145-86a5-cc0468d40027"))
                .Where(e=>!string.IsNullOrWhiteSpace(e))
                .ToList());

            SetupComboBox(cbxDatabase);
            SetupComboBox(cbxTable);
            // do this last so that it gets focus in the UI (yes I know thats hacky)
            SetupComboBox(cbxServer);  

            cbxDatabaseType.SelectedItem = cbxDatabaseType.Source.ToList().IndexOf(DatabaseType.MicrosoftSQLServer);
            btnCreateDatabase.Clicked += CreateDatabase;

            lblDescription.Text = prompt;

            btnOk.Text = okText;

            btnOk.Clicked += () =>
            {
                OkClicked = true;
                Application.RequestStop();
            };

            btnCancel.X = Pos.Right(btnOk) + 1;
            btnCancel.Clicked += () => Application.RequestStop();

            btnRefresh.Clicked += RefreshDatabaseList;

            if (!showTableComponents)
            {
                lblSchema.Visible = false;
                tbSchema.Visible = false;
                lblTable.Visible = false;
                cbxTable.Visible = false;
                rgTableType.Visible = false;
                btnOk.Y -= 5;
                btnCancel.Y -= 5;
                Height -= 5;
            }

        }

        private void SetupComboBox(ComboBox combo)
        {
            combo.AddKeyBinding(Key.CursorDown, Command.Expand);

            var expand = typeof(ComboBox).GetMethod("Expand");
            var collapse = typeof(ComboBox).GetMethod("Collapse");

            expand.Invoke(combo, new object[0]);
            collapse.Invoke(combo,new object[0]);
        }

        private void CbxDatabase_Leave(FocusEventArgs obj)
        {
            UpdateTableList();
        }

        private void UpdateTableList()
        {
            try
            {
                var db = new DiscoveredServer(GetBuilder()).ExpectDatabase(Database);

                cbxTable.SetSource(
                    db.DiscoverTables(true)
                    .Union(db.DiscoverTableValuedFunctions())
                    .Select(t=>t.GetRuntimeName())
                    .ToList());
            }
            catch (Exception)
            {
                // could not find any tables nevermind
                return;
            }
        }

        private void RefreshDatabaseList()
        {
            try
            {
                var server = new DiscoveredServer(GetBuilder());

                cbxDatabase.SetSource(
                    server.DiscoverDatabases()
                    .Select(d=>d.GetRuntimeName())
                    .ToList());
            }
            catch (Exception ex)
            {
                _activator.ShowException("Could not fetch databases", ex);
            }
        }

        public DbConnectionStringBuilder GetBuilder()
        {
            var helper = DatabaseCommandHelper.For(DatabaseType);
            return helper.GetConnectionStringBuilder(Server, Database, Username, Password);
        }

        public bool ShowDialog()
        {
            Application.Run(this);
            return OkClicked;
        }

        private void CreateDatabase()
        {
            try
            {
                var db = GetDiscoveredDatabase(true);

                if (db == null)
                    _activator.Show("Enter all database details before trying to create");
                else
                if (db.Exists())
                    _activator.Show("Database already exists");
                else
                {
                    db.Create();
                    _activator.Show("Database Created Successfully");
                }
            }
            catch (Exception e)
            {
                _activator.ShowException("Create Database Failed", e);
            }
        }


        public DiscoveredDatabase GetDiscoveredDatabase(bool ignoreOk = false)
        {
            if (!OkClicked && !ignoreOk)
                return null;

            if (string.IsNullOrWhiteSpace(Server))
                return null;

            if (string.IsNullOrWhiteSpace(Database))
                return null;

            return new DiscoveredServer(Server, Database, DatabaseType, Username, Password).ExpectDatabase(Database);
        }


        public DiscoveredTable GetDiscoveredTable()
        {
            if (!OkClicked)
                return null;

            if (string.IsNullOrWhiteSpace(Server))
                return null;

            if (string.IsNullOrWhiteSpace(Database))
                return null;

            if (TableType == TableType.TableValuedFunction)
                return new DiscoveredServer(Server, Database, DatabaseType, Username, Password).ExpectDatabase(Database).ExpectTableValuedFunction(Table, Schema);

            return new DiscoveredServer(Server, Database, DatabaseType, Username, Password).ExpectDatabase(Database).ExpectTable(Table, Schema, TableType);
        }


        private void BtnPickCredentials_Clicked()
        {
            if (_activator == null)
            {
                return;
            }

            var creds = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();

            if (!creds.Any())
            {
                _activator.Show("You do not have any DataAccessCredentials configured");
                return;
            }

            var cred = (DataAccessCredentials)_activator.SelectOne("Select Credentials", creds);
            if (cred != null)
            {
                try
                {
                    tbUsername.Text = cred.Username;
                    tbPassword.Text = cred.GetDecryptedPassword();
                }
                catch (Exception ex)
                {
                    _activator.ShowException("Error decrypting password", ex);
                }
            }
        }
    }
}
