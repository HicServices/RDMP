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
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.VisualStudioSolutionFileProcessing;

namespace Rdmp.Core.CommandLine.Packing
{
    /// <summary>
    /// Creates a .zip file containing all binary files and (optionally) source code for an RDMP plugin.  The resulting zip file can be committed as a Plugin 
    /// to the Catalogue database.  Takes as input the .sln file path.  Binaries that are already part of the core RDMP will not be included in the zip (e.g.
    /// CatalogueLibrary.dll etc). 
    /// </summary>
    public class Packager
    {
        private readonly FileInfo _solutionToPackage;
        private readonly string _outputZipFilePath;
        private readonly bool _skipSourceCollection;
        private readonly bool _release;

        private Version _pluginVersionOfCatalogueLibrary;

        private List<FileInfo> _dllPackage = new List<FileInfo>();
        private List<FileInfo> _srcFilesFound = new List<FileInfo>();
        private List<FileInfo> _pluginAssemblies = new List<FileInfo>();

        readonly List<string> _blacklist = new List<string>();

        public Packager(FileInfo solutionToPackage, string outputZipFilePath, bool skipSourceCollection = false, bool release = false)
        {
            _solutionToPackage = solutionToPackage;
            _outputZipFilePath = outputZipFilePath;
            _skipSourceCollection = skipSourceCollection;
            _release = release;

            if (File.Exists(_outputZipFilePath))
                File.Delete(_outputZipFilePath);
        }
        
        public void PackageUpFile(ICheckNotifier notifier)
        {
            try
            {
                // Pull all the dlls and pdbs from all related project directories into one place for processing
                SetupWorkingDictionaries(notifier);

                GenerateExclusionList();

                FileInfo pluginsCopyOfCatalogueLibrary = _dllPackage.FirstOrDefault(c => c.Name.Equals("CatalogueLibrary.dll"));

                if (pluginsCopyOfCatalogueLibrary == null)
                    throw new Exception(
                        "Package target does not have a copy of CatalogueLibrary.dll in it's output directory");

                _pluginVersionOfCatalogueLibrary = new Version(FileVersionInfo.GetVersionInfo(pluginsCopyOfCatalogueLibrary.FullName).FileVersion);
                notifier.OnCheckPerformed(new CheckEventArgs("Your plugin targets CatalogueLibrary version " + _pluginVersionOfCatalogueLibrary, CheckResult.Success));
                
                var memStream = new MemoryStream();
                var archive = new ZipArchive(memStream, ZipArchiveMode.Update, true);

                var manifest = archive.CreateEntry("PluginManifest.txt");

                //add the version of CatalogueLibrary that we are targetting
                using (var entryStream = manifest.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    streamWriter.Write("CatalogueLibraryVersion:" + _pluginVersionOfCatalogueLibrary);
                }

                foreach (var assembly in _pluginAssemblies)
                    AddWithDependencies(notifier, assembly, archive);

                //create a src.zip file within the archive
                if (!_skipSourceCollection)
                {
                    var srcZip = ZipFile.Open(Path.Combine(_solutionToPackage.Directory.FullName, "src.zip"), ZipArchiveMode.Create);
                    foreach (var file in _srcFilesFound)
                    {
                        srcZip.CreateEntryFromFile(file.FullName, file.Name);
                    }
                    srcZip.Dispose();

                    archive.CreateEntryFromFile(Path.Combine(_solutionToPackage.Directory.FullName, "src.zip"), "src.zip");
                }

                archive.Dispose(); // this writes the archive to the memorystream!

                using (var fileStream = new FileStream(_outputZipFilePath, FileMode.Create))
                {
                    memStream.Seek(0, SeekOrigin.Begin);
                    memStream.CopyTo(fileStream);
                }

                memStream.Dispose();

                // now delete the temp source file:
                File.Delete(Path.Combine(_solutionToPackage.Directory.FullName, "src.zip"));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not package plugin", CheckResult.Fail, e));
            }
        }

        private void SetupWorkingDictionaries(ICheckNotifier notifier)
        {
            _pluginAssemblies = new List<FileInfo>();

            var sln = new VisualStudioSolutionFile(_solutionToPackage.Directory,_solutionToPackage);
            
            var pathsToProcess = new List<string>();
            foreach (VisualStudioProjectReference project in sln.Projects.Where(p => !p.Name.Contains("Tests")))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Found .csproj file reference at path: " + project.Path,CheckResult.Success));

                string expectedPath = Path.Combine(_solutionToPackage.Directory.FullName, Path.GetDirectoryName(project.Path));
                
                if(Directory.Exists(expectedPath))
                    notifier.OnCheckPerformed(new CheckEventArgs("SUCCESS: Found it at: " + expectedPath, CheckResult.Success));
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("FAIL: Did not find it at: " + expectedPath, CheckResult.Fail));

                pathsToProcess.Add(expectedPath);
            }

            foreach (var path in pathsToProcess)
                CollectAssemblies(path);
            
            if(!_skipSourceCollection && _solutionToPackage.Directory != null)
            {
                CollateSourceRecursively(_solutionToPackage.Directory);
            }
        }
        
        private void CollateSourceRecursively(DirectoryInfo dir)
        {
            //do not add all the source code from all the nuget packages!
            if(dir.Name.Equals("packages"))
                return;
            
            foreach (FileInfo file in dir.EnumerateFiles("*.cs"))
            {
                //already found this one
                if (_srcFilesFound.Any(f => f.Name.Equals(file.Name)))
                    continue;
                
                _srcFilesFound.Add(file);
            }

            foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
                CollateSourceRecursively(subDir);
        }

        private void CollectAssemblies(string projectDir, string assemblyName = null)
        {
            var projectOutputDir = Path.Combine(projectDir, "bin", _release?"Release":"Debug");
            if (!Directory.Exists(projectOutputDir))
                throw new InvalidOperationException("The project dir (" + projectOutputDir + ") does not exist, has the project been built? You need to build the project before invoking this packager.");

            var dllFile = new FileInfo(Path.Combine(projectOutputDir, assemblyName ?? Path.GetFileName(projectDir) + ".dll"));

            if(dllFile.Exists)
                _pluginAssemblies.Add(dllFile);

            foreach (var srcFilename in Directory.GetFiles(projectOutputDir, "*.dll"))
            {
                if (_dllPackage.Any(fi => fi.FullName == srcFilename))
                    continue;
                _dllPackage.Add(new FileInfo(srcFilename));
            }

            foreach (var srcFilename in Directory.GetFiles(projectOutputDir, "*.pdb"))
            {
                if (_dllPackage.Any(fi => fi.FullName == srcFilename))
                    continue;
                _dllPackage.Add(new FileInfo(srcFilename));
            }
        }
        
        private void GenerateExclusionList()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                _blacklist.Add(assembly.GetName().Name);
        }

        HashSet<string> _dependenciesAlreadyTraversed = new HashSet<string>();

        private void AddWithDependencies(ICheckNotifier notifier, FileInfo dll, ZipArchive archive)
        {
            if (archive.Entries.All(ze => ze.Name != dll.Name))
                archive.CreateEntryFromFile(dll.FullName, dll.Name);

            var pdb = new FileInfo(dll.FullName.Substring(0, dll.FullName.Length - ".dll".Length) + ".pdb");
            if (pdb.Exists)
            {
                if (archive.Entries.All(ze => ze.Name != pdb.Name))
                    archive.CreateEntryFromFile(pdb.FullName, pdb.Name);
            }
            
            foreach (AssemblyName name in Assembly.LoadFile(dll.FullName).GetReferencedAssemblies())
            {
                if(_blacklist.Contains(name.Name))
                    continue;

                if (LoadModuleAssembly.ProhibitedDllNames.Any(prohibitedName => prohibitedName.Equals(name.Name + ".dll")))
                    continue;
                
                var dependentDll = _dllPackage.FirstOrDefault(f => f.Name.EndsWith(name.Name + ".dll"));

                if(dependentDll == null)
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not find dependent dll " + name.Name, CheckResult.Warning));
                else
                {
                    //don't process circular dependencies
                    if(_dependenciesAlreadyTraversed.Contains(dependentDll.Name))
                        continue;

                    _dependenciesAlreadyTraversed.Add(dependentDll.Name);
                    AddWithDependencies(notifier, dependentDll, archive);
                }
            }

            
        }
    }
}
