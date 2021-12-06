// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static string[] Ignore = new string[]
        {
            "mscorelib.dll",
            "Hunspellx64.dll",
            "Hunspellx86.dll",
            "NuGet.Squirrel.dll",
        };

        /// <summary>
        /// Assemblies succesfully loaded
        /// </summary>
        public Dictionary<string, Assembly> GoodAssemblies = new Dictionary<string, Assembly>();
        public Dictionary<Assembly,Type[]> TypesByAssembly = new Dictionary<Assembly, Type[]>();
        public HashSet<Type> Types = new HashSet<Type>();
        public Dictionary<string,Type> TypesByName = new Dictionary<string, Type>();
        private object typesByNameLock = new object();

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

            var files = new HashSet<FileInfo>();
                       
            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);//empty directory 

                foreach(var f in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
                {
                    var newOne = new FileInfo(f);
                    var existing = files.SingleOrDefault(d => d.Name.Equals(newOne.Name));

                    if (existing != null)
                    {
                        var existingOneVersion = FileVersionInfo.GetVersionInfo(existing.FullName);
                        var newOneVersion = FileVersionInfo.GetVersionInfo(newOne.FullName);

                        FileInfo winner = null;

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
            foreach(FileInfo f in files)
            {
                Assembly ass = null;
                if(Ignore.Contains(f.Name))
                    continue;
                if(!IsDotNetAssembly(f.FullName))
                {
                    listener?.OnCheckPerformed(new CheckEventArgs($"Skipped '{f}' because it is not a dotnet assembly (according to dll header)", CheckResult.Success));
                    continue;
                }

                try
                {
                    ass = AssemblyResolver.LoadFile(f);
                    AddTypes(f,ass,ass.GetTypes(),listener);
                }
                catch(ReflectionTypeLoadException ex)
                {
                    //if we loaded the assembly and some types
                    if(ex.Types.Any() && ass != null)
                    {
                        listener?.OnCheckPerformed(new CheckEventArgs(
                            $"Loaded {ex.Types.Count(t => t != null)}/{ex.Types.Length} Types from {f.Name}",CheckResult.Warning,ex));
                        AddTypes(f,ass,ex.Types,listener); //the assembly is bad but at least some of the Types were legit
                    }
                    else
                        AddBadAssembly(f,ex,listener); //the assembly could not be loaded properly
                }
                catch (BadImageFormatException)
                {
                    listener?.OnCheckPerformed(new CheckEventArgs($"Did not load '{f}' because it is not a dotnet assembly", CheckResult.Success));
                }
                catch (Exception ex)
                { 
                    AddBadAssembly(f,ex,listener);
                }
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
            if (!BadAssembliesDictionary.ContainsKey(f.FullName)) //couldn't load anything out of it
            {
                BadAssembliesDictionary.Add(f.FullName, ex);
                listener?.OnCheckPerformed(new CheckEventArgs($"Encountered Bad Assembly loading {f.FullName} into memory", CheckResult.Fail, ex));
            }
        }

        private void AddTypes(FileInfo f, Assembly ass, Type[] types, ICheckNotifier listener)
        {
            TypesByAssembly.Add(ass,types.Where(t=>t != null).ToArray());
            
            foreach(Type t in types.Where(t=>t != null))
                if(!TypesByName.ContainsKey(t.FullName))
                    AddType(t.FullName,t);

            GoodAssemblies.Add(f.FullName, ass);

            //tell them as we go how far we are through
            listener?.OnCheckPerformed(new CheckEventArgs($"Successfully loaded Assembly {f.FullName} into memory", CheckResult.Success));
        }

        internal void AddType(Type type)
        {
            AddType(type.FullName,type);
        }

        internal void AddType(string typeNameOrAlias, Type type)
        {
            lock(typesByNameLock)
            {
                //only add it if it is novel
                if(!TypesByName.ContainsKey(typeNameOrAlias))
                    TypesByName.Add(typeNameOrAlias,type);

                Types.Add(type);
            }
        }

        public IEnumerable<Type> GetAllTypes()
        {
            lock(typesByNameLock)
                return Types;
        }

        /// <summary>
        /// See https://stackoverflow.com/a/29643803 by Jeremy Thompson
        /// </summary>
        /// <param name="peFile"></param>
        /// <returns></returns>
        public static bool IsDotNetAssembly(string peFile)
        {
            uint[] dataDictionaryRVA = new uint[16];
            uint[] dataDictionarySize = new uint[16];


            using Stream fs = new FileStream(peFile, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new BinaryReader(fs);

            //PE Header starts @ 0x3C (60). It's a 4 byte header.
            fs.Position = 0x3C;

            var peHeader = reader.ReadUInt32();

            //Moving to PE Header start location...
            fs.Position = peHeader;
            if (reader.ReadUInt32() != 0x4550)
                return false; // PE signature "PE\0\0" as integer

            //We can also show all these value, but we will be       
            //limiting to the CLI header test.

            reader.ReadUInt16();                    // Machine type, eg 0x8664 for amd64
            reader.ReadUInt16();                    // Number of sections
            reader.ReadUInt32();                    // When the file was created/modified (seconds since start of 1970)
            reader.ReadUInt32();                    // Offset to symbol table if present
            reader.ReadUInt32();                    // Number of symbols
            var ohSize=reader.ReadUInt16();  // Size of 'optional' header
            reader.ReadUInt16();                    // Characteristics; we probably want 0x2000 ('is DLL')

            if (ohSize == 0)
                return false;   // Can't be a valid .Net DLL without the optional header section

            /*
             * 28 bytes 'standard' headers
             * 68 bytes 'NT-specific' headers
             * 128 bytes for 16 DataDictionaries (each RVA+size DWORD tuples)
             * 15th DataDictionary consist of CLR header! if it's 0, it's not a CLR file :)
             */
            fs.Seek(0x60, SeekOrigin.Current);  // Skip past standard+NT headers
            for (int i = 0; i < 15; i++)
            {
                dataDictionaryRVA[i] = reader.ReadUInt32();
                dataDictionarySize[i] = reader.ReadUInt32();
            }

            return (dataDictionaryRVA[14] != 0);    // Non-zero for CLR header present
        }
    }
}
