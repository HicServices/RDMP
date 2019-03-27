// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataQualityEngine.Data;
using FAnsi.Discovery;
using FAnsi.Discovery.ConnectionStringDefaults;
using HIC.Common.Validation;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using RDMPStartup.Events;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;


namespace RDMPStartup
{
    /// <summary>
    /// Locates main databases (Catalogue / Data Export - called Tier1 databases) and satellite databases (DQE, Logging etc - called Tier2 
    /// databases) and plugin databases (Called Tier3).
    /// 
    /// <para>Identifies which databases need to be patched.</para>
    /// 
    /// <para>Loads MEF assemblies and identifies assembly incompatibilities / Type Load errors.</para>
    /// </summary>
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
                var hostAssembly = typeof(Catalogue).Assembly;
                var dbAssembly = Assembly.Load("CatalogueLibrary.Database");

                foundCatalogue = Find((ITableRepository) RepositoryLocator.CatalogueRepository, hostAssembly, dbAssembly, 1, RDMPPlatformType.Catalogue);
            }
            catch (Exception e)
            {
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null,null,null,1, RDMPPlatformDatabaseStatus.Broken, RDMPPlatformType.Catalogue,e));
            }

            if (foundCatalogue)
                try
                {
                    //setup connection string keywords
                    foreach (ConnectionStringKeyword keyword in RepositoryLocator.CatalogueRepository.GetAllObjects<ConnectionStringKeyword>())
                    {
                        var tomem = new ToMemoryCheckNotifier(mefCheckNotifier);
                        keyword.Check(tomem);

                        //don't add broken keywords!
                        if (tomem.GetWorst() >= CheckResult.Fail)
                            continue;

                        //pass it into the system wide static keyword collection for use with all databases of this type all the time (that includes Microsoft Sql Server btw which means those options will happen for DataExport too!)
                        DiscoveredServerHelper.AddConnectionStringKeyword(keyword.DatabaseType, keyword.Name, keyword.Value, ConnectionStringKeywordPriority.SystemDefaultMedium);
                    }

                }
                catch (Exception ex)
                {
                    _mefCheckNotifier.OnCheckPerformed(new CheckEventArgs("Could not apply ConnectionStringKeywords",CheckResult.Fail, ex));
                }
            

            //only load data export manager if catalogue worked
            if(foundCatalogue)
            {
                LoadMEF(RepositoryLocator.CatalogueRepository);

                FindTier2Databases();

                try
                {
                    var dataExportRepository = RepositoryLocator.DataExportRepository;

                    //not configured
                    if(dataExportRepository == null)
                        return;

                    var hostAssembly = typeof(ExtractableDataSet).Assembly;
                    var dbAssembly = Assembly.Load("DataExportLibrary.Database");

                    Find((ITableRepository) dataExportRepository, hostAssembly, dbAssembly,1, RDMPPlatformType.DataExport);
                }
                catch (Exception e)
                {
                    DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null,null,null,1, RDMPPlatformDatabaseStatus.Broken, RDMPPlatformType.DataExport,e));
                }

                FindTier3Databases((CatalogueRepository) RepositoryLocator.CatalogueRepository);
            }

            Validator.RefreshExtraTypes(mefCheckNotifier);
        }

        private void FindTier3Databases(CatalogueRepository catalogueRepository)
        {
            ObjectConstructor constructor = new ObjectConstructor();

            foreach (Type patcherType in catalogueRepository.MEF.GetTypes<IPatcher>().Where(type => type.IsPublic))
            {
                try
                {
                    var instance = (IPatcher)constructor.Construct(patcherType);

                    PluginPatcherFound(this, new PluginPatcherFoundEventArgs(patcherType, instance, PluginPatcherStatus.Healthy));
                    FindWithPatcher(instance, 3, RDMPPlatformType.Plugin);

                }
                catch (Exception e)
                {
                    PluginPatcherFound(this, new PluginPatcherFoundEventArgs(patcherType, null, PluginPatcherStatus.CouldNotConstruct, e));
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
        private void FindTier2Databases()
        {
            //DQE
            Type type = typeof(DataQualityEngine.Class1);
            Debug.Assert(type != null);
            var dqe = new DataQualityEnginePatcher();
            FindWithPatcher(dqe,2,RDMPPlatformType.DQE);

            //Logging
            type = typeof(HIC.Logging.Class1);
            Debug.Assert(type != null);
            var logging = new LoggingDatabasePatcher();
            FindWithPatcher(logging,2,RDMPPlatformType.Logging);

            //ANO
            type = typeof(ANOStore.Class1);
            Debug.Assert(type != null);
            var ano = new ANOStoreDatabasePatcher();
            FindWithPatcher(ano,2,RDMPPlatformType.ANO);

            //Identifier Dump
            type = typeof(IdentifierDump.Class1);
            Debug.Assert(type != null);
            var identifierDump = new IdentifierDumpDatabasePatcher();
            FindWithPatcher(identifierDump, 2, RDMPPlatformType.IdentifierDump);
            
            //Query cache
            type = typeof(QueryCaching.Class1);
            var cache = new QueryCachingDatabasePatcher();
            FindWithPatcher(cache, 2, RDMPPlatformType.QueryCache);

            Debug.Assert(type != null);
        }

        private void FindWithPatcher(IPatcher patcher,int tier, RDMPPlatformType type)
        {
            Assembly hostAssembly = patcher.GetHostAssembly();
            Assembly dbAssembly = patcher.GetDbAssembly();

            var dbs = RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>().Where(eds => eds.WasCreatedByDatabaseAssembly(dbAssembly));

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

        private void LoadMEF(ICatalogueRepository catalogueRepository)
        {
            DirectoryInfo downloadDirectory = catalogueRepository.MEF.DownloadDirectory;
             
            //make sure the MEF directory exists
            if(!downloadDirectory.Exists)
                downloadDirectory.Create();

            var recordsInDatabase = catalogueRepository.PluginManager.GetCompatiblePlugins();

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

                foreach (var lma in recordsInDatabase[i].LoadModuleAssemblies.Where(lma => !LoadModuleAssembly.ProhibitedDllNames.Contains(lma.Name)))
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
            toProcess.Add(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            toProcess.AddRange(dirs);

            MEFSafeDirectoryCatalog = new SafeDirectoryCatalog(_mefCheckNotifier, toProcess.Select(d=>d.FullName).ToArray());
            catalogueRepository.MEF.Setup(MEFSafeDirectoryCatalog);

            _mefCheckNotifier.OnCheckPerformed(new CheckEventArgs("Loading Help...", CheckResult.Success));
            var sw = Stopwatch.StartNew();

            if(!CatalogueRepository.SuppressHelpLoading)
                catalogueRepository.CommentStore.ReadComments(Environment.CurrentDirectory);

            sw.Stop();
            _mefCheckNotifier.OnCheckPerformed(new CheckEventArgs("Help loading took:" + sw.Elapsed, CheckResult.Success));

        }
        #endregion
    }
}
