// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data
{
    /// <summary>
    /// Type manager which supports loading assemblies from both the bin directory and plugin directories.  Types discovered are indexed
    /// according to name so they can be built on demand later on.  
    /// 
    /// <para>Handles assembly resolution problems, binding redirection and partial assembly loading (e.g. if only some of the Types in the 
    /// assembly could be resolved).</para>
    /// </summary>
    public class SafeDirectoryCatalog
    {
        /// <summary>
        /// These assemblies do not load correctly and should be ignored (they produce warnings on Startup)
        /// </summary>
        public static string[] Ignore = {
            "mscorlib.dll",
            "Hunspellx64.dll",
            "Hunspellx86.dll"
        };

        /// <summary>
        /// Assemblies successfully loaded
        /// </summary>
        public readonly ConcurrentDictionary<string, Assembly> GoodAssemblies = new ();
        public readonly ConcurrentDictionary<Assembly,Type[]> TypesByAssembly = new ();

        private object oTypesLock = new object();
        public HashSet<Type> Types = new HashSet<Type>();
        public ConcurrentDictionary<string,Type> TypesByName = new ();

        /// <summary>
        /// The number of ignored dlls that were skipped because another copy was already seen
        /// with the same major/minor/build version
        /// </summary>
        public int DuplicateDllsIgnored { get; set; } = 0;

        /// <summary>
        /// Assemblies which could not be loaded
        /// </summary>
        public Dictionary<string,Exception> BadAssembliesDictionary { get; set; }
        
        /// <summary>
        /// Delegate for skipping certain dlls
        /// </summary>
        public static Func<FileInfo,bool> IgnoreDll { get; set; }

        /// <summary>
        /// Creates a new list of MEF plugin classes from the dlls/files in the directory list provided
        /// </summary>
        /// <param name="directories"></param>
        public SafeDirectoryCatalog(params string[] directories):this(new IgnoreAllErrorsCheckNotifier(), directories)
        {
        }

        

        /// <inheritdoc cref="SafeDirectoryCatalog(string[])"/>
        public SafeDirectoryCatalog(ICheckNotifier listener, params string[] directories)
        {
            BadAssembliesDictionary = new Dictionary<string, Exception>();

            TypesByAssembly.TryAdd(typeof(SafeDirectoryCatalog).Assembly,
                typeof(SafeDirectoryCatalog).Assembly.GetTypes());
            foreach(var t in typeof(SafeDirectoryCatalog).Assembly.GetTypes())
                AddType(t);

            var files = new HashSet<FileInfo>();
                       
            foreach (var directory in directories)
            {
                if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory); //empty directory 

                foreach(var f in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
                {
                    var newOne = new FileInfo(f);
                    var existing = files.SingleOrDefault(d => d.Name.Equals(newOne.Name));

                    // don't load the cli dir
                    if (IgnoreDll != null && IgnoreDll(newOne))
                        continue;

                    if (existing != null)
                    {
                        var existingOneVersion = FileVersionInfo.GetVersionInfo(existing.FullName);
                        var newOneVersion = FileVersionInfo.GetVersionInfo(newOne.FullName);

                        FileInfo winner;

                        // if we already have a copy of this exact dll we don't care about loading it
                        if(FileVersionsAreEqual(newOneVersion, existingOneVersion))
                        {
                            // no need to spam user with warnings about duplicated dlls
                            DuplicateDllsIgnored++;
                            continue;
                        }

                        if (FileVersionGreaterThan(newOneVersion, existingOneVersion)) 
                        {
                            files.Remove(existing);
                            files.Add(newOne);
                            winner = newOne;
                        }
                        else
                        {
                            winner = existing;
                        }

                        listener?.OnCheckPerformed(new CheckEventArgs($"Found 2 copies of {newOne.Name }.  They were {existing.FullName} ({existingOneVersion.FileVersion}) and {newOne.FullName} ({newOneVersion.FileVersion}).  Only {winner.FullName} will be loaded", CheckResult.Success));
                    }
                    else
                        files.Add(newOne);
                }
            }

            // Find and load all the DLLs which are not ignored
            Parallel.ForEach(files, f=>LoadDll(f,listener));
        }

        private void LoadDll(FileInfo f, ICheckNotifier listener)
        {
            Assembly ass = null;
            if (Ignore.Contains(f.Name))
                return;
            
            if(!IsDotNetAssembly(f))
            {
                listener?.OnCheckPerformed(new CheckEventArgs($"Skipped '{f}' because it is not a dotnet assembly (according to dll header)", CheckResult.Success));
            }
            try
            {
                ass = AssemblyResolver.LoadFile(f);
                AddTypes(f, ass, ass.GetTypes(), listener);
            }
            catch (ReflectionTypeLoadException ex)
            {
                //if we loaded the assembly and some types
                if (ex.Types.Any() && ass != null)
                {
                    listener?.OnCheckPerformed(new CheckEventArgs(
                        ErrorCodes.CouldOnlyHalfLoadDll,
                        ex,null,
                        ex.Types.Count(t => t != null),
                        ex.Types.Length,
                        f.Name));

                    AddTypes(f, ass, ex.Types, listener); //the assembly is bad but at least some of the Types were legit
                }
                else
                    AddBadAssembly(f, ex, listener); //the assembly could not be loaded properly
            }
            catch (BadImageFormatException)
            {
                listener?.OnCheckPerformed(new CheckEventArgs($"Did not load '{f}' because it is not a dotnet assembly", CheckResult.Success));
            }
            catch (Exception ex)
            {
                AddBadAssembly(f, ex, listener);
            }
            
        }

        /// <summary>
        /// Returns true if the two versions have the same FileMajorPart, FileMinorPart and FileBuildPart version numbers
        /// </summary>
        /// <param name="newOneVersion"></param>
        /// <param name="existingOneVersion"></param>
        /// <returns></returns>
        private static bool FileVersionsAreEqual(FileVersionInfo newOneVersion, FileVersionInfo existingOneVersion)
        {
            return newOneVersion.FileMajorPart == existingOneVersion.FileMajorPart &&
                newOneVersion.FileMinorPart == existingOneVersion.FileMinorPart &&
                newOneVersion.FileBuildPart == existingOneVersion.FileBuildPart;
        }

        /// <summary>
        /// Returns true if the <paramref name="newOneVersion"/> is a later version than <paramref name="existingOneVersion"/>.
        /// Does not consider private build part e.g. 1.0.0-alpha1 and 1.0.0-alpha2 are not considered different
        /// </summary>
        /// <param name="newOneVersion"></param>
        /// <param name="existingOneVersion"></param>
        /// <returns></returns>
        private static bool FileVersionGreaterThan(FileVersionInfo newOneVersion, FileVersionInfo existingOneVersion)
        {
            if (newOneVersion.FileMajorPart > existingOneVersion.FileMajorPart)
                return true;
            // This is needed to ensure that 1.2.0 is seen as older than 2.0.0
            if (newOneVersion.FileMajorPart < existingOneVersion.FileMajorPart)
                return false;

            // First part equal, so use second as tie-breaker:
            if (newOneVersion.FileMinorPart > existingOneVersion.FileMinorPart)
                return true;
            if (newOneVersion.FileMinorPart < existingOneVersion.FileMinorPart)
                return false;

            if (newOneVersion.FileBuildPart > existingOneVersion.FileBuildPart)
                return true;

            return false;
        }

        private void AddBadAssembly(FileInfo f, Exception ex,ICheckNotifier listener)
        {
            if (BadAssembliesDictionary.ContainsKey(f.FullName)) return;    // Only report each failure once
            BadAssembliesDictionary.Add(f.FullName, ex);
            listener?.OnCheckPerformed(new CheckEventArgs(ErrorCodes.CouldNotLoadDll, null,ex,f.FullName));
        }

        private void AddTypes(FileInfo f, Assembly ass, Type[] types, ICheckNotifier listener)
        {
            types = types.Where(t => t != null).ToArray();
            TypesByAssembly.TryAdd(ass,types);
            
            foreach(var t in types)
                if(t.FullName != null && !TypesByName.ContainsKey(t.FullName))
                    AddType(t.FullName,t);

            GoodAssemblies.TryAdd(f.FullName, ass);

            //tell them as we go how far we are through
            listener?.OnCheckPerformed(new CheckEventArgs($"Successfully loaded Assembly {f.FullName} into memory", CheckResult.Success));
        }

        internal void AddType(Type type)
        {
            AddType(type.FullName,type);
        }

        internal void AddType(string typeNameOrAlias, Type type)
        {
            //only add it if it is novel
            if (!TypesByName.ContainsKey(typeNameOrAlias))
                TypesByName.TryAdd(typeNameOrAlias, type);

            lock (oTypesLock)
            {
                Types.Add(type);
            }
        }

        public IEnumerable<Type> GetAllTypes()
        {
            lock(oTypesLock)
                return Types;
        }
        public static bool IsDotNetAssembly(FileInfo f)
        {
            try
            {
                AssemblyName testAssembly = AssemblyName.GetAssemblyName(f.FullName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
