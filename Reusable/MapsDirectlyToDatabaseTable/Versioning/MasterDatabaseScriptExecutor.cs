using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Version = System.Version;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <summary>
    /// Deploys a .Database assembly (e.g. CatalogueLibrary.Database) into a database server (e.g. localhost\sqlexpress).  .Database assemblies are just lists
    /// of SQL scripts for creating and patching a specific schema.  This class wraps roundhouse for the executing of the scripts and the creation of the 
    /// ScriptsRun and Version tables which are used to ensure that the host assembly (e.g. CatalogueLibrary) version matches the current database version. 
    /// </summary>
    public class MasterDatabaseScriptExecutor
    {
        private readonly string _server;
        private readonly string _database;

        private const string RoundhouseSchemaName ="RoundhousE";
        private const string RoundhouseVersionTable = "Version";
        private const string RoundhouseScriptsRunTable = "ScriptsRun";

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

        public bool CreateDatabase(string createTablesAndFunctionsSql, string initialVersionNumber, ICheckNotifier notifier)
        {
            try
            {
                // The _builder has InitialCatalog set which will cause the pre-database creation connection to fail, so create one which doesn't contain InitialCatalog
                var serverBuilder = new SqlConnectionStringBuilder(_builder.ConnectionString) { InitialCatalog = "" };

                DiscoveredServer server = new DiscoveredServer(serverBuilder);
                server.TestConnection();

                var db = server.ExpectDatabase(_database);

                if (db.Exists())//make sure database does not already exist
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

                using (var con = db.Server.GetConnection())
                {
                    con.Open();

                    var cmd =  db.Server.GetCommand("CREATE SCHEMA " + RoundhouseSchemaName, con);
                    cmd.ExecuteNonQuery();

                    var sql = 
                    @"CREATE TABLE [RoundhousE].[ScriptsRun](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[version_id] [bigint] NULL,
	[script_name] [nvarchar](255) NULL,
	[text_of_script] [text] NULL,
	[text_hash] [nvarchar](512) NULL,
	[one_time_script] [bit] NULL,
	[entry_date] [datetime] NULL,
	[modified_date] [datetime] NULL,
	[entered_by] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)

CREATE TABLE [RoundhousE].[Version](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[repository_path] [nvarchar](255) NULL,
	[version] [nvarchar](50) NULL,
	[entry_date] [datetime] NULL,
	[modified_date] [datetime] NULL,
	[entered_by] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)
";

                    var cmd2 = db.Server.GetCommand(sql, con);
                    cmd2.ExecuteNonQuery();
                }

                RunSQL(db, createTablesAndFunctionsSql, InitialDatabaseScriptName);

                SetVersion(db,"Initial Setup", initialVersionNumber);

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

        private void RunSQL(DiscoveredDatabase db, string sql, string filename)
        {
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                UsefulStuff.ExecuteBatchNonQuery(sql, con);

                string insert = @"
INSERT INTO [RoundhousE].[ScriptsRun]
           ([script_name],
           [text_of_script],
           [text_hash],
           [one_time_script],
           [entry_date],
           [modified_date],
           [entered_by])
     VALUES
          (@script_name,
           @text_of_script,
           @text_hash,
           @one_time_script,
           @entry_date,
           @modified_date,
           @entered_by)
";

                DateTime dt = DateTime.Now;

                var cmd2 = db.Server.GetCommand(insert, con);

                db.Server.AddParameterWithValueToCommand("@script_name",cmd2,filename);
                db.Server.AddParameterWithValueToCommand("@text_of_script",cmd2,sql);
                db.Server.AddParameterWithValueToCommand("@text_hash",cmd2,CalculateMD5Hash(sql));
                db.Server.AddParameterWithValueToCommand("@one_time_script",cmd2,1);
                db.Server.AddParameterWithValueToCommand("@entry_date",cmd2,dt);
                db.Server.AddParameterWithValueToCommand("@modified_date",cmd2,dt);
                db.Server.AddParameterWithValueToCommand("@entered_by",cmd2,Environment.UserName);

                cmd2.ExecuteNonQuery();
            }

        }
        
        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input

            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);


            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(i.ToString("X2"));

            return sb.ToString();

        }



        private void SetVersion(DiscoveredDatabase db, string name, string version)
        {
            var versionTable = db.ExpectTable(RoundhouseVersionTable,RoundhouseSchemaName);
            versionTable.Truncate();

            //repository_path	version	entry_date	modified_date	entered_by
            //Patching	2.6.0.1	2018-02-05 08:26:54.000	2018-02-05 08:26:54.000	DUNDEE\TZNind
            
            using(var con =  db.Server.GetConnection())
            {
                con.Open();

                var sql = "INSERT INTO " + versionTable.GetFullyQualifiedName() +
                          "(repository_path,version,entry_date,modified_date,entered_by) VALUES (@repository_path,@version,@entry_date,@modified_date,@entered_by)";


                var cmd = db.Server.GetCommand(sql, con);

                var dt = DateTime.Now;

                db.Server.AddParameterWithValueToCommand("@repository_path", cmd, name);
                db.Server.AddParameterWithValueToCommand("@version",cmd,version);
                db.Server.AddParameterWithValueToCommand("@entry_date",cmd, dt);
                db.Server.AddParameterWithValueToCommand("@modified_date",cmd,dt);
                db.Server.AddParameterWithValueToCommand("@entered_by", cmd, Environment.UserName);

                cmd.ExecuteNonQuery();
            }
                
        }

        public bool PatchDatabase(SortedDictionary<string, Patch> patches, ICheckNotifier notifier, Func<Patch, bool> patchPreviewShouldIRunIt)
        {
            if(!patches.Any())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("There are no patches to apply so skipping patching", CheckResult.Success,null));
                return true;
            }

            Version maxPatchVersion = patches.Values.Max(pat => pat.DatabaseVersionNumber);

            var db = new DiscoveredServer(_builder).GetCurrentDatabase();

            try
            {
                notifier.OnCheckPerformed(new CheckEventArgs("About to backup database", CheckResult.Success, null));

                db.CreateBackup("Full backup of " + _database);
            
                notifier.OnCheckPerformed(new CheckEventArgs("Database backed up", CheckResult.Success, null));
                
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

                        RunSQL(db,patch.Value.EntireScript, patch.Key);

                        notifier.OnCheckPerformed(new CheckEventArgs("Executed patch " + patch.Value, CheckResult.Success, null));
                    }
                    else
                        throw new Exception("User decided not to execute patch " + patch.Key + " aborting ");
                }

                UpdateVersionIncludingClearingLastVersion(db,notifier,maxPatchVersion);
                
                //all went fine
                notifier.OnCheckPerformed(new CheckEventArgs("All Patches applied, transaction committed", CheckResult.Success, null));
                
                return true;

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Error occurred during patching", CheckResult.Fail, e));
                return false;
            }
        }

        private void UpdateVersionIncludingClearingLastVersion(DiscoveredDatabase db,ICheckNotifier notifier, Version maxPatchVersion)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_builder.ConnectionString))
                {
                    con.Open();
                    SqlCommand cmdClear = new SqlCommand("Delete from RoundhousE.Version", con);
                    cmdClear.ExecuteNonQuery();
                    con.Close();
                    notifier.OnCheckPerformed(new CheckEventArgs("successfully deleted old Version number from RoundhousE.Version", CheckResult.Success, null));
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not clear previous version history (but will continue with versioning anyway) ",
                    CheckResult.Fail, e));
            }
            //increment the version number if there were any patches
            SetVersion(db,"Patching", maxPatchVersion.ToString());
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
        /// <param name="dotDatabaseAssembly"></param>
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
