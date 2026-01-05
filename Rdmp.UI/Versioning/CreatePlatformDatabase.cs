// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.SimpleDialogs.SqlDialogs;


namespace Rdmp.UI.Versioning;

/// <summary>
/// Allows you to create a new managed database (e.g. Logging database, Catalogue Manager database etc).
/// 
/// <para>Enter a server and a database (and optionally a username and password).  If you specify a username / password these will be stored either in a user settings file
/// for tier 1 databases (Catalogue Manager / Data Export Manager) or as encrypted strings in the catalogue database for Tier 2-3 databases (See
/// PasswordEncryptionKeyLocationUI).</para>
/// 
/// <para>You will be shown the initial creation script for the database so you can see what is being created and make sure it matches your expectations.  The database
/// will then be patched up to date with the current version of the RDMP.</para>
/// </summary>
public partial class CreatePlatformDatabase : Form
{
    private bool _completed;

    private bool _programaticClose;
    private IPatcher _patcher;

    private Task _tCreateDatabase;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DiscoveredDatabase DatabaseCreatedIfAny { get; private set; }

    /// <summary>
    /// Calls the main constructor but passing control of what scripts to extract to the Patch class
    /// </summary>
    public CreatePlatformDatabase(IPatcher patcher)
    {
        _patcher = patcher;
        InitializeComponent();

        //show only Database section
        serverDatabaseTableSelector1.HideTableComponents();

        if (patcher.SqlServerOnly)
            serverDatabaseTableSelector1.LockDatabaseType(DatabaseType.MicrosoftSQLServer);
    }


    private void btnCreate_Click(object sender, EventArgs e)
    {
        var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

        if (db == null)
        {
            MessageBox.Show(
                "You must pick an empty database or enter the name of a new one");
            return;
        }

        if (_completed)
        {
            MessageBox.Show("Setup completed already, review progress messages then close Form");
            return;
        }

        if (_tCreateDatabase is { IsCompleted: false })
        {
            MessageBox.Show("Setup already underway");
            return;
        }

        var createSql = _patcher.GetInitialCreateScriptContents(db);
        var patches = _patcher.GetAllPatchesInAssembly(db);

        var preview = new SQLPreviewWindow("Confirm happiness with SQL",
            "The following SQL is about to be executed:", createSql.EntireScript);

        var executor = new MasterDatabaseScriptExecutor(db);

        if (preview.ShowDialog() == DialogResult.OK)
            _tCreateDatabase = Task.Run(() =>

                {
                    var memory = new ToMemoryCheckNotifier(checksUI1);

                    if (executor.CreateDatabase(createSql, memory))
                    {
                        _completed = executor.PatchDatabase(patches, memory, silentlyApplyPatchCallback);

                        DatabaseCreatedIfAny = db;

                        if (memory.GetWorst() is not (CheckResult.Success or CheckResult.Warning) ||
                            MessageBox.Show("Successfully created database, close form?", "Success",
                                MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                        _programaticClose = true;
                        Invoke(new MethodInvoker(Close));
                    }
                    else
                    {
                        _completed = false; //failed to create database
                    }
                }
            );
    }


    private bool silentlyApplyPatchCallback(Patch p)
    {
        checksUI1.OnCheckPerformed(new CheckEventArgs($"About to apply patch {p.locationInAssembly}",
            CheckResult.Success, null));
        return true;
    }

    private void CreatePlatformDatabase_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_tCreateDatabase != null)
            if (!_tCreateDatabase.IsCompleted && !_programaticClose)
                if (
                    MessageBox.Show(
                        "CreateDatabase Task is still running.  Are you sure you want to close the form? If you close the form your database may be left in a half finished state.",
                        "Really Close?", MessageBoxButtons.YesNoCancel)
                    != DialogResult.Yes)
                    e.Cancel = true;
    }

    public static ExternalDatabaseServer CreateNewExternalServer(ICatalogueRepository repository,
        PermissableDefaults defaultToSet, IPatcher patcher)
    {
        var createPlatform = new CreatePlatformDatabase(patcher);
        createPlatform.ShowDialog();

        var db = createPlatform.DatabaseCreatedIfAny;

        if (db != null)
        {
            var newServer = new ExternalDatabaseServer(repository, db.GetRuntimeName(), patcher);
            newServer.SetProperties(db);

            if (defaultToSet != PermissableDefaults.None)
                repository.SetDefault(defaultToSet, newServer);

            return newServer;
        }

        return null;
    }
}