// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using FAnsi.Discovery;
using FAnsi.Discovery.ConnectionStringDefaults;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup.Events;
using Rdmp.Core.Validation;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.Startup
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

        private readonly EnvironmentInfo _environmentInfo;
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        public event FoundPlatformDatabaseHandler DatabaseFound = delegate { };
        public event MEFDownloadProgressHandler MEFFileDownloaded = delegate { };

        /// <summary>
        /// Set to true to ignore unpatched platform databases
        /// </summary>
        public bool SkipPatching { get; set; }

        public PluginPatcherFoundHandler PluginPatcherFound = delegate { }; 

        PatcherManager _patcherManager = new PatcherManager();

        #region Constructors
        public Startup(EnvironmentInfo environmentInfo,IRDMPPlatformRepositoryServiceLocator repositoryLocator):this(environmentInfo)
        {
            RepositoryLocator = repositoryLocator;
        }

        public Startup(EnvironmentInfo environmentInfo)
        {
            _environmentInfo = environmentInfo;
            TypeGuesser.GuessSettingsFactory.Defaults.CharCanBeBoolean = false;
        }
        #endregion

        #region Database Discovery
        public void DoStartup(ICheckNotifier notifier)
        {
            var foundCatalogue = false;

            notifier.OnCheckPerformed(new CheckEventArgs("Loading core assemblies",CheckResult.Success));

            DiscoveredServerHelper.CreateDatabaseTimeoutInSeconds = UserSettings.CreateDatabaseTimeout;

            var cataloguePatcher = new CataloguePatcher();

            try
            {
                foundCatalogue = Find(RepositoryLocator.CatalogueRepository,cataloguePatcher,notifier);
            }
            catch (Exception e)
            {
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null,cataloguePatcher, RDMPPlatformDatabaseStatus.Broken,e));
            }

            if (foundCatalogue)
                try
                {
                    //setup connection string keywords
                    foreach (var keyword in RepositoryLocator.CatalogueRepository.GetAllObjects<ConnectionStringKeyword>())
                    {
                        var tomem = new ToMemoryCheckNotifier(notifier);
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
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not apply ConnectionStringKeywords",CheckResult.Fail, ex));
                }
            

            //only load data export manager if catalogue worked
            if(foundCatalogue)
            {
                LoadMEF(RepositoryLocator.CatalogueRepository,notifier);

                //find tier 2 databases
                foreach (var patcher in _patcherManager.Tier2Patchers) 
                    FindWithPatcher(patcher,notifier);

                try
                {
                    var dataExportRepository = RepositoryLocator.DataExportRepository;

                    //not configured
                    if(dataExportRepository == null)
                        return;

                    Find(dataExportRepository, new DataExportPatcher(),notifier);
                }
                catch (Exception e)
                {
                    DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null,new DataExportPatcher(), RDMPPlatformDatabaseStatus.Broken,e));
                }

                FindTier3Databases( RepositoryLocator.CatalogueRepository,notifier);
            }
            
            if(MEFSafeDirectoryCatalog != null)
                Validator.RefreshExtraTypes(MEFSafeDirectoryCatalog,notifier);
        }

        private void FindTier3Databases(ICatalogueRepository catalogueRepository,ICheckNotifier notifier)
        {
            foreach (var patcher in _patcherManager.GetTier3Patchers(catalogueRepository.MEF,PluginPatcherFound))
                FindWithPatcher(patcher,notifier);
        }

        private bool Find(IRepository repository, IPatcher patcher,ICheckNotifier notifier)
        {
            //if it's not configured
            if (repository == null)
            {
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(null, patcher, RDMPPlatformDatabaseStatus.Unreachable));
                return false;
            }

            // it's not a database we are getting this data from then assume it's good to go
            if (repository is not ITableRepository tableRepository)
                return true;
                
            //check we can reach it
            var db = tableRepository.DiscoveredServer.GetCurrentDatabase();
            notifier.OnCheckPerformed(new CheckEventArgs($"Connecting to {db.GetRuntimeName()} on {db.Server.Name}",CheckResult.Success));

            //is it reachable
            try
            {
                tableRepository.TestConnection();
            }
            catch (Exception ex)
            {
                //no
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(tableRepository, patcher, RDMPPlatformDatabaseStatus.Unreachable, ex));
                return false;
            }


            Patch.PatchingState patchingRequired;
            try
            {
                //is it up-to-date on patches?
                patchingRequired = Patch.IsPatchingRequired(tableRepository.DiscoveredServer.GetCurrentDatabase(),
                    patcher, out _, out _, out _);
            }
            catch (Exception e)
            {
                //database is broken (maybe the version of the db is ahead of the host assembly?)
                DatabaseFound(this, new PlatformDatabaseFoundEventArgs(tableRepository, patcher, RDMPPlatformDatabaseStatus.Broken, e));
                return false;
            }

            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(tableRepository, patcher, patchingRequired switch
                {
                    Patch.PatchingState.NotRequired => RDMPPlatformDatabaseStatus.Healthy,
                    Patch.PatchingState.Required    => SkipPatching ? RDMPPlatformDatabaseStatus.Healthy : RDMPPlatformDatabaseStatus.RequiresPatching,
                    Patch.PatchingState.SoftwareBehindDatabase  => RDMPPlatformDatabaseStatus.SoftwareOutOfDate,
                    _ => throw new ArgumentOutOfRangeException(nameof(patchingRequired))
                }));

            return true;
        }

        private void FindWithPatcher(IPatcher patcher,ICheckNotifier notifier)
        {
            var dbs = RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>().Where(eds => eds.WasCreatedBy(patcher));

            foreach (IExternalDatabaseServer server in dbs)
            {
                try
                {
                    var builder = DataAccessPortal.GetInstance()
                        .ExpectServer(server, DataAccessContext.InternalDataProcessing)
                        .Builder;

                    Find(new CatalogueRepository(builder), patcher,notifier);
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs($"Could not resolve ExternalDatabaseServer '{server}'",CheckResult.Warning,e));
                }
            }
        }
        #endregion


        #region MEF

        private void LoadMEF(ICatalogueRepository catalogueRepository, ICheckNotifier notifier)
        {
            catalogueRepository.MEF ??= new MEF();

            var downloadDirectory = catalogueRepository.MEF.DownloadDirectory;
             
            //make sure the MEF directory exists
            if(!downloadDirectory.Exists)
                downloadDirectory.Create();

            var compatiblePlugins = catalogueRepository.PluginManager.GetCompatiblePlugins();

            var dirs = new List<DirectoryInfo>();
            var toLoad = new List<DirectoryInfo> {
                //always load the current application directory
                new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
            };

            for (var i = 0; i < compatiblePlugins.Length; i++)
            {
                var subDirName = compatiblePlugins[i].GetPluginDirectoryName(downloadDirectory);
                var subdir = Directory.CreateDirectory(subDirName);

                dirs.Add(subdir);
                                                             
                var existingFiles = subdir.GetFiles($"*{PackPluginRunner.PluginPackageSuffix}").ToList();

                //if we have not downloaded this yet
                if(!existingFiles.Any(f=>f.Name.Equals(compatiblePlugins[i].Name)))
                    compatiblePlugins[i].LoadModuleAssemblies.SingleOrDefault()?.DownloadAssembly(subdir); 
                else
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Found existing file '{compatiblePlugins[i].Name}' so didn't bother downloading it.",CheckResult.Success));
                                
                foreach(var archive in  subdir.GetFiles($"*{PackPluginRunner.PluginPackageSuffix}").ToList())
                {                    
                    //get rid of any old out dirs
                    var outDir = subdir.EnumerateDirectories("out").SingleOrDefault();
                    
                    var mustUnzip = true;

                    //if there's already an unpacked version
                    if(outDir is { Exists: true })
                    {
                        //if the directory has no files we have to unzip - otherwise it has an unzipped version already yay
                        mustUnzip = !outDir.GetFiles("*.dll",SearchOption.AllDirectories).Any();

                        if(mustUnzip)
                            outDir.Delete(true);
                    }
                    else
                        outDir = subdir.CreateSubdirectory("out");

                    if(mustUnzip)
                        using(var zf = ZipFile.OpenRead(archive.FullName))
                            try
                            {
                                zf.ExtractToDirectory(outDir.FullName);
                            }
                            catch(Exception ex)
                            {
                                notifier.OnCheckPerformed(new CheckEventArgs(
                                    $"Could not extract Plugin to '{outDir.FullName}'",CheckResult.Warning,ex));
                            }
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"Found existing directory '{outDir.FullName}' so didn't bother unzipping.",CheckResult.Success));

                    toLoad.AddRange(_environmentInfo.GetPluginSubDirectories(outDir.CreateSubdirectory("lib"), notifier));

                    //tell them we downloaded it
                    MEFFileDownloaded(this,
                            new MEFFileDownloadProgressEventArgs(subdir, compatiblePlugins.Length, i + 1,
                                archive.Name, false, MEFFileDownloadEventStatus.Success));
                }
            }

            //The only Directories in MEF folder should be Plugin subdirectories, any that don't correspond with a plugin should be deleted 
            foreach (var unexpectedDirectory in downloadDirectory.GetDirectories().Where(expected=>!dirs.Any(d=>d.FullName.Equals(expected.FullName))))
            {
                try
                {
                    unexpectedDirectory.Delete(true);
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Deleted unreferenced plugin folder {unexpectedDirectory.FullName}", CheckResult.Success));

                }
                catch (Exception ex)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"Found unreferenced (no Plugin) folder {unexpectedDirectory.FullName} but we were unable to delete it (possibly because it is in use, try closing all your local RDMP applications and restarting this one)",
                            CheckResult.Fail, ex));
                }
            }

            AssemblyResolver.SetupAssemblyResolver(toLoad.ToArray());
            
            MEFSafeDirectoryCatalog = new SafeDirectoryCatalog(notifier, toLoad.Select(d=>d.FullName).ToArray());
            catalogueRepository.MEF.Setup(MEFSafeDirectoryCatalog);
            
            notifier.OnCheckPerformed(new CheckEventArgs("Loading Help...", CheckResult.Success));
            var sw = Stopwatch.StartNew();

            if(!CatalogueRepository.SuppressHelpLoading)
                catalogueRepository.CommentStore.ReadComments(Environment.CurrentDirectory,"SourceCodeForSelfAwareness.zip");

            sw.Stop();
            notifier.OnCheckPerformed(new CheckEventArgs($"Help loading took:{sw.Elapsed}", CheckResult.Success));
        }
        #endregion

        /// <summary>
        /// <para>
        /// Call before running <see cref="Startup"/>.  Sets up basic assembly redirects to the execution directory 
        /// (see <see cref="AssemblyResolver"/>) and FAnsiSql DBMS implementations.
        /// </para>
        /// <para>Note that this method can be used even if you do not then go on to use <see cref="Startup"/> e.g. if you 
        /// are performing a low level operation like patching</para>
        /// </summary>
        public static void PreStartup()
        {
            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();
        }
    }
}
