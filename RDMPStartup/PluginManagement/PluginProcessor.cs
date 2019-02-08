// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Extensions;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Commits a .zip file containing dlls, pdbs and source code that constitutes an RDMP Plugin (Use PluginPackager.Packager to package up a .sln into such a
    /// zip file) into the given Catalogue Database.  
    /// 
    /// <para>All plugins are saved in binary format in the LoadModuleAssembly table of your Catalogue database and downloaded at Startup time into the MEF folder
    /// where they are loaded into the app domain.  Through this method every user gets the same version of the currently installed plugins and the database
    /// acts as the distributor of binaries.</para>
    /// </summary>
    public class PluginProcessor
    {
        private readonly ICheckNotifier _notifier;
        private readonly CatalogueRepository _repository;

        public PluginProcessor(ICheckNotifier notifier,CatalogueRepository repository)
        {
            _notifier = notifier;
            _repository = repository;
        }

        public bool ProcessFileReturningTrueIfIsUpgrade(FileInfo toCommit)
        {
            bool toReturn = false;

            if (toCommit.Extension.ToLowerInvariant() != ".zip")
                throw new NotSupportedException("Files must be zip files");
            
            var workingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            ZipFile.ExtractToDirectory(toCommit.FullName, workingDirectory);

            Version pluginVersion;
            try
            {
                pluginVersion = new Version(File.ReadLines(Path.Combine(workingDirectory, "PluginManifest.txt")).First().Split(':')[1]);
            }
            catch
            {
                throw new NotSupportedException("Could not find a valid version inside the PluginManifest.txt in the zip package");
            }

            // reject if it's not compatible with running version
            if (!pluginVersion.IsCompatibleWith(_repository.GetVersion(), 3))
                throw new NotSupportedException(String.Format("Plugin version {0} is incompatible with current running version of RDMP.", pluginVersion));
            
            // delete EXACT old versions of the Plugin
            var oldVersion = _repository.GetAllObjects<Plugin>().SingleOrDefault(p => p.Name.Equals(toCommit.Name) && p.PluginVersion == pluginVersion);

            List<LoadModuleAssembly> legacyDlls = new List<LoadModuleAssembly>();
            Plugin plugin = null;

            if (oldVersion != null)
            {
                legacyDlls.AddRange(oldVersion.LoadModuleAssemblies);
                toReturn = true;
                plugin = oldVersion;
            }
            else
                plugin = new Plugin(_repository, toCommit, pluginVersion);

            try
            {
                foreach (var file in Directory.GetFiles(workingDirectory, "*.dll"))
                    ProcessFile(plugin, new FileInfo(file), legacyDlls);

                foreach (var srcZipFile in Directory.GetFiles(workingDirectory, "src.zip"))
                    ProcessFile(plugin, new FileInfo(srcZipFile), legacyDlls);

                //For assemblies that have less dll dependencies now than before (i.e. redundant assemblies)
                foreach (LoadModuleAssembly unused in legacyDlls)
                {
                    var export = _repository.GetAllObjects<ObjectExport>().SingleOrDefault(e => e.IsReferenceTo(unused));
                    if (export != null)
                        export.DeleteInDatabase();

                    unused.DeleteInDatabase();
                }
            }
            catch (Exception e)
            {
                _notifier.OnCheckPerformed(new CheckEventArgs("Failed processing plugin " + toCommit.Name,
                    CheckResult.Fail, e));
                throw;
            }
            finally
            {
                //make sure we always delete the working directory
                Directory.Delete(workingDirectory, true);
            }

            return toReturn;
        }

        private void ProcessFile(Plugin plugin, FileInfo toCommit, List<LoadModuleAssembly> legacyDlls)
        {
            if (LoadModuleAssembly.IsDllProhibited(toCommit))
                return;

            try
            {
                var toUpdate = legacyDlls.SingleOrDefault(d => d.Name == toCommit.Name);

                if (toUpdate != null)
                {
                    toUpdate.UpdateTo(toCommit);
                    legacyDlls.Remove(toUpdate);
                }
                else
                    new LoadModuleAssembly(_repository,toCommit,plugin);

                _notifier.OnCheckPerformed(new CheckEventArgs("File " + toCommit.Name + " uploaded as a new LoadModuleAssembly under plugin " + plugin.Name, CheckResult.Success));
            }
            catch (Exception e)
            {
                _notifier.OnCheckPerformed(new CheckEventArgs("Failed to construct new load module assembly",CheckResult.Fail, e));
            }
        }
        /*
        private bool CheckAssemblyAndManifestAgreement(string workingDirectory, Version pluginCatalogueVersionFromManifest)
        {
            // Single: there must be one and only one *Plugin.dll file
            var assemblyFile = Directory.GetFiles(workingDirectory, "*Plugin.dll", SearchOption.TopDirectoryOnly).Single();

            // Create a new AppDomain so we can do our assembly testing in a clean environment and then unload it afterwards (otherwise if the version check fails we can't cleanup the directory because the assembly has been loaded and has a current handle in the current AppDomain)
            var testDomain = AppDomain.CreateDomain("TestDomain", null, AppDomain.CurrentDomain.SetupInformation);

            try
            {
                // Unfortunately, loading an assembly into a separate domain through domain.Load also loads it into the current domain!
                // So to avoid polluting CurrentDomain with unwanted/potentially dangerous assemblies, we need a bit of marshalling magic (and a separate class which can be marshalled).
                var assemblyChecker =
                    (PluginManifestAgreementChecker)
                        testDomain.CreateInstanceAndUnwrap(typeof (PluginManifestAgreementChecker).Assembly.FullName,
                            typeof (PluginManifestAgreementChecker).FullName);

                var versionsAgree = assemblyChecker.CheckPlugin(assemblyFile, pluginCatalogueVersionFromManifest, _notifier);
                if (!versionsAgree)
                    _notifier.OnCheckPerformed(new CheckEventArgs(assemblyChecker.LastMessage, CheckResult.Fail));

                return versionsAgree;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                AppDomain.Unload(testDomain);
            }
        }*/
    }
}
