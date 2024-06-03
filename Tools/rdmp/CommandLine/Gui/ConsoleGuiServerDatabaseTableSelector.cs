// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Terminal.Gui;


namespace Rdmp.Core.CommandLine.Gui;

public partial class ConsoleGuiServerDatabaseTableSelector
{
    private readonly IBasicActivateItems _activator;
    public string Username => tbUsername.Text.ToString();

    public string Password => tbPassword.Text.ToString();

    public string Server => tbServer.Text.ToString();
    public string Database => tbDatabase.Text.ToString();
    public string Schema => tbSchema.Text.ToString();
    public string Table => tbTable.Text.ToString();

    /// <summary>
    /// Returns the DatabaseType that is selected in the dropdown or
    /// <see cref="DatabaseType.MicrosoftSQLServer"/> if none selected
    /// </summary>
    public DatabaseType DatabaseType => cbxDatabaseType.SelectedItem < 0
        ? DatabaseType.MicrosoftSQLServer
        : (DatabaseType)cbxDatabaseType.Source.ToList()[cbxDatabaseType.SelectedItem];

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


    public ConsoleGuiServerDatabaseTableSelector(IBasicActivateItems activator, string prompt, string okText,
        bool showTableComponents)
    {
        _activator = activator;

        InitializeComponent();
        btnUseExisting.Clicked += BtnPickCredentials_Clicked;

        tbUsername.ColorScheme = ColorScheme;
        tbPassword.ColorScheme = ColorScheme;
        tbPassword.Secret = true;
        btnUseExisting.ColorScheme = ColorScheme;

        cbxDatabaseType.SetSource(Enum.GetValues<DatabaseType>());

        cbxDatabaseType.AddKeyBinding(Key.CursorDown, Command.Expand);

        AddNoWordMeansShowAllAutocomplete(tbServer);
        AddNoWordMeansShowAllAutocomplete(tbDatabase);
        AddNoWordMeansShowAllAutocomplete(tbTable);

        // Same guid as used by the windows client but probably the apps have different UserSettings files
        // so sadly won't share one anothers recent histories
        tbServer.Autocomplete.AllSuggestions = UserSettings
            .GetHistoryForControl(new Guid("01ccc304-0686-4145-86a5-cc0468d40027"))
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

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

        btnListDatabases.Clicked += RefreshDatabaseList;
        btnListTables.Clicked += UpdateTableList;

        if (!showTableComponents)
        {
            lblSchema.Visible = false;
            tbSchema.Visible = false;
            lblTable.Visible = false;
            tbTable.Visible = false;
            rgTableType.Visible = false;
            btnListTables.Visible = false;
            btnOk.Y -= 5;
            btnCancel.Y -= 5;
            Height -= 5;
        }
    }

    private sealed class NoWordMeansShowAllAutocomplete : TextFieldAutocomplete
    {
        public NoWordMeansShowAllAutocomplete(TextField tb)
        {
            HostControl = tb;
            PopupInsideContainer = false;
        }

        public override void GenerateSuggestions(int columnOffset = 0)
        {
            // if there is something to pick
            if (AllSuggestions.Count > 0)
            {
                // and no current word
                var currentWord = GetCurrentWord(columnOffset);
                if (string.IsNullOrWhiteSpace(currentWord))
                {
                    Suggestions = AllSuggestions.AsReadOnly();
                    return;
                }
            }

            // otherwise let the default implementation run
            base.GenerateSuggestions();
        }
    }

    private static void AddNoWordMeansShowAllAutocomplete(TextField tb)
    {
        var prop = typeof(TextField).GetProperty(nameof(TextField.Autocomplete));
        prop?.SetValue(tb, new NoWordMeansShowAllAutocomplete(tb));

        tb.Autocomplete.MaxWidth = tb.Frame.Width;
    }

    private void UpdateTableList()
    {
        var open = new LoadingDialog("Fetching Tables...");
        List<string> tables = null;

        Task.Run(() =>
        {
            var db = new DiscoveredServer(GetBuilder()).ExpectDatabase(Database);
            tables = db.DiscoverTables(true).Union(db.DiscoverTableValuedFunctions())
                .Select(t => t.GetRuntimeName())
                .ToList();
        }).ContinueWith((t, o) =>
        {
            // no longer loading
            Application.MainLoop.Invoke(() => Application.RequestStop());

            if (t.Exception != null)
            {
                Application.MainLoop.Invoke(() =>
                    _activator.ShowException("Failed to list tables", t.Exception));
                return;
            }

            // if loaded correctly then
            if (tables != null)
                Application.MainLoop.Invoke(() =>
                    tbTable.Autocomplete.AllSuggestions = tables);
        }, TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, ConsoleMainWindow.ExceptionPopup);
    }

    private void RefreshDatabaseList()
    {
        var open = new LoadingDialog("Fetching Databases...");
        List<string> databases = null;

        Task.Run(() =>
        {
            var server = new DiscoveredServer(GetBuilder());
            databases = server.DiscoverDatabases()
                .Select(d => d.GetRuntimeName())
                .ToList();
        }).ContinueWith((t, o) =>
        {
            // no longer loading
            Application.MainLoop.Invoke(() => Application.RequestStop());

            if (t.Exception != null)
            {
                Application.MainLoop.Invoke(() =>
                    _activator.ShowException("Failed to list databases", t.Exception));
                return;
            }

            // if loaded correctly then
            if (databases != null)
                Application.MainLoop.Invoke(() =>
                    tbDatabase.Autocomplete.AllSuggestions = databases
                );
        }, TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, ConsoleMainWindow.ExceptionPopup);
    }

    public DbConnectionStringBuilder GetBuilder()
    {
        var helper = DatabaseCommandHelper.For(DatabaseType);
        return helper.GetConnectionStringBuilder(Server, Database, Username, Password);
    }

    public bool ShowDialog()
    {
        Application.Run(this, ConsoleMainWindow.ExceptionPopup);
        return OkClicked;
    }

    private void CreateDatabase()
    {
        var db = GetDiscoveredDatabase(true);

        if (db == null)
        {
            _activator.Show("Enter all database details before trying to create");
            return;
        }

        var open = new LoadingDialog($"Creating Database '{db}'");
        string message = null;

        Task.Run(() =>
        {
            if (db.Exists())
            {
                message = "Database already exists";
            }
            else
            {
                db.Create();
                message = "Database Created Successfully";
            }
        }).ContinueWith((t, o) =>
        {
            // no longer loading
            Application.MainLoop.Invoke(() => Application.RequestStop());

            if (t.Exception != null)
            {
                Application.MainLoop.Invoke(() =>
                    _activator.ShowException("Failed to create database", t.Exception));
                return;
            }

            // if loaded correctly then
            if (message != null)
                Application.MainLoop.Invoke(() =>
                    _activator.Show("Create Database", message));
        }, TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, ConsoleMainWindow.ExceptionPopup);
    }


    public DiscoveredDatabase GetDiscoveredDatabase(bool ignoreOk = false)
    {
        if (!OkClicked && !ignoreOk)
            return null;

        if (string.IsNullOrWhiteSpace(Server))
            return null;

        return string.IsNullOrWhiteSpace(Database)
            ? null
            : new DiscoveredServer(Server, Database, DatabaseType, Username, Password).ExpectDatabase(Database);
    }


    public DiscoveredTable GetDiscoveredTable()
    {
        if (!OkClicked)
            return null;

        if (string.IsNullOrWhiteSpace(Server))
            return null;

        if (string.IsNullOrWhiteSpace(Database))
            return null;

        return TableType == TableType.TableValuedFunction
            ? new DiscoveredServer(Server, Database, DatabaseType, Username, Password).ExpectDatabase(Database)
                .ExpectTableValuedFunction(Table, Schema)
            : new DiscoveredServer(Server, Database, DatabaseType, Username, Password).ExpectDatabase(Database)
                .ExpectTable(Table, Schema, TableType);
    }


    private void BtnPickCredentials_Clicked()
    {
        if (_activator == null) return;

        var creds = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();

        if (!creds.Any())
        {
            _activator.Show("You do not have any DataAccessCredentials configured");
            return;
        }

        var cred = (DataAccessCredentials)_activator.SelectOne("Select Credentials", creds);
        if (cred != null)
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