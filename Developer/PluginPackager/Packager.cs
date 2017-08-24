using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using CatalogueLibrary.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.VisualStudioSolutionFileProcessing;

namespace PluginPackager
{
    class Packager
    {
        private readonly FileInfo _solutionToPackage;
        private readonly string _outputZipFilePath;
        private readonly bool _skipSourceCollection;

        private DirectoryInfo _packageSourceDirectory;
        private Version _pluginVersionOfCatalogueLibrary;
        private string _pluginBaseName;

        public Packager(FileInfo solutionToPackage, string outputZipFilePath, bool skipSourceCollection = false)
        {
            _solutionToPackage = solutionToPackage;
            _pluginBaseName = Path.GetFileNameWithoutExtension(_solutionToPackage.Name);
            _outputZipFilePath = outputZipFilePath;
            _skipSourceCollection = skipSourceCollection;

            if (File.Exists(_outputZipFilePath))
                File.Delete(_outputZipFilePath);
        }

        public void PackageDatabase()
        {
            
        }

        public DirectoryInfo PackageUpFile()
        {
            try
            {
                // Pull all the dlls and pdbs from all related project directories into one place for processing
                SetupWorkingDirectory();

                GenerateExclusionList();

                FileInfo[] candidates = _workingDirectory.GetFiles("*.dll").ToArray();
                FileInfo pluginsCopyOfCatalogueLibrary = candidates.SingleOrDefault(c => c.Name.Equals("CatalogueLibrary.dll"));

                if (pluginsCopyOfCatalogueLibrary == null)
                    throw new Exception("Package target does not have a copy of CatalogueLibrary.dll in it's output directory");

                _pluginVersionOfCatalogueLibrary = new Version(FileVersionInfo.GetVersionInfo(pluginsCopyOfCatalogueLibrary.FullName).FileVersion);
                Console.WriteLine("Your plugin targets CatalogueLibrary version " + _pluginVersionOfCatalogueLibrary);

                //add the version of CatalogueLibrary that we are targetting
                File.WriteAllText(Path.Combine(_outputDirectory.FullName, "PluginManifest.txt"), "CatalogueLibraryVersion:" + _pluginVersionOfCatalogueLibrary);

                foreach (var assembly in _pluginAssemblies)
                    AddWithDependencies(assembly);

                SafeDirectoryCatalog mefloader = new SafeDirectoryCatalog(_workingDirectory.FullName);

                foreach (ComposablePartDefinition part in mefloader.Parts)
                    Console.WriteLine("Found good part " + part);

                foreach (var badpart in mefloader.BadAssembliesDictionary)
                {
                    Console.WriteLine("Found bad part " + badpart.Key + " : " + badpart.Value.Message);
                    Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(badpart.Value));
                }

                mefloader.Dispose();

                //create a src.zip file within the archive
                if(!_skipSourceCollection)
                {
                    ZipFile.CreateFromDirectory(_srcDirectory.FullName, Path.Combine(_outputDirectory.FullName,"src.zip"));
                    _srcDirectory.Delete(true);
                }

                ZipFile.CreateFromDirectory(_outputDirectory.FullName, _outputZipFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return _workingDirectory;
        }

        private void SetupWorkingDirectory()
        {
            CreateWorkingDirectory();

            _pluginAssemblies = new List<FileInfo>();

            
            VisualStudioSolutionFile sln = new VisualStudioSolutionFile(_solutionToPackage);


            List<string> pathsToProcess = new List<string>();
            foreach (VisualStudioProjectReference project in sln.Projects.Where(p => !p.Name.Contains("Tests")))
            {
                Console.WriteLine("Found .csproj file reference at path: " + project.Path);

                string expectedPath = Path.Combine(_solutionToPackage.Directory.FullName,Path.GetDirectoryName(project.Path));


                if(Directory.Exists(expectedPath))
                    Console.WriteLine("SUCCESS: Found it at:" + expectedPath);
                else
                    Console.WriteLine("FAIL: Did not find it at:" + expectedPath);

                pathsToProcess.Add(expectedPath);
            }

            foreach (var path in pathsToProcess)
                CopyAssembliesToWorkingDir(path);
            
            if(!_skipSourceCollection && _solutionToPackage.Directory != null)
            {
                CollateSourceRecursively(_solutionToPackage.Directory);

                //now zip the source

            }
        }

        List<FileInfo> _srcFilesFound = new List<FileInfo>();

        private void CollateSourceRecursively(DirectoryInfo dir)
        {
            //do not add all the source code from all the nuget packages!
            if(dir.Name.Equals("packages"))
                return;
            
            foreach (FileInfo file in dir.EnumerateFiles("*.cs"))
            {
                //already found this one
                if(_srcFilesFound.Any(f=>f.Name.Equals(file.Name)))
                    continue;
                
                _srcFilesFound.Add(file);
                file.CopyTo(Path.Combine(_srcDirectory.FullName,file.Name), true);
            }

            foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
                CollateSourceRecursively(subDir);
        }

        private void CopyAssembliesToWorkingDir(string projectDir, string assemblyName = null)
        {
            var projectOutputDir = Path.Combine(projectDir, "bin", "Debug");
            if (!Directory.Exists(projectOutputDir))
                throw new InvalidOperationException("The project dir (" + projectOutputDir + ") does not exist, has the project been built? You need to build the project before invoking this packager.");

            _pluginAssemblies.Add(new FileInfo(Path.Combine(projectOutputDir, assemblyName ?? Path.GetFileName(projectDir) + ".dll")));

            foreach (var srcFilename in Directory.GetFiles(projectOutputDir, "*.dll"))
            {
                var destFilename = Path.Combine(_workingDirectory.FullName, Path.GetFileName(srcFilename));
                if (File.Exists(destFilename)) continue;
                File.Copy(srcFilename, destFilename);
            }

            foreach (var srcFilename in Directory.GetFiles(projectOutputDir, "*.pdb"))
            {
                var destFilename = Path.Combine(_workingDirectory.FullName, Path.GetFileName(srcFilename));
                if (File.Exists(destFilename)) continue;
                File.Copy(srcFilename, destFilename);
            }
        }

        private void CreateWorkingDirectory()
        {
            _workingDirectory = _solutionToPackage.Directory.CreateSubdirectory(Path.GetRandomFileName());
            _outputDirectory = _workingDirectory.CreateSubdirectory("output");
            _srcDirectory = _outputDirectory.CreateSubdirectory("src");
        }

        readonly List<string> _blacklist = new List<string>();
        private DirectoryInfo _workingDirectory;
        private List<FileInfo> _pluginAssemblies;
        private DirectoryInfo _outputDirectory;
        private DirectoryInfo _srcDirectory;

        private void GenerateExclusionList()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                _blacklist.Add(assembly.GetName().Name);
        }

        private void AddWithDependencies(FileInfo dll)
        {
            string copyToLocation = Path.Combine(_outputDirectory.FullName, dll.Name);
            
            if(!File.Exists(copyToLocation))
                File.Copy(dll.FullName,copyToLocation);

            var pdb = new FileInfo(dll.FullName.Substring(0, dll.FullName.Length - ".dll".Length) + ".pdb");
            if (pdb.Exists)
            {
                var pdbCopyDestination = Path.Combine(_outputDirectory.FullName, pdb.Name);
                if (!File.Exists(pdbCopyDestination))
                    File.Copy(pdb.FullName, Path.Combine(_outputDirectory.FullName, pdb.Name));
            }
            
            foreach (AssemblyName name in Assembly.LoadFile(dll.FullName).GetReferencedAssemblies())
            {
                if(_blacklist.Contains(name.Name))
                    continue;

                if(LoadModuleAssembly.ProhibitedDllNames.Any(prohibitedName=>prohibitedName.Equals(name.Name +".dll")))
                    continue;
                
                var dependentDll = new FileInfo(Path.Combine(_workingDirectory.FullName,name.Name + ".dll"));

                if(!dependentDll.Exists)
                    Console.WriteLine("Could not find dependent dll "+ dependentDll.Name);
                else
                    AddWithDependencies(dependentDll);
            }

            
        }
    }
}
