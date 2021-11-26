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

                        // if we already have a copy of this exactl dll we don't care bout loading it
                        if(FileVersionsAreEqual(newOneVersion, existingOneVersion))
                        {
                            // no need to spam user with warnings about duplicated dlls
                            DuplicateDllsIgnored++;
                            continue;
                        }
                        else
                        // pick the newer one
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
                    //if we loaded thea ssembly and some types
                    if(ex.Types.Any() && ass != null)
                    {
                        if(listener != null)
                            listener.OnCheckPerformed(new CheckEventArgs("Loaded " + ex.Types.Count(t=>t!= null) + "/" + ex.Types.Length + " Types from " + f.Name  ,CheckResult.Warning,ex));
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
                if(listener != null)
                    listener.OnCheckPerformed(new CheckEventArgs("Encountered Bad Assembly " + f.FullName + " into memory", CheckResult.Fail, ex));
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
            if (listener != null)
                listener.OnCheckPerformed(new CheckEventArgs("Successfully loaded Assembly " + f.FullName + " into memory", CheckResult.Success));
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
        private bool IsDotNetAssembly(string peFile)
        {
            uint peHeader;
            uint peHeaderSignature;
            ushort machine;
            ushort sections;
            uint timestamp;
            uint pSymbolTable;
            uint noOfSymbol;
            ushort optionalHeaderSize;
            ushort characteristics;
            ushort dataDictionaryStart;
            uint[] dataDictionaryRVA = new uint[16];
            uint[] dataDictionarySize = new uint[16];


            using Stream fs = new FileStream(peFile, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            //PE Header starts @ 0x3C (60). Its a 4 byte header.
            fs.Position = 0x3C;

            peHeader = reader.ReadUInt32();

            //Moving to PE Header start location...
            fs.Position = peHeader;
            peHeaderSignature = reader.ReadUInt32();

            //We can also show all these value, but we will be       
            //limiting to the CLI header test.

            machine = reader.ReadUInt16();
            sections = reader.ReadUInt16();
            timestamp = reader.ReadUInt32();
            pSymbolTable = reader.ReadUInt32();
            noOfSymbol = reader.ReadUInt32();
            optionalHeaderSize = reader.ReadUInt16();
            characteristics = reader.ReadUInt16();

            /*
                Now we are at the end of the PE Header and from here, the
                            PE Optional Headers starts...
                    To go directly to the datadictionary, we'll increase the      
                    stream’s current position to with 96 (0x60). 96 because,
                            28 for Standard fields
                            68 for NT-specific fields
                From here DataDictionary starts...and its of total 128 bytes. DataDictionay has 16 directories in total,
                doing simple maths 128/16 = 8.
                So each directory is of 8 bytes.
                            In this 8 bytes, 4 bytes is of RVA and 4 bytes of Size.

                btw, the 15th directory consist of CLR header! if its 0, its not a CLR file :)
         */

            dataDictionaryStart = Convert.ToUInt16(Convert.ToUInt16(fs.Position) + 0x60);
            fs.Position = dataDictionaryStart;
            for (int i = 0; i < 15; i++)
            {
                dataDictionaryRVA[i] = reader.ReadUInt32();
                dataDictionarySize[i] = reader.ReadUInt32();
            }
            if (dataDictionaryRVA[14] == 0)
            {
                Console.WriteLine("This is NOT a valid CLR File!!");
                return false;
            }
            else
            {
                Console.WriteLine("This is a valid CLR File..");
                return true;
            }
        }
    }
}
