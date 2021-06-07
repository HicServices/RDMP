// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiServerDatabaseTableSelector
    {
        private readonly IBasicActivateItems _activator;
        private readonly string _prompt;
        private readonly string _okText;
        private readonly bool _showTableComponents;


        public string Username { get; private set; }

        public string Password { get; private set; }

        public string Server { get; private set; }
        public string Database { get; private set; }
        public string Schema { get;private set; }
        public string Table { get; private set; }
        
        public DatabaseType DatabaseType { get; private set; } = DatabaseType.MicrosoftSQLServer;
        public TableType TableType => CbIsView != null && CbIsView.Checked ? TableType.View : TableType.Table;

        public bool OkClicked { get; private set; }

        private CheckBox CbIsView;
        private CheckBox CbIsTableValuedFunc;
        private TextField tbuser;
        private TextField tbPassword;

        public ConsoleGuiServerDatabaseTableSelector(IBasicActivateItems activator,string prompt, string okText, bool showTableComponents)
        {
            _activator = activator;
            _prompt = prompt;
            _okText = okText;
            _showTableComponents = showTableComponents;
        }

        public bool ShowDialog()
        {
            var win = new Window (_prompt) {
                X = 0,
                Y = 0,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill (),
                Modal = true,
                ColorScheme = ConsoleMainWindow.ColorScheme
            };

            //////////////////////////////////////////////// Username //////////////////////
            
            var lbluser = new Label("Username:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };
            win.Add(lbluser);

            tbuser = new TextField(string.Empty)
            {
                X = Pos.Right(lbluser),
                Y = 0,
                Width = 15,
            };
            tbuser.TextChanged += (s) => Username = tbuser.Text.ToString();
            win.Add(tbuser);

            //////////////////////////////////////////////// Password  //////////////////////

            var lblPassword = new Label("Password:")
            {
                X = 0,
                Y = Pos.Bottom(lbluser),
                Height = 1,
            };
            win.Add(lblPassword);

            tbPassword = new TextField(string.Empty){
                X = Pos.Right(lblPassword),
                Y = Pos.Bottom(lbluser),
                Width = 15,
                Secret = true
            };

            tbPassword.TextChanged += (s) => Password = tbPassword.Text.ToString();
            win.Add(tbPassword);

            var btnPickCredentials = new Button("Use Credentials")
            {
                X = Pos.Right(tbPassword),
                Y = Pos.Bottom(lbluser),
            };

            btnPickCredentials.Clicked += BtnPickCredentials_Clicked;
            win.Add(btnPickCredentials);



            //////////////////////// Database Type /////////////

            var btnDatabaseType = new Button($"Database Type ({DatabaseType})")
            {
                X = 0,
                Y = Pos.Bottom(lblPassword),
                Height = 1
            };
            win.Add(btnDatabaseType);

            btnDatabaseType.Clicked += () =>
            {
                if (_activator.SelectEnum("Database Type", typeof(DatabaseType), out Enum chosen))
                {
                    DatabaseType = (DatabaseType) chosen;
                    btnDatabaseType.Text = $"Database Type ({chosen})";
                    win.SetNeedsDisplay();
                }
            };

            //////////////////////////////////////////////// Server  //////////////////////

            var lblServer = new Label("Server:")
            {
                X = 0,
                Y = Pos.Bottom(btnDatabaseType),
                Height = 1,
            };
            win.Add(lblServer);

            var tbServer = new TextField(string.Empty){
                X = Pos.Right(lblServer),
                Y = Pos.Bottom(btnDatabaseType),
                Width = 17
            };
            tbServer.TextChanged += (s) => Server = tbServer.Text.ToString();

            win.Add(tbServer);

            //////////////////////////////////////////////// Database  //////////////////////

            var lblDatabase = new Label("Database:")
            {
                X = 0,
                Y = Pos.Bottom(lblServer),
                Height = 1,
            };
            win.Add(lblDatabase);

            var tbDatabase = new TextField(string.Empty){
                X = Pos.Right(lblDatabase),
                Y = Pos.Bottom(lblServer),
                Width = 15
            };
            win.Add(tbDatabase);
            tbDatabase.TextChanged += (s) => Database = tbDatabase.Text.ToString();

            var btnCreateDatabase = new Button("Create Da_tabase")
            {
                X = Pos.Right(tbDatabase) + 1,
                Y = Pos.Bottom(lblServer)
            };
            btnCreateDatabase.Clicked += CreateDatabase;
            win.Add(btnCreateDatabase);

            //////////////////////////////////////////////// Schema  //////////////////////

            var lblSchema = new Label("Schema:")
            {
                X = 0,
                Y = Pos.Bottom(lblDatabase),
                Height = 1
            };

            var tbSchema = new TextField(string.Empty)
            {
                X = Pos.Right(lblSchema),
                Y = Pos.Bottom(lblDatabase),
                Width = Dim.Fill()
            };

            tbSchema.TextChanged += (s) => Schema = tbSchema.Text.ToString();


            //////////////////////////////////////////////// Table  //////////////////////
             
            CbIsView = new CheckBox("Is View")
            {
                X = 0,
                Y = Pos.Bottom(lblSchema),
            };
            
            CbIsTableValuedFunc = new CheckBox("Is Func")
            {
                X = Pos.Right(CbIsView) + 5,
                Y = Pos.Bottom(lblSchema),
            };
            

            var lblTable = new Label("Table:")
            {
                X = 0,
                Y = Pos.Bottom(CbIsView),
                Height = 1,
            };

            var tbTable = new TextField(string.Empty)
            {
                X = Pos.Right(lblTable),
                Y = Pos.Bottom(CbIsView),
                Width = Dim.Fill()
            };
            
            tbTable.TextChanged += (s) => Table = tbTable.Text.ToString();
            

            if (_showTableComponents)
            {
                win.Add(lblSchema);
                win.Add(tbSchema);
                win.Add(CbIsView);
                win.Add(CbIsTableValuedFunc);
                win.Add(lblTable);
                win.Add(tbTable);
            }

            var btnOk = new Button(_okText,true)
            {
                X = 0,
                Y = _showTableComponents ? Pos.Bottom(lblTable) : Pos.Bottom(lblDatabase),
                Height = 1
            };
            btnOk.Clicked += () =>
            {
                OkClicked = true;
                Application.RequestStop();
            };

            var btnCancel = new Button("Cancel",true)
            {
                X = Pos.Right(btnOk)+10,
                Y = _showTableComponents ? Pos.Bottom(lblTable) : Pos.Bottom(lblDatabase),
                Height = 1
            };
            btnCancel.Clicked += Application.RequestStop;

            win.Add(btnOk);
            win.Add(btnCancel);

            Application.Run(win);

            return OkClicked;
        }

        private void CreateDatabase()
        {
            try
            {
                var db = GetDiscoveredDatabase(true);

                if(db == null)
                    _activator.Show("Enter all database details before trying to create");
                else
                if(db.Exists())
                    _activator.Show("Database already exists");
                else
                {
                    db.Create();
                    _activator.Show("Database Created Successfully");
                }
            }
            catch (Exception e)
            {
                _activator.ShowException("Create Database Failed",e);
            }
        }


        public DiscoveredDatabase GetDiscoveredDatabase(bool ignoreOk=false)
        {
            if (!OkClicked && !ignoreOk)
                return null;

            if (string.IsNullOrWhiteSpace(Server))
                return null;

            if (string.IsNullOrWhiteSpace(Database))
                return null;

            return new DiscoveredServer(Server,Database,DatabaseType,Username,Password).ExpectDatabase(Database);
        }


        public DiscoveredTable GetDiscoveredTable()
        {
            if (!OkClicked)
                return null;

            if (string.IsNullOrWhiteSpace(Server))
                return null;

            if (string.IsNullOrWhiteSpace(Database))
                return null;

            if(CbIsTableValuedFunc.Checked)
                return new DiscoveredServer(Server,Database,DatabaseType,Username,Password).ExpectDatabase(Database).ExpectTableValuedFunction(Table,Schema);

            return new DiscoveredServer(Server,Database,DatabaseType,Username,Password).ExpectDatabase(Database).ExpectTable(Table,Schema,TableType);
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
                    tbuser.Text = cred.Username;
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
