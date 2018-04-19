using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// This entity is a DLL (Dynamic Link Library - AKA Assembly) of compiled C# code that is either a MEF (Managed Extensibility Framework) plugin or a dependency of a MEF
    /// plugin.  Plugins add third party extension functionality (not part of the core RDMP functionality).  You can commit your compiled dlls by packaging them with 
    /// package.bat (or by zipping up your bin directory files) and committing the .zip via PluginManagementForm (Accessible via Ctrl+R).  PluginManagementForm will upload
    /// the DLL as a binary and pushed into the LoadModuleAssembly table.  This allows everyone using your Catalogue database access to the [Exports] defined in the compiled dll.
    /// 
    /// <para>A typical use case for this is when you are required to load a particularly freaky data format (e.g. even records are in UTF8 binary and odd records are in ASCII) which
    /// requires specific code to execute.  You would make a class for dealing with the file format and make it implement IPluginAttacher.  Upload your dll along with any
    /// dependency dlls and the next time a DataAnalyst is building a load configuration your attacher will be displayed along with all the 'out of the box' attachers (CSV, Excel etc)</para>
    /// </summary>
    public class LoadModuleAssembly : VersionedDatabaseEntity
    {
        //also prohibited are anything with Test in the title (the captial T is needed i.e. detest.dll would not be excluded but dTest.dll would be)
        public static readonly string[] ProhibitedDllNames = new []
        {
            //part of main Platform
            "CatalogueLibrary.dll",
            "CatalogueLibrary.Database.dll",
            "CatalogueManager.dll",
            "DataLoadEngine.dll",
            "DataExportLibrary.Interfaces.dll",
            "DataExportLibrary.dll",
            "DataExportManager.Database.dll",
            "DataLoadEngine.ANO.Database.dll",
            "ANOStore.dll",
            "ANOStore.Database.dll",
            "HIC.Logging.dll",
            "HIC.Logging.Database.dll",
            "ValidationUtilities.dll",
            "CommitAssembly.dll",
            "ReusableLibraryCode.dll",
            "LoadModules.Generic.dll",
            "LoadModules.GenericUIs.dll",
            "CohortManagerLibrary.dll",
            "DataQualityEngine.dll",
            "DataQualityEngine.Database.dll",
            "Diagnostics.dll",
            "IdentifierDump.Database.dll",
            "IdentifierDump.dll",
            "RDMPObjectVisualisation.dll",
            "CachingEngine.dll",
            "HIC.RDMP.Plugin.dll",

            "MapsDirectlyToDatabaseTable.dll",
            "ReusableUIComponents.dll",
            "Ticketing.dll",

            // Validation utilities
            "ValidationUtilitiesIntegrationTests.dll",
            "ValidationUtilitiesScenarioTests.dll",
            "ValidationUtilitiesTestConstants.dll",
            "ValidationUtilitiesUnitTests.dll",

            // SMO
            "Microsoft.SqlServer.ConnectionInfo.dll",
            "Microsoft.SqlServer.Management.Sdk.Sfc.dll",
            "Microsoft.SqlServer.Smo.dll",
            "Microsoft.SqlServer.SmoExtended.dll",

            // Third-party bits and bobs
            "ObjectListView.dll",
            "ICSharpCode.SharpZipLib.dll",
            "SciLexer.dll",
            "SciLexer64.dll",
            "ScintillaNET.dll",
            "DiffieHellman.dll",
            "Renci.SshNet.dll",
            "CsvHelper.dll", // CatalogueManager and others

            // Catalogue Manager UI refs
            "GraphX.Controls.dll",
            "GraphX.PCL.Common.dll",
            "GraphX.PCL.Logic.dll",
            "QuickGraph.Data.dll",
            "QuickGraph.dll",
            "QuickGraph.Graphviz.dll",
            "QuickGraph.Serialization.dll",

            //already part of Generic load modules
            "Ionic.Zip.dll",
            "Tamir.SharpSSH.dll",
            "NLog.dll",

            //part of testing
            "Tests.dll",
            "nunit.framework.dll",
            "CatalogueLibraryTests.dll",
            "DataLoadEngineTests.dll",
            "Tests.Common.dll",
            "DataQualityEngine.Tests.dll",
            "HIC.Logging.Tests.dll",
            "Rhino.Mocks.dll",
            "AnonymisationTests.dll",
            "CachingEngineTests.dll",
            "ReusableCodeTests.dll",
            "TicketingTests.dll",

            //other random stuff already included as part of the runtime distributable that shouldn't also exist in the server
            "log4net.dll",
            "Microsoft.SqlServer.DTSRuntimeWrap.dll",
            "MySql.Data.dll",
            "Org.Mentalis.Security.dll",
            "mscorlib.dll", // well, duh

            // required for roundhouse
            "FluentNHibernate.dll",
            "Iesi.Collections.dll",
            "NHibernate.dll",
            "roundhouse.databases.sqlserver.dll",
            "roundhouse.dll",

            "Oracle.ManagedDataAccess.dll"
        };

       #region Database Properties
        private string _name;
        private string _description;
        private Byte[] _dll;
        private Byte[] _pdb;
        private string _committer;
        private DateTime _uploadDate;
        private string _dllFileVersion;
        private int _plugin_ID;

        public string Name
        {
	        get { return _name;}
	        set { SetField(ref _name,value);}
        }
        public string Description
        {
	        get { return _description;}
	        set { SetField(ref _description,value);}
        }
        public Byte[] Dll
        {
	        get { return _dll;}
	        set { SetField(ref _dll,value);}
        }
        public Byte[] Pdb
        {
	        get { return _pdb;}
	        set { SetField(ref _pdb,value);}
        }
        public string Committer
        {
	        get { return _committer;}
	        set { SetField(ref _committer,value);}
        }
        public DateTime UploadDate
        {
	        get { return _uploadDate;}
	        set { SetField(ref _uploadDate,value);}
        }
        public string DllFileVersion
        {
	        get { return _dllFileVersion;}
	        set { SetField(ref _dllFileVersion,value);}
        }

        [Relationship(typeof(Plugin))]
        public int Plugin_ID
        {
	        get { return _plugin_ID;}
	        set { SetField(ref _plugin_ID,value);}
        }

        #endregion

        #region Relationships
        
        [NoMappingToDatabase]
        public Plugin Plugin { get { return Repository.GetObjectByID<Plugin>(Plugin_ID); }}

        #endregion

        /// <summary>
        /// Uploads the given dll file to the catalogue database ready for use as a plugin within RDMP (also uploads any pdb file in the same dir)
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="f"></param>
        public LoadModuleAssembly(ICatalogueRepository repository, FileInfo f, Plugin plugin)
        {
            byte[] allPdbBytes = null;
            string version = null;

            //always allowed
            if(f.Name != "src.zip")
            {
                if (!f.Extension.ToLower().Equals(".dll"))
                    throw new NotSupportedException("Only .dll files can be commited");

                if (ProhibitedDllNames.Contains(f.Name) || ProhibitedDllNames.Contains("Test"))
                    throw new ArgumentException("Cannot commit assembly " + f.Name + " because it is a prohibited dll or has the word 'Test' in its filename");
                
                var pdb = new FileInfo(f.FullName.Substring(0, f.FullName.Length - ".dll".Length) + ".pdb");
                if (pdb.Exists)
                    allPdbBytes = File.ReadAllBytes(pdb.FullName);

                try
                {
                    version = FileVersionInfo.GetVersionInfo(f.FullName).FileVersion;
                }
                catch (Exception)
                {
                    // couldn't get file version, nevermind maybe it is some kind of freaky dll type
                }


            }
            else
            {
                //source code
                version = "1.0";
            }

            //so we can reference it in fetch requests to check for duplication (normaly Repository is set during hydration by the repo)
            Repository = repository;

            string name = f.Name;
            byte[] allBytes = File.ReadAllBytes(f.FullName);
            
            var dictionaryParameters = new Dictionary<string, object>()
                {
                    {"Name",name},
                    {"Dll",allBytes},
                    {"DllFileVersion",version},
                    {"Committer",Environment.UserName},
                    {"Plugin_ID",plugin.ID}
                };
                
            if (allPdbBytes != null)
                dictionaryParameters.Add("Pdb", allPdbBytes);

            Repository.InsertAndHydrate(this,dictionaryParameters);
        }

        internal LoadModuleAssembly(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Dll = r["Dll"] as byte[];
            Pdb = r["Pdb"] as byte[];
            Name = (string) r["Name"];
            Description = r["Description"] as string;
            Committer = r["Committer"] as string;
            UploadDate = Convert.ToDateTime(r["UploadDate"]);
            DllFileVersion = r["DllFileVersion"] as string;
            Plugin_ID = Convert.ToInt32(r["Plugin_ID"]);
        }
        
        internal LoadModuleAssembly(ShareManager shareManager, ShareDefinition shareDefinition)
        {
            shareManager.RepositoryLocator.CatalogueRepository.UpsertAndHydrate(this, shareManager, shareDefinition);
        }

        public static bool IsDllProhibited(FileInfo f)
        {
            return ProhibitedDllNames.Contains(f.Name);
        }
        
        public void DownloadAssembly(DirectoryInfo downloadDirectory)
        {
            string targetDirectory = downloadDirectory.FullName;

            if (targetDirectory == null)
                throw new Exception("Could not get currently executing assembly directory");

            if (!downloadDirectory.Exists)
                downloadDirectory.Create();

            string targetFile = Path.Combine(targetDirectory, Name);
            
            //file already exists
            if (File.Exists(targetFile))
                if(AreEqual(File.ReadAllBytes(targetFile), Dll))
                    return;

            int timeout = 5000;

            TryAgain:
            try
            {
                //if it has changed length or does not exist, write it out to the hardisk
                File.WriteAllBytes(targetFile, Dll);
                
                if (Pdb != null)
                {
                    string pdbFilename = Path.Combine(targetDirectory,
                        Name.Substring(0, Name.Length - ".dll".Length) + ".pdb");
                    File.WriteAllBytes(pdbFilename, Pdb);
                }
            }
            catch (Exception)
            {
                timeout -= 100;
                Thread.Sleep(100);

                if (timeout <= 0)
                    throw;

                goto TryAgain;
            }

        }

        private bool AreEqual(byte[] readAllBytes, byte[] dll)
        {
            if (readAllBytes.Length != dll.Length)
                return false;

            for (int i = 0; i < dll.Length; i++)
                if (!readAllBytes[i].Equals(dll[i]))
                    return false;

            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
