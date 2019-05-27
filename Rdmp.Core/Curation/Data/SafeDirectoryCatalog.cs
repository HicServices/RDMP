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
using System.Runtime.Loader;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data
{
    /// <summary>
    /// Managed Extensibility Framework (MEF) Catalog of Class Types that are exposed via .  Constructing this class will process the directories
    /// provided (usually the current directory and the %appdata%\MEF directory).  Each dll (Assembly) is classed as either a 'Bad Assembly' (could not be loaded) or a
    /// 'Good Assembly' (was loaded).  GoodAssemblies are exposed as AssemblyCatalogs (MEF) which area  collection of ComposablePartDefinition (Parts).
    /// 
    /// <para>These can then be constructed/queried like you normally do with MEF (See MEF.LocateExportInContainerByTypeName).</para>
    /// 
    /// <para>This class deliberately tries to filter interfaces and abstract class exports since the goal is to construct instances of plugin classes</para>
    /// </summary>
    public class SafeDirectoryCatalog
    {
        /// <summary>
        /// Assemblies succesfully loaded
        /// </summary>
        public Dictionary<string, Assembly> GoodAssemblies = new Dictionary<string, Assembly>();
        public Dictionary<Assembly,Type[]> Types = new Dictionary<Assembly, Type[]>();
        public Dictionary<string,Type> TypesByName = new Dictionary<string, Type>();
        /// <summary>
        /// Assemblies which could not be loaded
        /// </summary>
        public Dictionary<string,Exception> BadAssembliesDictionary { get; set; }
        
        /// <summary>
        /// Creates a new list of MEF plugin classes from the dlls/files in the directory list provided
        /// </summary>
        /// <param name="directories"></param>
        public SafeDirectoryCatalog(params string[] directories):this(null,directories)
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
                        listener.OnCheckPerformed(new CheckEventArgs("Found 2 copies of " + newOne.Name +".  Loaded will be '" + existing.FullName +"'.  Rejected one will be '" + newOne.FullName +"'",CheckResult.Warning));
                    else
                        files.Add(newOne);
                }
            }
            
            // Find and load all the DLLs which are not ignored
            foreach(FileInfo f in files)
            {
                Assembly ass = null;

                try
                {
                    ass = AssemblyResolver.LoadFile(f);
                    AddTypes(f,ass,ass.GetTypes(),listener);
                }
                catch(ReflectionTypeLoadException ex)
                {
                    //if we loaded thea ssembly and some types
                    if(ex.Types.Any() && ass != null)
                        AddTypes(f,ass,ex.Types,listener);
                    else
                        AddBadAssembly(f,ex,listener);
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
            Types.Add(ass,types.Where(t=>t != null).ToArray());
            
            foreach(Type t in types.Where(t=>t != null))
                if(!TypesByName.ContainsKey(t.FullName))
                    TypesByName.Add(t.FullName,t);

            GoodAssemblies.Add(f.FullName, ass);

            //tell them as we go how far we are through
            if (listener != null)
                listener.OnCheckPerformed(new CheckEventArgs("Successfully loaded Assembly " + f.FullName + " into memory", CheckResult.Success));
        }

        HashSet<Type> _injectedTypes = new HashSet<Type>();

        internal void AddType(Type type)
        {
            //Only add novel types
            if(_injectedTypes.Contains(type))
                return;

            //and record the explicit injection
            _injectedTypes.Add(type);
        }

        public IEnumerable<Type> GetAllTypes()
        {
            return Types.SelectMany(a=>a.Value).Union(_injectedTypes);
        }
    }
}
