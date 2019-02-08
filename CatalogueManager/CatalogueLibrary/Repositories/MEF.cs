// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// MEF stands for Managed Extensibility Framework which is a Microsoft library for building Extensions (Plugins) into programs.  It involves decoarting classes as
    /// [Export] or [InheritedExport] and defining contracts, importing constructors, paramters etc.  RDMP makes use of MEF in a limited fashion, it processes all 
    /// Exported classes into a SafeDirectoryCatalog (a collection of MEF AssemblyCatalogs/AggregateCatalog).
    /// 
    /// <para>This class provides support for downloading Plugins out of the Catalogue Database, identifying Exports and building the SafeDirectoryCatalog.  It also includes
    /// methods for creating instances of the exported Types.  Because MEF only gets you so far it also has some generally helpful reflection based methods such as 
    /// GetAllTypesFromAllKnownAssemblies.</para>
    /// </summary>
    public class MEF
    {
        public DirectoryInfo DownloadDirectory { get; private set; }

        public bool HaveDownloadedAllAssemblies = false;
        public SafeDirectoryCatalog SafeDirectoryCatalog;

        private readonly object ExportLocatorLock = new object();
        
        private readonly string _localPath = null;

        public MEF()
        {
            //try to use the app data folder to download MEF but also evaluate everything in _localPath
            _localPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);

            string _MEFPathAsString;

            try
            {
                _MEFPathAsString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MEF");
            }
            catch (Exception)//couldnt get the AppData/MEF directory so instead go to .\MEF\
            {
                if (_localPath == null)
                    throw new Exception("ApplicationData was not available to download MEF and neither apparently was Assembly.GetExecutingAssembly().GetName().CodeBase");

                _MEFPathAsString = Path.Combine(_localPath, "MEF");
            }
            DownloadDirectory = new DirectoryInfo(_MEFPathAsString);
        }



        public void Setup(SafeDirectoryCatalog result)
        {

            SafeDirectoryCatalog = result;
            HaveDownloadedAllAssemblies = true;
        }
        
        public void SetupMEFIfRequired()
        {
            if (!HaveDownloadedAllAssemblies)
                throw new NotSupportedException("MEF was not loaded by Startup?!!");
        }
        
        /// <summary>
        /// Makes the given Type appear as a MEF exported class.  Can be used to test your types without 
        /// building and committing an actual <see cref="Plugin"/>
        /// </summary>
        /// <param name="type"></param>
        public void AddTypeToCatalogForTesting(Type type)
        {
            SetupMEFIfRequired();

            SafeDirectoryCatalog.AddType(type);
        }

        public Dictionary<string, Exception> ListBadAssemblies()
        {
            SetupMEFIfRequired();

            return SafeDirectoryCatalog.BadAssembliesDictionary;
        }

        private Type ComposablePartExportType<T>(ComposablePartDefinition part)
        {
            string whatIAmLookingFor = GetMEFNameForType(typeof(T));
            try
            {
                if (part.ExportDefinitions.Any(
                    def => def.Metadata.ContainsKey("ExportTypeIdentity") &&
                           def.Metadata["ExportTypeIdentity"].Equals(whatIAmLookingFor)))
                {
                    return ReflectionModelServices.GetPartType(part).Value;
                }
                return null;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get ComposablePartExportType<T> where T was " + typeof(T).Name + " and part was " + part + " and we decided MEF would probably be calling T a " + whatIAmLookingFor, e);
            }
        }
        private Type ComposablePartExportType(Type t, ComposablePartDefinition part)
        {
            string whatIAmLookingFor = GetMEFNameForType(t);
            try
            {
                if (part.ExportDefinitions.Any(
                    def => def.Metadata.ContainsKey("ExportTypeIdentity") &&
                           def.Metadata["ExportTypeIdentity"].Equals(whatIAmLookingFor)))
                {
                    return ReflectionModelServices.GetPartType(part).Value;
                }
                return null;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get ComposablePartExportType<T> where T was " + t.Name + " and part was " + part + " and we decided MEF would probably be calling T a " + whatIAmLookingFor, e);
            }
        }

        /// <summary>
        /// Turns the legit C# name:
        /// DataLoadEngine.DataFlowPipeline.IDataFlowSource`1[[System.Data.DataTable, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
        /// 
        /// <para>Into a freaky MEF name:
        /// DataLoadEngine.DataFlowPipeline.IDataFlowSource(System.Data.DataTable)</para>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetMEFNameForType(Type t)
        {
            if (t.IsGenericType)
            {
                if (t.GenericTypeArguments.Count() != 1)
                    throw new NotSupportedException("Generic type has more than 1 token (e.g. T1,T2) so no idea what MEF would call it");
                string genericTypeName = t.GetGenericTypeDefinition().FullName;

                Debug.Assert(genericTypeName.EndsWith("`1"));
                genericTypeName = genericTypeName.Substring(0, genericTypeName.Length - "`1".Length);

                string underlyingType = t.GenericTypeArguments.Single().FullName;
                return genericTypeName + "(" + underlyingType + ")";
            }

            return t.FullName;
        }
        
        /// <summary>
        /// 
        /// <para>Turns the legit C# name: 
        /// DataLoadEngine.DataFlowPipeline.IDataFlowSource`1[[System.Data.DataTable, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</para>
        /// 
        /// <para>Into a proper C# code:
        /// IDataFlowSource&lt;DataTable&gt;</para>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetCSharpNameForType(Type t)
        {
            if (t.IsGenericType)
            {
                if (t.GenericTypeArguments.Count() != 1)
                    throw new NotSupportedException("Generic type has more than 1 token (e.g. T1,T2) so no idea what MEF would call it");
                string genericTypeName = t.GetGenericTypeDefinition().Name;

                Debug.Assert(genericTypeName.EndsWith("`1"));
                genericTypeName = genericTypeName.Substring(0, genericTypeName.Length - "`1".Length);

                string underlyingType = t.GenericTypeArguments.Single().Name;
                return genericTypeName + "<" + underlyingType + ">";
            }

            return t.Name;
        }

        public void CheckForVersionMismatches(ICheckNotifier notifier)
        {
            SetupMEFIfRequired();

            DirectoryInfo root = new DirectoryInfo(".");

            var binDirectoryFiles = root.EnumerateFiles().ToArray();

            foreach (FileInfo dllInMEFFolder in DownloadDirectory.GetFiles())
            {
                FileInfo dllInBinFolder = binDirectoryFiles.FirstOrDefault(f => f.Name.Equals(dllInMEFFolder.Name));

                if (dllInBinFolder != null)
                {
                    string md5Bin = UsefulStuff.MD5File(dllInBinFolder.FullName);
                    string md5Mef = UsefulStuff.MD5File(dllInMEFFolder.FullName);

                    if (!md5Bin.Equals(md5Mef))
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Different versions of the dll exist in MEF and BIN directory:" + Environment.NewLine +
                             dllInBinFolder.FullName + " (MD5=" + md5Bin + ")" + Environment.NewLine +
                             "Version:" + FileVersionInfo.GetVersionInfo(dllInBinFolder.FullName).FileVersion + Environment.NewLine +
                             "and" + Environment.NewLine +
                             dllInMEFFolder.FullName + " (MD5=" + md5Mef + ")" + Environment.NewLine +
                             "Version:" + FileVersionInfo.GetVersionInfo(dllInMEFFolder.FullName).FileVersion + Environment.NewLine
                        , CheckResult.Warning, null));

                    }
                }
            }
        }
        public IEnumerable<Type> GetTypes<T>()
        {
            SetupMEFIfRequired();

            return SafeDirectoryCatalog.Parts
                    .Select(part =>
                        ComposablePartExportType<T>(part))
                    .Where(t => t != null);
        }

        public IEnumerable<Type> GetTypes(Type type)
        {
            SetupMEFIfRequired();

            return SafeDirectoryCatalog.Parts
                    .Select(part =>
                        ComposablePartExportType(type, part))
                    .Where(t => t != null);
        }

        /// <summary>
        /// Returns all MEF exported classes decorated with the specified generic export e.g. [Export(typeof(IDataFlowComponent&lt;DataTable&gt;))]
        /// </summary>
        /// <param name="genericType"></param>
        /// <param name="typeOfT"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetGenericTypes(Type genericType, Type typeOfT)
        {
            return GetTypes(genericType.MakeGenericType(typeOfT));
        }

        public IEnumerable<Type> GetAllTypes()
        {
            SetupMEFIfRequired();

            return SafeDirectoryCatalog.GetAllTypes();
        }

        /// <summary>
        /// Creates an instance of the named class whcih must have a blank constructor
        /// 
        /// <para>IMPORTANT: this will create classes from the MEF Exports ONLY i.e. not any loaded type but has to be an explicitly labled Export of a LoadModuleAssembly</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toCreate"></param>
        /// <returns></returns>
        public T FactoryCreateA<T>(string toCreate)
        {
            using (var container = CreateCompositionContainer())
            {
                return LocateExportInContainerByTypeName<T>(container, toCreate);
            }
        }
        
        /// <summary>
        /// Creates an instance of the named class with a single constructor parameter
        /// 
        /// <para>IMPORTANT: this will create classes from the MEF Exports ONLY i.e. not any loaded type but has to be an explicitly labled Export of a LoadModuleAssembly</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="subTypeToCreate"></param>
        /// <param name="ctorParam1"></param>
        /// <returns></returns>
        public T FactoryCreateA<T, T1>(string subTypeToCreate, T1 ctorParam1)
        {
            using (var container = CreateCompositionContainer())
            {
                container.ComposeExportedValue(ctorParam1);
                return LocateExportInContainerByTypeName<T>(container, subTypeToCreate);
            }
        }


        public CompositionContainer CreateCompositionContainer()
        {
            SetupMEFIfRequired();
            return new CompositionContainer(SafeDirectoryCatalog, true);
        }


        private T LocateExportInContainerByTypeName<T>(CompositionContainer container, string typeToCreate)
        {
            List<Exception> compositonExceptions = new List<Exception>();
            lock (ExportLocatorLock)
            {
                foreach (Lazy<T> export in container.GetExports<T>())
                {
                    T canidiate;
                    
                    try
                    {
                        canidiate = export.Value;
                    }
                    catch (CompositionContractMismatchException ex)
                    {
                        //these only matter if we are unable to find what we are looking for
                        compositonExceptions.Add(ex);
                        
                        continue;
                    }

                    if (canidiate.GetType().FullName.Equals(typeToCreate))
                        return canidiate;
                }

                string compositionErrors = "";
                if (compositonExceptions.Any())
                    compositionErrors = "The following Composition errors were encountered:" + Environment.NewLine +
                                        string.Join(Environment.NewLine + Environment.NewLine,
                                            compositonExceptions.Select(ex => ex.Message));

                throw new KeyNotFoundException("Could not find [Export] of type " + typeToCreate + " using MEF " + " possibly because it is not declared as [Export(typeof(" + GetCSharpNameForType(typeof(T)) + "))]." + compositionErrors);
            }
        }
        
        public void ManufactureFactory(object importingFactory, object param1, object param2)
        {
            SetupMEFIfRequired();

            var container = new CompositionContainer(SafeDirectoryCatalog);
            container.ComposeExportedValue("CreateDatabase", param1);
            container.ComposeExportedValue("Version", param2);


            container.ComposeParts(importingFactory);
        }

        readonly Dictionary<string,Type> _cachedTypes = new Dictionary<string, Type>();
        public object _oLockcachedTypes = new object();

        public Type GetTypeByNameFromAnyLoadedAssembly(string name, Type expectedBaseClassType = null, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            lock (_oLockcachedTypes)
            {
                if (_cachedTypes.ContainsKey(name))
                    return _cachedTypes[name];

                Type toReturn = null;

                SetupMEFIfRequired();

                if (string.IsNullOrWhiteSpace(name))
                    return null;

                //could be custom imported type
                foreach (Type type in GetAllTypes())
                {
                    if (type == null)
                        throw new InvalidOperationException("The type array produced by GetAllTypes should not contain any nulls");

                    if (type.FullName.Equals(name,comparisonType))
                        toReturn =  type;
                }

                if(toReturn == null)
                {
                    List<Type> matches = new List<Type>();
                    List<Type> fullMatches = new List<Type>();

                    //could be basic type
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if(asm.FullName.Contains("nunit.engine"))
                            continue;

                        try
                        {
                            foreach (Type type in asm.GetTypes())
                            {
                                //type doesn't match base type
                                if (expectedBaseClassType != null)
                                    if (!expectedBaseClassType.IsAssignableFrom(type))
                                        continue;

                                if (type.FullName.Equals(name, comparisonType))
                                    fullMatches.Add(type);
                                else
                                    if (type.Name.Equals(name, comparisonType))
                                        matches.Add(type);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    if (fullMatches.Any())
                        if(fullMatches.Count>1)
                            throw new Exception("Found " + fullMatches.Count + " classes called '" + name + "':" + string.Join("," + Environment.NewLine, fullMatches.Select(m => m.AssemblyQualifiedName + " (Located:" + m.Assembly.CodeBase +")")));
                        else
                            toReturn = fullMatches.Single();

                    if (matches.Any())
                        if (matches.Count > 1)
                            throw new Exception("Found " + matches.Count + " classes called '" + name + "':" + string.Join("," + Environment.NewLine, matches.Select(m => m.FullName)));
                        else
                            toReturn = matches.Single();
                }

                //cache the answer even if it is null (could not resolve Type name)
                _cachedTypes.Add(name,toReturn);

                return toReturn;
            }
        }


        /// <summary>
        /// Lists every single Type in the current AppDomain (every assembly that is currently loaded) regardless of whether it is a MEF Export or not.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetAllTypesFromAllKnownAssemblies(out List<Exception> ex)
        {
            List<Type> toReturn = new List<Type>();
            ex = new List<Exception>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                try
                {
                    toReturn.AddRange(assembly.GetTypes());
                }
                catch (FileNotFoundException e)
                {
                    ex.Add(new Exception("Error loading module " + assembly.FullName, e));
                }
                catch (ReflectionTypeLoadException e)
                {
                    toReturn.AddRange(e.Types.Where(t => t != null)); // because the exception contains null types!
                    ex.Add(new Exception("Error loading module " + assembly.FullName, e));
                }


            //could be custom imported type
            toReturn.AddRange(GetAllTypes());

            return toReturn;
        }

        public string DescribeBadAssembliesIfAny(string separator = " ")
        {
            var baddies = ListBadAssemblies().ToArray();
            if (!baddies.Any())
                return null;

            return ListBadAssemblies()
                .Aggregate("Bad Assemblies:",
                    (prev, next) =>
                        prev + "Dll:" + next.Key + separator + " Exception:" +
                        ExceptionHelper.ExceptionToListOfInnerMessages(next.Value) + separator);
        }
    }
}
