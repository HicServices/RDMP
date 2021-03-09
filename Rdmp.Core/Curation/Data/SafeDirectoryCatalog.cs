// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
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
                    
                    if(existing != null)
                        listener?.OnCheckPerformed(new CheckEventArgs("Found 2 copies of " + newOne.Name +".  Loaded will be '" + existing.FullName +"'.  Rejected one will be '" + newOne.FullName +"'",CheckResult.Success));
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
                catch(Exception ex)
                { 
                    AddBadAssembly(f,ex,listener);
                }
            }
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
    }
}
