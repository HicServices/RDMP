using System;
using System.Collections.Generic;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
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
                Height = Dim.Fill ()
            };

            //////////////////////////////////////////////// Username //////////////////////
            
            var lbluser = new Label("Username:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };

            var tbuser = new TextField(string.Empty)
            {
                X = Pos.Right(lbluser),
                Y = 0,
                Width = Dim.Fill(),
            };
            tbuser.Changed += (s, e) => Username = ((TextField)s).Text.ToString();
            
            //////////////////////////////////////////////// Password  //////////////////////
            
            var lblPassword = new Label("Password:")
            {
                X = 0,
                Y = Pos.Bottom(lbluser),
                Height = 1,
            };

            var tbPassword = new TextField(string.Empty){
                X = Pos.Right(lblPassword),
                Y = Pos.Bottom(lbluser),
                Width = Dim.Fill()
            };
            tbPassword.Changed += (s, e) => Password = ((TextField)s).Text.ToString();

            //////////////////////// Database Type /////////////

            var btnDatabaseType = new Button("Database Type")
            {
                X = 0,
                Y = Pos.Bottom(lblPassword),
                Width = 10,
                Height = 1,
                Clicked = () =>
                {
                    if(_activator.SelectEnum("Database Type",typeof(DatabaseType),out Enum chosen))
                    {
                        DatabaseType = (DatabaseType) chosen;
                    }
                }
            };

            //////////////////////////////////////////////// Server  //////////////////////
            
            var lblServer = new Label("Server:")
            {
                X = 0,
                Y = Pos.Bottom(btnDatabaseType),
                Height = 1,
            };
            
            var tbServer = new TextField(string.Empty){
                X = Pos.Right(lblServer),
                Y = Pos.Bottom(btnDatabaseType),
                Width = Dim.Fill()
            };
            tbServer.Changed += (s, e) => Server = ((TextField)s).Text.ToString();
            

            //////////////////////////////////////////////// Database  //////////////////////

            var lblDatabase = new Label("Database:")
            {
                X = 0,
                Y = Pos.Bottom(lblServer),
                Height = 1,
            };

            var tbDatabase = new TextField(string.Empty){
                X = Pos.Right(lblDatabase),
                Y = Pos.Bottom(lblServer),
                Width = Dim.Fill()
            };
            tbDatabase.Changed += (s, e) => Database = ((TextField)s).Text.ToString();
            
            //////////////////////////////////////////////// Schema  //////////////////////
            
            var lblSchema = new Label("Schema:")
            {
                X = 0,
                Y = Pos.Bottom(lblDatabase),
                Height = 1,
            };

            var tbSchema = new TextField(string.Empty)
            {
                X = Pos.Right(lblSchema),
                Y = Pos.Bottom(lblDatabase),
                Width = Dim.Fill()
            };

            tbSchema.Changed += (s, e) => Schema = ((TextField)s).Text.ToString();


            //////////////////////////////////////////////// Table  //////////////////////
             
            CbIsView = new CheckBox("Is View")
            {
                X = 0,
                Y = Pos.Bottom(lblSchema),
                Width = Dim.Fill()
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


            tbTable.Changed += (s, e) => Table = ((TextField)s).Text.ToString();
            
            win.Add(lbluser);
            win.Add(tbuser);
            win.Add(lblPassword);
            win.Add(tbPassword);
            win.Add(btnDatabaseType);
            win.Add(lblServer);
            win.Add(tbServer);
            win.Add(lblDatabase);
            win.Add(tbDatabase);

            if (_showTableComponents)
            {
                win.Add(lblSchema);
                win.Add(tbSchema);
                win.Add(CbIsView);
                win.Add(lblTable);
                win.Add(tbTable);
            }
            


            var btnOk = new Button(_okText,true)
            {
                X = 0,
                Y = _showTableComponents ? Pos.Bottom(lblTable) : Pos.Bottom(lblDatabase),
                Width = 5,
                Height = 1,
                Clicked = () =>
                {
                    OkClicked = true;
                    Application.RequestStop();
                }
            };

            var btnCancel = new Button("Cancel",true)
            {
                X = Pos.Right(btnOk)+10,
                Y = _showTableComponents ? Pos.Bottom(lblTable) : Pos.Bottom(lblDatabase),
                Width = 5,
                Height = 1,
                Clicked = Application.RequestStop

            };

            win.Add(btnOk);
            win.Add(btnCancel);

            Application.Run(win);

            return OkClicked;
        }

        


        public DiscoveredDatabase GetDiscoveredDatabase()
        {
            if (!OkClicked)
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

            return new DiscoveredServer(Server,Database,DatabaseType,Username,Password).ExpectDatabase(Database).ExpectTable(Table,Schema,TableType);
        }

        
    }
}
