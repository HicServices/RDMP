using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataQualityEngine.Data;
using HIC.Common.Validation;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using RDMPStartup.Events;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;


namespace RDMPStartup
{
    public class Startup
    {
        public SafeDirectoryCatalog MEFSafeDirectoryCatalog { get; private set; }

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        private ICheckNotifier _mefCheckNotifier;
        public event FoundPlatformDatabaseHandler DatabaseFound = delegate { };
        public event MEFDownloadProgressHandler MEFFileDownloaded = delegate { };

        public event PluginPatcherFoundHandler PluginPatcherFound = delegate { }; 

        #region Constructors
        public Startup(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
        }

        public Startup()
        {
            
        }
        #endregion

        #region Database Discovery
        public void DoStartup(ICheckNotifier mefCheckNotifier)
        {
            _mefCheckNotifier = mefCheckNotifier;
            bool foundCatalogue = false;

            Assembly.Load(typeof(Catalogue).Assembly.FullName);
            Assembly.Load(typeof(CatalogueLibrary.Database.Class1).Assembly.FullName);

            Assembly.Load(typeof(ExtractableDataSet).Assembly.FullName);
            Assembly.Load(typeof(DataExportLibrary.Database.Class1).Assembly.FullName);

            Assembly.Load(typeof(Evaluation).Assembly.FullName);
            Assembly.Load(typeof(DataQualityEngine.Database.Class1).Assembly.FullName);

            Assembly.Load(typeof(LogManager).Assembly.FullName);
            Assembly.Load(typeof(HIC.Logging.Database.Class1).Assembly.FullName);

            Assembly.Load(typeof(ANOStore.Class1).Assembly.FullName);
            Assembly.Load(typeof(ANOStore.Database.Class1).Assembly.FullName);

            Assembly.Load(typeof(IdentifierDump.Class1).Assembly.FullName);
            Assembly.Load(typeof(IdentifierDump.Database.Class1).Assembly.FullName);

            Assembly.Load(typeof(QueryCachingDatabasePatcher).Assembly.FullName);
            Assembly.Load(typeof(QueryCaching.Database.Class1).Assembly.FullName);

            try
            {
                TableRepository catalogueRepository = RepositoryLocator.CatalogueRepository;
                var hostAssembly = typeof(Catalogue).Assembly;
                var dbAssembly = Assembly.Load("CatalogueLibrary.Database");
                
                foundCatalogue = Find(catalogueRepository,hostAssembly, dbAssembly,1, RDMPPlatformType.Catalogue);
            }
            catch (Exception e)
            {
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null,null,null,1, RDMPPlatformDatabaseStatus.Broken, RDMPPlatformType.Catalogue,e));
            }

            //only load data export manager if catalogue worked
            if(foundCatalogue)
            {
                LoadMEF(RepositoryLocator.CatalogueRepository);

                FindTier2Databases(RepositoryLocator.CatalogueRepository);

                try
                {
                    var dataExportRepository = RepositoryLocator.DataExportRepository;

                    //not configured
                    if(dataExportRepository == null)
                        return;

                    var hostAssembly = typeof(ExtractableDataSet).Assembly;
                    var dbAssembly = Assembly.Load("DataExportLibrary.Database");

                    Find(dataExportRepository, hostAssembly, dbAssembly,1, RDMPPlatformType.DataExport);
                }
                catch (Exception e)
                {
                    DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null,null,null,1, RDMPPlatformDatabaseStatus.Broken, RDMPPlatformType.DataExport,e));
                }

                FindTier3Databases(RepositoryLocator.CatalogueRepository);
            }

            Validator.RefreshExtraTypes();
        }

        private void FindTier3Databases(CatalogueRepository catalogueRepository)
        {
            var pluginBootstrapper = new PluginBootstrapper(catalogueRepository);
            
            var pluginPatcherTypes = pluginBootstrapper.FindPatchers();

            foreach (Type t in pluginPatcherTypes)
            {
                try
                {
                    var instance = pluginBootstrapper.Create(t);

                    PluginPatcherFound(this, new PluginPatcherFoundEventArgs(t, instance, PluginPatcherStatus.Healthy));
                    FindWithPatcher(instance, 3, RDMPPlatformType.Plugin);

                }
                catch (Exception e)
                {
                    PluginPatcherFound(this,new PluginPatcherFoundEventArgs(t,null, PluginPatcherStatus.CouldNotConstruct,e));
                }
            }

        }

        private bool Find(ITableRepository tableRepository, Assembly hostAssembly, Assembly dbAssembly, int tier,RDMPPlatformType rdmpPlatformType)
        {
            //is it reachable
            try
            {
                tableRepository.TestConnection();
            }
            catch (Exception ex)
            {
                //no
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(tableRepository, hostAssembly, dbAssembly, tier, RDMPPlatformDatabaseStatus.Unreachable, rdmpPlatformType, ex));
                return false;
            }


            bool patchingRequired;
            try
            {
                //is it up-to-date on patches?
                Version databaseVersion;
                Patch[] patchesInDatabase;
                SortedDictionary<string, Patch> allPatchesInAssembly;
                
                
                patchingRequired = Patch.IsPatchingRequired((SqlConnectionStringBuilder) tableRepository.ConnectionStringBuilder, dbAssembly, hostAssembly, out databaseVersion, out patchesInDatabase,out allPatchesInAssembly);
            }
            catch (Exception e)
            {
                //database is broken (maybe the version of the db is ahead of the host assembly?)
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(tableRepository, hostAssembly, dbAssembly, tier, RDMPPlatformDatabaseStatus.Broken, rdmpPlatformType, e));
                return false;
            }

            if (patchingRequired)
                //database is broken
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(tableRepository, hostAssembly, dbAssembly, tier, RDMPPlatformDatabaseStatus.RequiresPatching, rdmpPlatformType));
            else
                //database is broken
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(tableRepository, hostAssembly, dbAssembly, tier, RDMPPlatformDatabaseStatus.Healthy, rdmpPlatformType));
          

            return true;
        }
        private void FindTier2Databases(CatalogueRepository catalogueRepository)
        {
            var defaults = new ServerDefaults(catalogueRepository);
            
            //DQE
            Type type = typeof(DataQualityEngine.Class1);
            Debug.Assert(type != null);
            var dqe = new DataQualityEnginePatcher(defaults);
            FindWithPatcher(dqe,2,RDMPPlatformType.DQE);

            //Logging
            type = typeof(HIC.Logging.Class1);
            Debug.Assert(type != null);
            var logging = new LoggingDatabasePatcher(catalogueRepository);
            FindWithPatcher(logging,2,RDMPPlatformType.Logging);

            //ANO
            type = typeof(ANOStore.Class1);
            Debug.Assert(type != null);
            var ano = new ANOStoreDatabasePatcher(catalogueRepository);
            FindWithPatcher(ano,2,RDMPPlatformType.ANO);

            //Identifier Dump
            type = typeof(IdentifierDump.Class1);
            Debug.Assert(type != null);
            var identifierDump = new IdentifierDumpDatabasePatcher(catalogueRepository);
            FindWithPatcher(identifierDump, 2, RDMPPlatformType.IdentifierDump);
            
            //Query cache
            type = typeof(QueryCaching.Class1);
            var cache = new QueryCachingDatabasePatcher(catalogueRepository);
            FindWithPatcher(cache, 2, RDMPPlatformType.QueryCache);

            Debug.Assert(type != null);
        }

        private void FindWithPatcher(IPatcher patcher,int tier, RDMPPlatformType type)
        {
            Assembly hostAssembly, dbAssembly;
            var dbs = patcher.FindDatabases(out hostAssembly, out dbAssembly);

            //there are none
            if(dbs == null)
                return;

            foreach (IExternalDatabaseServer server in dbs)
            {
                var builder = DataAccessPortal.GetInstance()
                    .ExpectServer(server, DataAccessContext.InternalDataProcessing)
                    .Builder;

                Find(new CatalogueRepository(builder), hostAssembly, dbAssembly,tier,type);
            }
        }
        #endregion


        #region MEF

        private void LoadMEF(CatalogueRepository catalogueRepository)
        {
            DirectoryInfo downloadDirectory = catalogueRepository.MEF.DownloadDirectory;
             
            //make sure the MEF directory exists
            if(!downloadDirectory.Exists)
                downloadDirectory.Create();

            var recordsInDatabase = catalogueRepository.GetAllObjects<Plugin>();

            List<DirectoryInfo> dirs = new List<DirectoryInfo>();

            for (int i = 0; i < recordsInDatabase.Length; i++)
            {
                var subDirName = recordsInDatabase[i].GetPluginDirectoryName(downloadDirectory);
                var subdir = Directory.CreateDirectory(subDirName);

                dirs.Add(subdir);

                var files = subdir.GetFiles("*.dll").ToList();

                var srcFile = subdir.GetFiles().SingleOrDefault(f => f.Name.Equals("src.zip"));
                if(srcFile != null)
                    files.Add(srcFile);

                foreach (var lma in recordsInDatabase[i].LoadModuleAssemblies)
                {
                    
                    try
                    {
                        lma.DownloadAssembly(subdir);
                        MEFFileDownloaded(this,
                                new MEFFileDownloadProgressEventArgs(subdir, recordsInDatabase.Length, i + 1,
                                    lma.Name, lma.Pdb != null, MEFFileDownloadEventStatus.Success));
                    }
                    catch (Exception e)
                    {
                        MEFFileDownloaded(this,
                            new MEFFileDownloadProgressEventArgs(subdir, recordsInDatabase.Length, i + 1,
                                lma.Name, lma.Pdb != null, MEFFileDownloadEventStatus.OtherError, e));
                    }

                    //file is supposed to be there
                    files.Remove(files.SingleOrDefault(f => f.Name.Equals(lma.Name)));
                }

                //After processing all the load module assemblies lets get rid of the unreferenced dlls that are kicking about 
                foreach (FileInfo f in files)
                {
                    try
                    {
                        f.Delete();
                        _mefCheckNotifier.OnCheckPerformed(new CheckEventArgs("Deleted unreferenced dll " + f.FullName, CheckResult.Success));
                    }
                    catch (Exception e)
                    {
                        _mefCheckNotifier.OnCheckPerformed(new CheckEventArgs("Found unreferenced dll " + f.FullName + " but we were unable to delete it (possibly because it is in use, try closing all your local RDMP applications and restarting this one)", CheckResult.Fail,e));
                    }
                }
            }

            //The only Directories in MEF folder should be Plugin subdirectories, any that don't correspond with a plugin should be deleted 
            foreach (DirectoryInfo unexpectedDirectory in downloadDirectory.GetDirectories().Where(expected=>!dirs.Any(d=>d.FullName.Equals(expected.FullName))))
            {
                try
                {
                    unexpectedDirectory.Delete(true);
                    _mefCheckNotifier.OnCheckPerformed(new CheckEventArgs("Deleted unreferenced plugin folder " + unexpectedDirectory.FullName, CheckResult.Success));

                }
                catch (Exception ex)
                {
                    _mefCheckNotifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Found unreferenced (no Plugin) folder " + unexpectedDirectory.FullName +
                            " but we were unable to delete it (possibly because it is in use, try closing all your local RDMP applications and restarting this one)",
                            CheckResult.Fail, ex));

                }
            }

            List<DirectoryInfo> toProcess = new List<DirectoryInfo>();
            toProcess.Add(new DirectoryInfo(Environment.CurrentDirectory));
            toProcess.AddRange(dirs);

            MEFSafeDirectoryCatalog = new SafeDirectoryCatalog(_mefCheckNotifier, toProcess.Select(d=>d.FullName).ToArray());
            catalogueRepository.MEF.Setup(MEFSafeDirectoryCatalog);
        }
        #endregion
    }
}