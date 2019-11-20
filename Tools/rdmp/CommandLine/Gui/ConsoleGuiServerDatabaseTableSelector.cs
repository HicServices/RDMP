using System.Collections.Generic;
using FAnsi.Discovery;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiServerDatabaseTableSelector
    {
        private readonly string _prompt;
        private readonly string _okText;


        public string Username { get; private set; }

        public string Password { get; private set; }

        public string Server { get; private set; }
        public string Database { get; private set; }
        public string Schema { get;private set; }
        public string Table { get; private set; }

        public ConsoleGuiServerDatabaseTableSelector(string prompt, string okText)
        {
            _prompt = prompt;
            _okText = okText;
        }

        public bool ShowDialog()
        {
            bool okClicked = false;

            var win = new Window (_prompt) {
                X = 0,
                Y = 0,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };

            var lbluser = new Label("Username:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };
            
            var lblPassword = new Label("Password:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };

            var tbPassword = new TextField(string.Empty){
                X = Pos.Right(lbluser),
                Width = Dim.Fill()
            };
            tbPassword.Changed += (s, e) => Password = ((TextField)s).Text.ToString();

            var tbuser = new TextField(string.Empty)
            {
                X = Pos.Right(lbluser),
                Width = Dim.Fill(),
            };
            tbuser.Changed += (s, e) => Username = ((TextField)s).Text.ToString();
            
            var lblServer = new Label("Server:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };
            
            var tbServer = new TextField(string.Empty){
                X = Pos.Right(lblServer),
                Width = Dim.Fill()
            };
            tbServer.Changed += (s, e) => Server = ((TextField)s).Text.ToString();
            
            var lblDatabase = new Label("Database:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };

            var tbDatabase = new TextField(string.Empty){
                X = Pos.Right(lblDatabase),
                Width = Dim.Fill()
            };
            tbDatabase.Changed += (s, e) => Database = ((TextField)s).Text.ToString();
            
            
            var lblSchema = new Label("Schema:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };

            var tbSchema = new TextField(string.Empty)
            {
                X = Pos.Right(lblSchema),
                Width = Dim.Fill()
            };

            tbSchema.Changed += (s, e) => Schema = ((TextField)s).Text.ToString();

            var lblTable = new Label("Table:")
            {
                X = 0,
                Y = 0,
                Height = 1,
            };

            var tbTable = new TextField(string.Empty)
            {
                X = Pos.Right(lblTable),
                Width = Dim.Fill()
            };
            tbTable.Changed += (s, e) => Table = ((TextField)s).Text.ToString();
            
            win.Add(lbluser);
            win.Add(tbuser);

            
            var btnOk = new Button(_okText,true)
            {
                X = Pos.Percent(100)-10,
                Y = Pos.Bottom(tbuser), //todo change to bot
                Width = 5,
                Height = 1
            };

            Application.Run(win);

            return okClicked;
        }


        public DiscoveredDatabase GetDiscoveredDatabase()
        {
            return null;
        }

        public DiscoveredTable GetDiscoveredTable()
        {
            return null;
        }
    }
}
