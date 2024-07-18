// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Loader;
using FAnsi.Discovery;
using FAnsi.Discovery.ConnectionStringDefaults;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Startup.Events;

namespace Rdmp.Core.Startup;

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
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
    public event FoundPlatformDatabaseHandler DatabaseFound = static delegate { };
    //public event MEFDownloadProgressHandler MEFFileDownloaded = delegate { };

    /// <summary>
    /// Set to true to ignore unpatched platform databases
    /// </summary>
    public bool SkipPatching { get; init; }

    public PluginPatcherFoundHandler PluginPatcherFound = static delegate { };

    private readonly PatcherManager _patcherManager = new();

    #region Constructors

    public Startup(IRDMPPlatformRepositoryServiceLocator repositoryLocator) : this()
    {
        RepositoryLocator = repositoryLocator;
    }

    public Startup()
    {
        TypeGuesser.GuessSettingsFactory.Defaults.CharCanBeBoolean = false;
    }

    #endregion

    #region Database Discovery

    public void DoStartup(ICheckNotifier notifier)
    {
        var foundCatalogue = false;

        notifier.OnCheckPerformed(new CheckEventArgs("Loading core assemblies", CheckResult.Success));

        DiscoveredServerHelper.CreateDatabaseTimeoutInSeconds = UserSettings.CreateDatabaseTimeout;

        var cataloguePatcher = new CataloguePatcher();

        try
        {
            foundCatalogue = Find(RepositoryLocator.CatalogueRepository, cataloguePatcher, notifier);
        }
        catch (Exception e)
        {
            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(null, cataloguePatcher, RDMPPlatformDatabaseStatus.Broken, e));
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
                    DiscoveredServerHelper.AddConnectionStringKeyword(keyword.DatabaseType, keyword.Name, keyword.Value,
                        ConnectionStringKeywordPriority.SystemDefaultMedium);
                }
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not apply ConnectionStringKeywords",
                    CheckResult.Fail, ex));
            }


        //only load data export manager if catalogue worked
        if (!foundCatalogue) return;

        LoadMEF(RepositoryLocator.CatalogueRepository, notifier);

        //find tier 2 databases
        foreach (var patcher in _patcherManager.Tier2Patchers)
            FindWithPatcher(patcher, notifier);

        try
        {
            var dataExportRepository = RepositoryLocator.DataExportRepository;

            //not configured
            if (dataExportRepository == null)
                return;

            Find(dataExportRepository, new DataExportPatcher(), notifier);
        }
        catch (Exception e)
        {
            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(null, new DataExportPatcher(), RDMPPlatformDatabaseStatus.Broken,
                    e));
        }

        FindTier3Databases(notifier);
    }

    private void FindTier3Databases(ICheckNotifier notifier)
    {
        foreach (var patcher in _patcherManager.GetTier3Patchers(PluginPatcherFound))
            FindWithPatcher(patcher, notifier);
    }

    private bool Find(IRepository repository, IPatcher patcher, ICheckNotifier notifier)
    {
        //if it's not configured
        if (repository == null)
        {
            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(null, patcher, RDMPPlatformDatabaseStatus.Unreachable));
            return false;
        }

        // it's not a database we are getting this data from then assume it's good to go
        if (repository is not ITableRepository tableRepository)
            return true;

        //check we can reach it
        var db = tableRepository.DiscoveredServer.GetCurrentDatabase();
        notifier.OnCheckPerformed(new CheckEventArgs($"Connecting to {db.GetRuntimeName()} on {db.Server.Name}",
            CheckResult.Success));

        //is it reachable
        try
        {
            tableRepository.TestConnection();
        }
        catch (Exception ex)
        {
            //no
            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(tableRepository, patcher, RDMPPlatformDatabaseStatus.Unreachable,
                    ex));
            return false;
        }


        try
        {
            //is it up-to-date on patches?
            var patchingRequired = Patch.IsPatchingRequired(tableRepository.DiscoveredServer.GetCurrentDatabase(),
                patcher, out _, out _, out _);
            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(tableRepository, patcher, patchingRequired switch
                {
                    Patch.PatchingState.NotRequired => RDMPPlatformDatabaseStatus.Healthy,
                    Patch.PatchingState.Required => SkipPatching
                        ? RDMPPlatformDatabaseStatus.Healthy
                        : RDMPPlatformDatabaseStatus.RequiresPatching,
                    Patch.PatchingState.SoftwareBehindDatabase => RDMPPlatformDatabaseStatus.SoftwareOutOfDate,
                    _ => throw new InvalidOperationException(nameof(patchingRequired))
                }));
        }
        catch (Exception e)
        {
            //database is broken (maybe the version of the db is ahead of the host assembly?)
            DatabaseFound(this,
                new PlatformDatabaseFoundEventArgs(tableRepository, patcher, RDMPPlatformDatabaseStatus.Broken, e));
            return false;
        }

        return true;
    }

    private void FindWithPatcher(IPatcher patcher, ICheckNotifier notifier)
    {
        var dbs = RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>()
            .Where(eds => eds.WasCreatedBy(patcher));

        foreach (IExternalDatabaseServer server in dbs)
            try
            {
                var builder = DataAccessPortal
                    .ExpectServer(server, DataAccessContext.InternalDataProcessing)
                    .Builder;

                Find(new CatalogueRepository(builder), patcher, notifier);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"Could not resolve ExternalDatabaseServer '{server}'",
                    CheckResult.Warning, e));
            }
    }

    #endregion


    #region MEF

    /// <summary>
    /// Load the plugins from the platform DB
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <param name="notifier"></param>
    private static void LoadMEF(ICatalogueRepository catalogueRepository, ICheckNotifier notifier)
    {
        foreach (var (name, body) in LoadModuleAssembly.PluginFiles().SelectMany(LoadModuleAssembly.GetContents))
            try
            {
                AssemblyLoadContext.Default.LoadFromStream(body);
            }
            catch (Exception e)
            {
                var msg = $"Could not load plugin component {name} due to {e.Message}";
                Console.Error.WriteLine(msg);
                notifier.OnCheckPerformed(new CheckEventArgs(msg, CheckResult.Warning, e));
            }
            finally
            {
                body.Dispose();
            }

        if (CatalogueRepository.SuppressHelpLoading) return;

        notifier.OnCheckPerformed(new CheckEventArgs("Loading Help...", CheckResult.Success));
        var sw = Stopwatch.StartNew();
        catalogueRepository.CommentStore.ReadComments("SourceCodeForSelfAwareness.zip");
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