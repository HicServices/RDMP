// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Managed Extensibility Framework (MEF) Catalog of Class Types that are exposed via [InheritedExport(typeof(X))].  Constructing this class will process the directories
    /// provided (usually the current directory and the %appdata%\MEF directory).  Each dll (Assembly) is classed as either a 'Bad Assembly' (could not be loaded) or a
    /// 'Good Assembly' (was loaded).  GoodAssemblies are exposed as AssemblyCatalogs (MEF) which area  collection of ComposablePartDefinition (Parts).
    /// 
    /// <para>These can then be constructed/queried like you normally do with MEF (See MEF.LocateExportInContainerByTypeName).</para>
    /// 
    /// <para>This class deliberately tries to filter interfaces and abstract class exports since the goal is to construct instances of plugin classes</para>
    /// </summary>
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        Regex[] blacklist = new Regex[] { new Regex("SciLexer6?4?.dll$") };

        /// <summary>
        /// Assemblies succesfully loaded
        /// </summary>
        public Dictionary<string, Assembly> GoodAssemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// Assemblies which could not be loaded
        /// </summary>
        public Dictionary<string,Exception> BadAssembliesDictionary { get; set; }

        /// <summary>
        /// All MEF Export classes found in all assemblies loaded
        /// </summary>
        public Dictionary<string, AssemblyCatalog> PartsByFileDictionary = new Dictionary<string, AssemblyCatalog>();

        private readonly AggregateCatalog _catalog;

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
                
            _catalog = new AggregateCatalog();
            
            foreach(FileInfo f in files)
            {
                if (blacklist.Any(r => r.IsMatch(f.Name)))
                {
                    if(listener != null)
                        listener.OnCheckPerformed(new CheckEventArgs("skipping blacklisted dll " + f, CheckResult.Success));
                    continue;
                }

                try
                {
                    //we have already processed this one
                    if (GoodAssemblies.ContainsKey(f.FullName))
                    {
                        if(listener != null)
                            listener.OnCheckPerformed(new CheckEventArgs("File " + f.FullName + " already loaded", CheckResult.Warning));

                        continue;
                    }

                    var asmCat = new AssemblyCatalog(f.FullName);

                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);

                    //Add the parts to the parts by file dictionary too so if you want to know what exports come from what file you can do so
                    PartsByFileDictionary.Add(f.FullName,asmCat);

                    //no bombing occurred so must be a good file
                    GoodAssemblies.Add(f.FullName, asmCat.Assembly);

                    //tell them as we go how far we are through
                    if (listener != null)
                        listener.OnCheckPerformed(new CheckEventArgs("Successfully loaded Assembly " + f.FullName + " into memory", CheckResult.Success));
                }
                catch (Exception ex)
                {
                    if (!BadAssembliesDictionary.ContainsKey(f.FullName))
                    {
                        BadAssembliesDictionary.Add(f.FullName, ex);
                        if(listener != null)
                            listener.OnCheckPerformed(new CheckEventArgs("Encountered Bad Assembly " + f.FullName + " into memory", CheckResult.Fail, ex));
                    }
                }
            }
        }

        HashSet<Type> _injectedTypes = new HashSet<Type>();

        internal void AddType(Type type)
        {
            //Only add novel types
            if(_injectedTypes.Contains(type))
                return;

            //inject the type into the MEF Catalogs
            _catalog.Catalogs.Add(new TypeCatalog(type));

            //and record the explicit injection
            _injectedTypes.Add(type);
        }

        /// <summary>
        /// Gets all MEF exported classes that were succesfully loaded
        /// </summary>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }

        private static readonly object PartIteratorLock = new object();

        /// <summary>
        /// Returns <see cref="Parts"/> as System.Type
        /// 
        /// <para>Excludes interfaces and abstract classes</para>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> GetAllTypes()
        {
            lock (PartIteratorLock)
            {
                foreach (var part in Parts)
                {
                    if (part == null)
                        throw new Exception("Somehow a null has gotten into the Parts array");

                    if (part.ExportDefinitions == null)
                        throw new Exception("ExportDefinitions is null for " + part);

                    if (!part.ExportDefinitions.Any(def => def != null && def.Metadata != null && def.Metadata.ContainsKey("ExportTypeIdentity")))
                        continue;

                    var partType = ReflectionModelServices.GetPartType(part);
                    if (partType == null)
                        throw new Exception("Could not get part type for " + part);

                    Type value = partType.Value;

                    if (value.IsAbstract || value.IsInterface)
                        continue;

                    if (value != null)
                        yield return value;
                }
            }
        }
    }
}
