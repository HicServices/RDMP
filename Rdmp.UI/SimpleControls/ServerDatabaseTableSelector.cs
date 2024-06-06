// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.SimpleDialogs;
using YamlDotNet.Core.Tokens;

namespace Rdmp.UI.SimpleControls;

public delegate void IntegratedSecurityUseChangedHandler(bool use);

/// <summary>
/// Lets you select a server database or table.  Includes auto population of database/table lists.  This is a reusable component.
/// </summary>
public partial class ServerDatabaseTableSelector : UserControl
{
    private bool _allowTableValuedFunctionSelection;

    public string Server
    {
        get => cbxServer.Text;
        set => cbxServer.Text = value;
    }

    public string Database
    {
        get => cbxDatabase.Text;
        set => cbxDatabase.Text = value;
    }

    public string Table
    {
        private get => cbxTable.Text;
        set => cbxTable.Text = value;
    }

    public string Username
    {
        get => tbUsername.Text;
        set => tbUsername.Text = value;
    }

    public string Password
    {
        get => tbPassword.Text;
        set => tbPassword.Text = value;
    }

    public string Timeout
    {
        get => tbTimeout.Text;
        set => tbTimeout.Text = value;
    }

    private string TableValuedFunction => cbxTableValueFunctions.Text;

    public event Action SelectionChanged;
    private IDiscoveredServerHelper _helper;

    private BackgroundWorker _workerRefreshDatabases = new();
    private CancellationTokenSource _workerRefreshDatabasesToken;
    private string[] _listDatabasesAsyncResult;

    private BackgroundWorker _workerRefreshTables = new();
    private CancellationTokenSource _workerRefreshTablesToken;
    private List<DiscoveredTable> _listTablesAsyncResult;

    private const string CancelConnection = "Cancel Connection";
    private const string ConnectionFailed = "Connection Failed";

    //constructor
    public ServerDatabaseTableSelector()
    {
        InitializeComponent();

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return;

        _workerRefreshDatabases.DoWork += UpdateDatabaseListAsync;
        _workerRefreshDatabases.WorkerSupportsCancellation = true;
        _workerRefreshDatabases.RunWorkerCompleted += UpdateDatabaseAsyncCompleted;

        _workerRefreshTables.DoWork += UpdateTablesListAsync;
        _workerRefreshTables.WorkerSupportsCancellation = true;
        _workerRefreshTables.RunWorkerCompleted += UpdateTablesAsyncCompleted;

        var r = new RecentHistoryOfControls(cbxServer, new Guid("01ccc304-0686-4145-86a5-cc0468d40027"));
        RecentHistoryOfControls.AddHistoryAsItemsToComboBox(cbxServer);

        var r2 = new RecentHistoryOfControls(cbxDatabase, new Guid("e1a4e7a8-3f7a-4018-8ff5-2fd661ee06a3"));
        RecentHistoryOfControls.AddHistoryAsItemsToComboBox(cbxDatabase);

        _helper = DatabaseCommandHelper.For(DatabaseType);

        btnPickCredentials.Image = CatalogueIcons.DataAccessCredentials.ImageToBitmap();
        btnPickCredentials.Enabled = false;
    }


    #region Async Stuff

    private void UpdateTablesListAsync(object sender, DoWorkEventArgs e)
    {
        var builder = (DbConnectionStringBuilder)((object[])e.Argument)[0];
        if (!string.IsNullOrWhiteSpace(Timeout) && int.TryParse(Timeout, out var _timeout))
        {
            builder["Timeout"] = _timeout;
        }

        var database = (string)((object[])e.Argument)[1];

        var discoveredDatabase = new DiscoveredServer(builder).ExpectDatabase(database);
        var databaseHelper = discoveredDatabase.Helper;

        _workerRefreshTablesToken = new CancellationTokenSource();

        var syntaxHelper = discoveredDatabase.Server.GetQuerySyntaxHelper();
        try
        {
            using var con = discoveredDatabase.Server.GetConnection();
            var openTask = con.OpenAsync(_workerRefreshTablesToken.Token);
            openTask.Wait(_workerRefreshTablesToken.Token);

            var result = new List<DiscoveredTable>();

            result.AddRange(databaseHelper.ListTables(discoveredDatabase, syntaxHelper, con, database, true));
            result.AddRange(databaseHelper.ListTableValuedFunctions(discoveredDatabase, syntaxHelper, con, database));

            _listTablesAsyncResult = result;
        }
        catch (OperationCanceledException) //user cancels
        {
            _listTablesAsyncResult = new List<DiscoveredTable>();
        }
    }

    private void UpdateTablesAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        //success?
        ResetState();

        if (e.Error != null)
        {
            SetState(e.Error);
        }
        else if (!e.Cancelled)
        {
            cbxTable.Items.AddRange(_listTablesAsyncResult.Where(static t => t is not DiscoveredTableValuedFunction)
                .ToArray());
            cbxTableValueFunctions.Items.AddRange(_listTablesAsyncResult
                .Where(static t => t is DiscoveredTableValuedFunction).ToArray());
        }


        SetLoading(false);

        cbxTable.Focus();
    }


    //do work
    private void UpdateDatabaseListAsync(object sender, DoWorkEventArgs e)
    {
        var builder = (DbConnectionStringBuilder)((object[])e.Argument)[0];
        if (!string.IsNullOrWhiteSpace(Timeout) && int.TryParse(Timeout, out var _timeout))
        {
            builder["Timeout"] = _timeout;
        }
        else
        {
            builder["Timeout"] = 30000;
        }
        ResetState();

        _workerRefreshDatabasesToken = new CancellationTokenSource();
        try
        {
            using var con = _helper.GetConnection(builder);
            var openTask = con.OpenAsync(_workerRefreshDatabasesToken.Token);
            openTask.Wait(_workerRefreshDatabasesToken.Token);
            _listDatabasesAsyncResult =  _helper.ListDatabases(con).ToAsyncEnumerable().ToBlockingEnumerable(_workerRefreshDatabasesToken.Token).ToArray();
        }
        catch (OperationCanceledException)
        {
            _listDatabasesAsyncResult = Array.Empty<string>();
        }
        catch (AggregateException ex) //user cancels
        {
            if (ex.GetExceptionIfExists<OperationCanceledException>() != null)
            {
                _listDatabasesAsyncResult = Array.Empty<string>();
            }
            else
            {
                SetState(ex);
                _listDatabasesAsyncResult = Array.Empty<string>();
            }
        }
    }

    private void ResetState()
    {
        _exception = null;
        SetText(CancelConnection);
    }

    private void SetState(Exception ex)
    {
        _exception = ex;
        SetText(ConnectionFailed);
    }

    private void SetText(string v)
    {
        if (llLoading.InvokeRequired)
            llLoading.Invoke(new MethodInvoker(() => llLoading.Text = v));
        else
            llLoading.Text = v;
    }

    //handle complete
    private void UpdateDatabaseAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
            SetState(e.Error);
        else if (!e.Cancelled)
            cbxDatabase.Items.AddRange(_listDatabasesAsyncResult);
        else
            // user cancelled
            ResetState();

        SetLoading(false);
        cbxDatabase.Focus();
    }

    //aborting
    private void AbortWorkers()
    {
        if (_workerRefreshDatabases.IsBusy)
        {
            _workerRefreshDatabases.CancelAsync();
            _workerRefreshDatabasesToken?.Cancel();
        }

        if (_workerRefreshTables.IsBusy)
        {
            _workerRefreshTables.CancelAsync();
            _workerRefreshTablesToken?.Cancel();
        }
    }

    #endregion

    public void SetDefaultServers(string[] defaultServers)
    {
        cbxServer.Items.AddRange(defaultServers);
    }

    public bool AllowTableValuedFunctionSelection
    {
        get => _allowTableValuedFunctionSelection;
        set
        {
            _allowTableValuedFunctionSelection = value;

            lblOr.Visible = value;
            lblTableValuedFunction.Visible = value;
            cbxTableValueFunctions.Visible = value;
        }
    }

    public DatabaseType DatabaseType
    {
        get => databaseTypeUI1.DatabaseType;
        set => databaseTypeUI1.DatabaseType = value;
    }

    public DiscoveredServer Result => new(GetBuilder());

    public bool TableShouldBeNovel
    {
        set
        {
            cbxTable.AutoCompleteMode = value ? AutoCompleteMode.None : AutoCompleteMode.Suggest;
            cbxTableValueFunctions.AutoCompleteMode = value ? AutoCompleteMode.None : AutoCompleteMode.Suggest;
        }
    }

    private void cbxServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if they have not selected anything selected something
        if (string.IsNullOrWhiteSpace(cbxServer.Text))
            return;

        UpdateDatabaseList();
    }

    private void cbxDatabase_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateTableList();
    }

    private bool _clearingTable;

    private void cbxTable_SelectedIndexChanged(object sender, EventArgs e)
    {
        //don't clear both!
        if (_clearingTable)
            return;
        _clearingTable = true;

        cbxTableValueFunctions.Text = null;
        SelectionChanged?.Invoke();

        _clearingTable = false;
    }

    private void cbxTableValueFunctions_SelectedIndexChanged(object sender, EventArgs e)
    {
        //don't clear both!
        if (_clearingTable)
            return;

        _clearingTable = true;
        cbxTable.Text = null;
        SelectionChanged?.Invoke();

        _clearingTable = false;
    }

    public void HideTableComponents()
    {
        AllowTableValuedFunctionSelection = false;
        cbxTable.Visible = false;
        lblTable.Visible = false;
        btnRefreshTables.Visible = false;
    }

    private void cbxServer_Leave(object sender, EventArgs e)
    {
        UpdateDatabaseList();
    }

    private void UpdateTableList()
    {
        if (string.IsNullOrWhiteSpace(cbxServer.Text) || string.IsNullOrWhiteSpace(cbxDatabase.Text))
            return;

        SelectionChanged?.Invoke();

        AbortWorkers();

        cbxTable.Items.Clear();
        cbxTableValueFunctions.Items.Clear();

        SetLoading(true);

        if (!_workerRefreshTables.IsBusy)
            _workerRefreshTables.RunWorkerAsync(new object[]
            {
                GetBuilder(),
                cbxDatabase.Text
            });
    }

    public void SetItemActivator(IBasicActivateItems activator)
    {
        _activator = activator;
        btnPickCredentials.Enabled = _activator != null;
    }

    private void UpdateDatabaseList()
    {
        if (string.IsNullOrWhiteSpace(cbxServer.Text) || _helper == null)
            return;

        AbortWorkers();
        cbxDatabase.Items.Clear();

        SetLoading(true);

        SelectionChanged?.Invoke();


        if (!_workerRefreshDatabases.IsBusy)
            _workerRefreshDatabases.RunWorkerAsync(new object[]
            {
                GetBuilder()
            });
    }

    private void SetLoading(bool isLoading)
    {
        llLoading.Visible = isLoading || _exception != null;
        pbLoading.Visible = isLoading;

        cbxServer.Enabled = !isLoading;
        cbxDatabase.Enabled = !isLoading;
        databaseTypeUI1.Enabled = !isLoading;
        btnRefreshDatabases.Enabled = !isLoading;
        btnRefreshTables.Enabled = !isLoading;
    }

    public event IntegratedSecurityUseChangedHandler IntegratedSecurityUseChanged;

    private string oldUsername;
    private IBasicActivateItems _activator;
    private Exception _exception;

    private void tbUsername_TextChanged(object sender, EventArgs e)
    {
        //if the last value they typed was blank and the new value is not blank
        if (string.IsNullOrWhiteSpace(oldUsername) && !string.IsNullOrWhiteSpace(Username))
            IntegratedSecurityUseChanged?.Invoke(false);

        //if the last value they typed was NOT blank and it is now blank
        if (!string.IsNullOrWhiteSpace(oldUsername) && string.IsNullOrWhiteSpace(Username))
            IntegratedSecurityUseChanged?.Invoke(true);

        oldUsername = tbUsername.Text;

        SelectionChanged?.Invoke();
    }

    private void tbPassword_TextChanged(object sender, EventArgs e)
    {
        SelectionChanged?.Invoke();
    }

    public DiscoveredDatabase GetDiscoveredDatabase()
    {
        if (string.IsNullOrWhiteSpace(cbxServer.Text))
            return null;

        return string.IsNullOrWhiteSpace(cbxDatabase.Text)
            ? null
            : new DiscoveredServer(GetBuilder()).ExpectDatabase(cbxDatabase.Text);
    }

    public DiscoveredTable GetDiscoveredTable()
    {
        //if user selected a specific object from the drop down properly

        if (cbxTable.SelectedItem is DiscoveredTable tbl)
            return tbl;

        if (cbxTableValueFunctions.SelectedItem is DiscoveredTableValuedFunction tblValuedFunction)
            return tblValuedFunction;

        //Did they at least pick a database
        var db = GetDiscoveredDatabase();

        if (db == null)
            return null;

        //They made up a table that may or may not exist
        if (!string.IsNullOrWhiteSpace(Table))
            return db.ExpectTable(Table);

        //They made up a table valued function which may or may not exist
        if (!string.IsNullOrWhiteSpace(TableValuedFunction))
            return db.ExpectTableValuedFunction(TableValuedFunction);

        //they haven't entered anything
        return null;
    }

    public DbConnectionStringBuilder GetBuilder() =>
        _helper.GetConnectionStringBuilder(cbxServer.Text, cbxDatabase.Text, tbUsername.Text, tbPassword.Text);

    private void llLoading_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (llLoading.Text == CancelConnection)
        {
            AbortWorkers();
        }
        else
        {
            if (_exception != null) ExceptionViewer.Show(_exception);
        }
    }

    private void btnRefreshDatabases_Click(object sender, EventArgs e)
    {
        cbxDatabase.Text = "";
        UpdateDatabaseList();
    }

    private void btnRefreshTables_Click(object sender, EventArgs e)
    {
        UpdateTableList();
    }

    public void SetExplicitServer(string serverName)
    {
        cbxServer.Text = serverName;
        UpdateDatabaseList();
    }

    public void SetExplicitDatabase(string serverName, string databaseName)
    {
        cbxServer.Text = serverName;
        UpdateDatabaseList();
        cbxDatabase.Text = databaseName;
    }

    public void SetExplicitDatabase(string serverName, string databaseName, string username, string password)
    {
        tbUsername.Text = username;
        tbPassword.Text = password;
        cbxServer.Text = serverName;
        cbxDatabase.Text = databaseName;
    }

    private void cbxDatabase_TextChanged(object sender, EventArgs e)
    {
        SelectionChanged?.Invoke();
    }

    private void databaseTypeUI1_DatabaseTypeChanged(object sender, EventArgs e)
    {
        _helper = DatabaseCommandHelper.For(DatabaseType);
        UpdateDatabaseList();
    }

    public void LockDatabaseType(DatabaseType databaseType)
    {
        databaseTypeUI1.LockDatabaseType(databaseType);
    }

    private void btnPickCredentials_Click(object sender, EventArgs e)
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

    private void label3_Click(object sender, EventArgs e)
    {

    }
}