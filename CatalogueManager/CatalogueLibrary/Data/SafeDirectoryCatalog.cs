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
    /// provided (usually the current directory and the %appdata%\MEF directory).  Each dll (Assembly) is classed as either a 'BadAssembly' (could not be loaded) or a
    /// 'GoodAssembly' (was loaded).  GoodAssemblies are exposed as AssemblyCatalogs (MEF) which area  collection of ComposablePartDefinition (Parts).
    /// 
    /// These can then be constructed/queried like you normally do with MEF (See MEF.LocateExportInContainerByTypeName).
    /// </summary>
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        Regex[] blacklist = new Regex[] { new Regex("SciLexer6?4?.dll$") };

        public Dictionary<string, Assembly> GoodAssemblies = new Dictionary<string, Assembly>();
        public Dictionary<string,Exception> BadAssembliesDictionary { get; set; }

        public Dictionary<string, AssemblyCatalog> PartsByFileDictionary = new Dictionary<string, AssemblyCatalog>();

        private readonly AggregateCatalog _catalog;


        public SafeDirectoryCatalog(params string[] directories):this(null,directories)
        {
        }

        public SafeDirectoryCatalog(ICheckNotifier listener, params string[] directories)
        {
            BadAssembliesDictionary = new Dictionary<string, Exception>();

            var files = new List<string>();

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);//empty directory 

                files.AddRange(Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories));
            }
                
            _catalog = new AggregateCatalog();
            
            for (int index = 0; index < files.Count; index++)
            {
                var file = files[index];

                if (blacklist.Any(r => r.IsMatch(file)))
                {
                    if(listener != null)
                        listener.OnCheckPerformed(new CheckEventArgs("skipping blacklisted dll " + file, CheckResult.Success));
                    continue;
                }

                int percentage = (int)(((float)(index+1.0)/files.Count)*100.0);


                try
                {
                    //we have already processed this one
                    if (GoodAssemblies.ContainsKey(file))
                    {
                        if(listener != null)
                            listener.OnCheckPerformed(new CheckEventArgs(percentage + "% - File " + file + " already loaded", CheckResult.Warning));

                        continue;
                    }

                    var asmCat = new AssemblyCatalog(file);

                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);

                    //Add the parts to the parts by file dictionary too so if you want to know what exports come from what file you can do so
                    PartsByFileDictionary.Add(file,asmCat);

                    //no bombing occurred so must be a good file
                    GoodAssemblies.Add(file, asmCat.Assembly);

                    //tell them as we go how far we are through
                    if (listener != null)
                        listener.OnCheckPerformed(new CheckEventArgs(percentage +"% - successfully loaded Assembly " + file + " into memory", CheckResult.Success));
                }
                catch (Exception ex)
                {
                    if (!BadAssembliesDictionary.ContainsKey(file))
                    {
                        BadAssembliesDictionary.Add(file, ex);
                        if(listener != null)
                            listener.OnCheckPerformed(new CheckEventArgs(percentage + "% - Encountered Bad Assembly " + file + " into memory", CheckResult.Fail, ex));
                    }
                }
            }
        }

        HashSet<Type> _injectedTypes = new HashSet<Type>();

        public void AddType(Type type)
        {
            //Only add novel types
            if(_injectedTypes.Contains(type))
                return;

            //inject the type into the MEF Catalogs
            _catalog.Catalogs.Add(new TypeCatalog(type));

            //and record the explicit injection
            _injectedTypes.Add(type);
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }

        private static readonly object PartIteratorLock = new object();
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