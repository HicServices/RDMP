using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using roundhouse.consoles;
using roundhouse.cryptography;
using roundhouse.databases.sqlserver;
using roundhouse.environments;
using roundhouse.infrastructure;
using roundhouse.infrastructure.app;
using roundhouse.migrators;
using ConnectionType = roundhouse.infrastructure.app.ConnectionType;
using Version = System.Version;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    public class MasterDatabaseScriptExecutor
    {
        private readonly string _server;
        private readonly string _database;

        private const string RoundhouseSchemaName ="RoundhousE";
        private const string RoundhouseVersionTable = "Version";
        private const string RoundhouseScriptsRunTable = "ScriptsRun";
        private const string RoundhouseScriptErrorsTable = "ScriptsRunErrors";

        private readonly SqlConnectionStringBuilder _builder;
        private const string InitialDatabaseScriptName = @"Initial Database Setup";

        public MasterDatabaseScriptExecutor(string connectionString)
        {
            _builder = new SqlConnectionStringBuilder(connectionString);
            _server = _builder.DataSource;
            _database = _builder.InitialCatalog;
        }

        public MasterDatabaseScriptExecutor(string server, string database, string username, string password)
        {
            _server = server;
            _database = database;

            _builder = new SqlConnectionStringBuilder
            {
                DataSource = _server,
                InitialCatalog = _database,
                IntegratedSecurity = string.IsNullOrWhiteSpace(username),
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                _builder.UserID = username;
                _builder.Password = password;
            }
        }

        public MasterDatabaseScriptExecutor(DiscoveredServer server, string database):this(server.ExpectDatabase(database))
        {
            
        }

        public MasterDatabaseScriptExecutor(DiscoveredDatabase discoveredDatabase)
        {
            _builder = (SqlConnectionStringBuilder)discoveredDatabase.Server.Builder;
            _server = _builder.DataSource;
            _database = _builder.InitialCatalog;

            _builder = new SqlConnectionStringBuilder
            {
                DataSource = _server,
                InitialCatalog = _database,
                IntegratedSecurity = string.IsNullOrWhiteSpace(_builder.UserID),
            };

            if (!string.IsNullOrWhiteSpace(_builder.UserID))
            {
                _builder.UserID = _builder.UserID;
                _builder.Password = _builder.Password;
            }
        }

        public bool BinaryCollation { get; set; }

        public string CreateConnectionString(bool includeDatabaseInString = true)
        {
            if (!includeDatabaseInString)
            {
                var serverOnlyBuilder = new SqlConnectionStringBuilder(_builder.ConnectionString) {InitialCatalog = ""};
                return serverOnlyBuilder.ConnectionString;
            }

            return _builder.ConnectionString;
        }

        private DefaultDatabaseMigrator CreateDatabaseMigratorForConnection(SqlConnectionStringBuilder builder, string connectionString, ICheckNotifier notifier, out ConfigurationPropertyHolder propertyHolder)
        {
            ApplicationParameters.CurrentMappings.roundhouse_schema_name = RoundhouseSchemaName;
            ApplicationParameters.CurrentMappings.scripts_run_errors_table_name = RoundhouseScriptErrorsTable;
            ApplicationParameters.CurrentMappings.scripts_run_table_name = RoundhouseScriptsRunTable;
            ApplicationParameters.CurrentMappings.version_table_name = RoundhouseVersionTable;

            //Configuration setup
            propertyHolder = new DefaultConfiguration
            {
                DoNotCreateDatabase = false,
                DatabaseName = builder.InitialCatalog,
                DatabaseType = @"roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver",
                VersionTableName = RoundhouseVersionTable,
                ScriptsRunTableName = RoundhouseScriptsRunTable,
                ScriptsRunErrorsTableName = RoundhouseScriptErrorsTable,
                SchemaName = RoundhouseSchemaName,
                ConnectionStringAdmin = connectionString,
                ConnectionString = connectionString
            };

            //database target
            var s = new SqlServerDatabase
            {
                server_name = builder.DataSource,
                database_name = builder.InitialCatalog,
                roundhouse_schema_name = RoundhouseSchemaName,
                configuration = propertyHolder,
                admin_connection_string = connectionString,
                connection_string = connectionString,
                version_table_name = RoundhouseVersionTable,
                scripts_run_table_name = RoundhouseScriptsRunTable,
                scripts_run_errors_table_name = RoundhouseScriptErrorsTable

            };

            notifier.OnCheckPerformed(new CheckEventArgs("RoundhousE settings configured", CheckResult.Success, null));

            //migration to create the database 
            return new DefaultDatabaseMigrator(s, new MD5CryptographicService(), propertyHolder);
        }

        public bool CreateDatabase(string createTablesAndFunctionsSql, string initialVersionNumber, ICheckNotifier notifier)
        {
            try
            {
                // The _builder has InitialCatalog set which will cause the pre-database creation connection to fail, so create one which doesn't contain InitialCatalog
                var serverBuilder = new SqlConnectionStringBuilder(_builder.ConnectionString) { InitialCatalog = "" };

                DiscoveredServer server = new DiscoveredServer(serverBuilder);
                server.TestConnection();

                ConfigurationPropertyHolder propertyHolder;


                if (server.ExpectDatabase(_database).Exists())//make sure database does not already exist
                {
                    bool createAnyway = notifier.OnCheckPerformed(new CheckEventArgs("Database already exists", CheckResult.Warning, null,"Attempt to create database inside existing database (will cause problems if the database is not empty)?"));

                    if(!createAnyway)
                        throw new Exception("User chose not continue");
                }
                else
                {
                    using (var con = server.GetConnection())//do it manually 
                    {
                        con.Open();
                        server.GetCommand("CREATE DATABASE " + _database + (BinaryCollation?" COLLATE Latin1_General_BIN2":""), con).ExecuteNonQuery();
                        notifier.OnCheckPerformed(new CheckEventArgs("Database " + _database + " created", CheckResult.Success, null));
                    }
                }
                
                SqlConnection.ClearAllPools();

                var migratorForTableCreation = CreateDatabaseMigratorForConnection(_builder, _builder.ConnectionString, notifier, out propertyHolder);
                migratorForTableCreation.initialize_connections();
                migratorForTableCreation.open_connection(false);
                migratorForTableCreation.run_roundhouse_support_tasks();
                migratorForTableCreation.close_connection();
                notifier.OnCheckPerformed(new CheckEventArgs("run_roundhouse_support_tasks called", CheckResult.Success, null));
                
                var environment = new DefaultEnvironment(propertyHolder);
                if (!migratorForTableCreation.run_sql(createTablesAndFunctionsSql, InitialDatabaseScriptName, true, false, 1, environment, initialVersionNumber, "Custom", ConnectionType.Default))
                    throw new Exception("run_sql did not succeed (don't know where to get any further information...). Connection string = " + _builder.ConnectionString);
                migratorForTableCreation.version_the_database("Initial Setup", initialVersionNumber);
                notifier.OnCheckPerformed(new CheckEventArgs("Tables created", CheckResult.Success, null));

                notifier.OnCheckPerformed(new CheckEventArgs("Setup Completed successfully", CheckResult.Success, null));

                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Create failed", CheckResult.Fail, e));
                return false;
            }
        }

        public bool PatchDatabase(SortedDictionary<string, Patch> patches, ICheckNotifier notifier, Func<Patch, bool> patchPreviewShouldIRunIt)
        {
            ConfigurationPropertyHolder configurationPropertyHolder;

            if(!patches.Any())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("There are no patches to apply so skipping patching", CheckResult.Success,null));
                return true;
            }

            Version maxPatchVersion = patches.Values.Max(pat => pat.DatabaseVersionNumber);

            DefaultDatabaseMigrator migrator;
            try
            {
                migrator = CreateDatabaseMigratorForConnection(_builder, _builder.ConnectionString, notifier, out configurationPropertyHolder);
                
                notifier.OnCheckPerformed(new CheckEventArgs("About to backup database", CheckResult.Success, null));

                UsefulStuff.BackupSqlServerDatabase(CreateConnectionString(),_database, "Full backup of " + _database);
            
                notifier.OnCheckPerformed(new CheckEventArgs("Database backed up", CheckResult.Success, null));

                migrator.initialize_connections();
                migrator.open_connection(true);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Patching failed during setup and preparation (includes failures due to backup creation failures)",
                    CheckResult.Fail, e));
                return false;
            }

            try
            {
                int i = 0;
                foreach (KeyValuePair<string, Patch> patch in patches)
                {
                    i++;

                    bool shouldRun = patchPreviewShouldIRunIt(patch.Value);

                    if (shouldRun)
                    {

                        migrator.run_sql(patch.Value.EntireScript, patch.Key, true, false, i, new DefaultEnvironment(configurationPropertyHolder), "1.0.0.0",patch.Key, ConnectionType.Default);

                        notifier.OnCheckPerformed(new CheckEventArgs("Executed patch " + patch.Value, CheckResult.Success, null));
                    }
                    else
                        throw new Exception("User decided not to execute patch " + patch.Key + " aborting ");
                }

                UpdateVersionIncludingClearingLastVersion(migrator,notifier,maxPatchVersion);
                
                //all went fine
                migrator.close_connection();
                notifier.OnCheckPerformed(new CheckEventArgs("All Patches applied, transaction committed", CheckResult.Success, null));
                
                return true;

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Error occurred during patching", CheckResult.Fail, e));
                return false;
            }
        }

        private void UpdateVersionIncludingClearingLastVersion(DefaultDatabaseMigrator migrator, ICheckNotifier notifier, Version maxPatchVersion)
        {
            try
            {
                SqlConnection con = new SqlConnection(_builder.ConnectionString);
                con.Open();
                SqlCommand cmdClear = new SqlCommand("Delete from RoundhousE.Version", con);
                cmdClear.ExecuteNonQuery();
                con.Close();
                notifier.OnCheckPerformed(new CheckEventArgs("successfully deleted old Version number from RoundhousE.Version",CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not clear previous version history (but will continue with versioning anyway) ",
                    CheckResult.Fail, e));
            }
            //increment the version number if there were any patches
            migrator.version_the_database("Patching", maxPatchVersion.ToString());
            notifier.OnCheckPerformed(new CheckEventArgs("Updated database version to " + maxPatchVersion.ToString(), CheckResult.Success, null));
                
        }


        public Patch[] GetPatchesRun()
        { 
            List<Patch> toReturn = new List<Patch>();
            
            using (var con = new SqlConnection(CreateConnectionString()))
            {
                
                con.Open();

                SqlCommand cmd = new SqlCommand("Select * from " +RoundhouseSchemaName +"."+ RoundhouseScriptsRunTable, con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    string text_of_script = (string)r["text_of_script"];
                    string script_name = (string)r["script_name"] ;

                    if(script_name.Equals(InitialDatabaseScriptName))
                        continue;

                    Patch p = new Patch(script_name,text_of_script);
                    toReturn.Add(p);
                }
                
                con.Close();
            }
            return toReturn.ToArray();
        }

        /// <summary>
        /// Creates a new platform database and patches it
        /// </summary>
        /// <param name="hostAssembly">The HOST assembly (not the databas assembly) e.g. if you want to create and patch CatalogueLibrary.Database then pass in typeof(Catalogue).Assembly instead</param>
        /// <param name="notifier">audit object, can be a new ThrowImmediatelyCheckNotifier if you aren't in a position to pass one</param>
        public void CreateAndPatchDatabase(Assembly hostAssembly, ICheckNotifier notifier)
        {
            var databaseAssembly = GetDatabaseAssemblyForHost(hostAssembly);
            string sql = Patch.GetInitialCreateScriptContents(databaseAssembly);

            //get everything in the /up/ folder that are .sql
            var patches = Patch.GetAllPatchesInAssembly(databaseAssembly);

            CreateDatabase(sql, "1.0.0.0", notifier);
            PatchDatabase(patches,notifier,(p)=>true);//apply all patches without question
        }
        /// <summary>
        /// Creates a new platform database and patches it
        /// </summary>
        /// <param name="notifier">audit object, can be a new ThrowImmediatelyCheckNotifier if you aren't in a position to pass one</param>
        public void CreateAndPatchDatabaseWithDotDatabaseAssembly(Assembly dotDatabaseAssembly, ICheckNotifier notifier)
        {
            string sql = Patch.GetInitialCreateScriptContents(dotDatabaseAssembly);

            //get everything in the /up/ folder that are .sql
            var patches = Patch.GetAllPatchesInAssembly(dotDatabaseAssembly);

            CreateDatabase(sql, "1.0.0.0", notifier);
            PatchDatabase(patches, notifier, (p) => true);//apply all patches without question
        }
        /// <summary>
        /// Gets the dll called MyAssembly.Database.dll when passed the assembly MyAssembly.dll for this to work MyAssembly.dll must have a reference to MyAssembly.Database.dll and use it
        /// so that it gets compiled and included wherever MyAssembly.dll is used
        /// </summary>
        /// <param name="hostAssembly"></param>
        /// <returns></returns>
        private Assembly GetDatabaseAssemblyForHost(Assembly hostAssembly)
        {
            string expectedDatabaseDllName = hostAssembly.GetName().Name + ".Database";
            var databaseAssembly = Assembly.Load(expectedDatabaseDllName);

            if (databaseAssembly == null)
                throw new Exception("Expected the passed host assembly " + hostAssembly.FullName + " to have refernced assembly " + expectedDatabaseDllName + " containing embedded database scripts but Assembly.Load returned null indicating it is not in scope");

            return databaseAssembly;
        }
    }


    
}
