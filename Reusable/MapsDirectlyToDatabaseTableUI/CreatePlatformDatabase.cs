// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable.Versioning;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;


namespace MapsDirectlyToDatabaseTableUI
{
    /// <summary>
    /// Allows you to create a new managed database (e.g. Logging database, Catalogue Manager database etc).
    /// 
    /// <para>Enter a server and a database (and optionally a username and password).  If you specify a username / password these will be stored either in a user settings file 
    /// for tier 1 databases (Catalogue Manager / Data Export Manager) or as encrypted strings in the catalogue database for Tier 2-3 databases (See 
    /// PasswordEncryptionKeyLocationUI).  For a description of each tier of databases See PluginPatcherUI.</para>
    /// 
    /// <para>You will be shown the initial creation script for the database so you can see what is being created and make sure it matches your expectations.  The database
    /// will then be patched up to date with the current version of the RDMP.</para>
    /// </summary>
    public partial class CreatePlatformDatabase : Form
    {
        private readonly string _createSql;
        private readonly string _initialVersionNumber;
        private readonly SortedDictionary<string, Patch> _patches;
        private bool _completed = false;

        private Thread _tCreateDatabase;
        private bool _programaticClose;

        public string DatabaseConnectionString { get ; private set; }

        /// <summary>
        /// You should probably be using the other constructor
        /// </summary>
        /// <param name="createSql"></param>
        /// <param name="initialVersionNumber"></param>
        /// <param name="patches"></param>
        public CreatePlatformDatabase(string createSql, string initialVersionNumber, SortedDictionary<string, Patch> patches)
        {
            _createSql = createSql;
            _initialVersionNumber = initialVersionNumber;
            _patches = patches;
            InitializeComponent();

            new RecentHistoryOfControls(tbServer, new Guid("8c32e04c-f3c4-465d-8dbb-f180264b87f8"));
            new RecentHistoryOfControls(tbDatabase, new Guid("dee8d99f-625b-4e20-b63c-63613302e1bc"));
        }
        
        /// <summary>
        /// Calls the main constructor but passing control of what scripts to extract to the Patch class
        /// </summary>
        /// <param name="databaseAssembly">A database hosting assembly e.g. ANOStore.Database, use typeof(ANOStore.Database.Class1).Assembly to populate this parameter</param>
        public CreatePlatformDatabase(Assembly databaseAssembly)
            : this(Patch.GetInitialCreateScriptContents(databaseAssembly),
            "1.0.0.0", Patch.GetAllPatchesInAssembly(databaseAssembly))
        {
        }

        
        private void btnCreate_Click(object sender, EventArgs e)
        {
            var preview = new SQLPreviewWindow("Confirm happiness with SQL",
                "The following SQL is about to be executed:", _createSql);

            MasterDatabaseScriptExecutor executor = null;

            if (string.IsNullOrWhiteSpace(tbDatabase.Text) || string.IsNullOrWhiteSpace(tbServer.Text))
            {
                MessageBox.Show("You must specify both a Server and a Database");
                return;
            }
            else
                executor = new MasterDatabaseScriptExecutor(tbServer.Text, tbDatabase.Text, tbUsername.Text, tbPassword.Text);

            if (_completed)
            {
                MessageBox.Show("Setup completed already, review progress messages then close Form");
                return;
            }

            if (_tCreateDatabase != null && _tCreateDatabase.IsAlive)
            {
                MessageBox.Show("Setup already underaway, Thread State is:" + _tCreateDatabase.ThreadState);
                return;
            }

            if (preview.ShowDialog() == DialogResult.OK)
            {
                _tCreateDatabase = new Thread(
                    () =>
                    {
                        var memory = new ToMemoryCheckNotifier(checksUI1);

                        if (executor.CreateDatabase(_createSql, _initialVersionNumber, memory))
                        {
                            _completed = executor.PatchDatabase(_patches, memory, silentlyApplyPatchCallback);
                            GenerateConnectionStringThenCopy();

                            var worst = memory.GetWorst();
                            if(worst == CheckResult.Success || worst == CheckResult.Warning)
                                if (MessageBox.Show("Succesfully created database, close form?", "Success",MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    _programaticClose = true;
                                    Invoke(new MethodInvoker(Close));
                                }
                        }
                        else
                            _completed = false;//failed to create database
                    }
                    );
                _tCreateDatabase.Start();
            }
        }

        private bool silentlyApplyPatchCallback(Patch p)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("About to apply patch " + p.locationInAssembly, CheckResult.Success, null));
            return true;
        }
        
        private void CreatePlatformDatabase_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_tCreateDatabase != null)
            {
                if (_tCreateDatabase.ThreadState != ThreadState.Stopped && !_programaticClose)
                {
                    if(
                        MessageBox.Show("Thread state is " + _tCreateDatabase.ThreadState +
                                    ".  Are you sure you want to close the form? If you close the form your database may be left in a half finished state.","Really Close?",MessageBoxButtons.YesNoCancel) 
                        != DialogResult.Yes)
                            e.Cancel = true;
                }
            }
        }

       private void GenerateConnectionStringThenCopy()
        {
            SqlConnectionStringBuilder builder;
            if (!string.IsNullOrWhiteSpace(tbUsername.Text) || !string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                builder = new SqlConnectionStringBuilder()
                {
                    DataSource = tbServer.Text,
                    InitialCatalog = tbDatabase.Text,
                    UserID = tbUsername.Text,
                    Password = tbPassword.Text
                };
            }
            else
            {
                builder = new SqlConnectionStringBuilder()
                {
                    DataSource = tbServer.Text,
                    InitialCatalog = tbDatabase.Text,
                    IntegratedSecurity = true
                };
            }
            DatabaseConnectionString = builder.ConnectionString;
            
        }

        public static ExternalDatabaseServer CreateNewExternalServer(CatalogueRepository repository,PermissableDefaults defaultToSet, Assembly databaseAssembly)
        {

            CreatePlatformDatabase createPlatform = new CreatePlatformDatabase(databaseAssembly);
            createPlatform.ShowDialog();

            if (!string.IsNullOrWhiteSpace(createPlatform.DatabaseConnectionString))
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(createPlatform.DatabaseConnectionString);
                var newServer = new ExternalDatabaseServer(repository, builder.InitialCatalog, databaseAssembly);

                newServer.Server = builder.DataSource;
                newServer.Database = builder.InitialCatalog;

                //if there is a username/password
                if (!builder.IntegratedSecurity)
                {
                    newServer.Password = builder.Password;
                    newServer.Username = builder.UserID;
                }
                newServer.SaveToDatabase();
                
                if(defaultToSet != PermissableDefaults.None)
                    repository.GetServerDefaults().SetDefault(defaultToSet, newServer);

                return newServer;
            }

            return null;
        }
    }
}
