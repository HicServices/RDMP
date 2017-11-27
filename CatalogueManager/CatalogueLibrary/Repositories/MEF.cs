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
    public class MEF
    {
        public DirectoryInfo DownloadDirectory { get; private set; }

        public bool HaveDownloadedAllAssemblies = false;
        public SafeDirectoryCatalog SafeDirectoryCatalog;

        private object MEFDownloadLock = new object();
        private readonly object ExportLocatorLock = new object();
        
        private readonly CatalogueRepository _repository;

        private readonly string _localPath = null;

        public MEF(CatalogueRepository repository)
        {
            _repository = repository;

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

        /// <summary>
        /// Turns the legit C# name:
        /// DataLoadEngine.DataFlowPipeline.IDataFlowSource`1[[System.Data.DataTable, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
        /// 
        /// Into a freaky MEF name:
        /// DataLoadEngine.DataFlowPipeline.IDataFlowSource(System.Data.DataTable)
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
        /// Turns the legit C# name: 
        /// DataLoadEngine.DataFlowPipeline.IDataFlowSource`1[[System.Data.DataTable, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
        /// 
        /// Into a proper C# code:
        /// IDataFlowSource&lt;DataTable&gt;
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

        public IEnumerable<Type> GetAllTypes()
        {
            SetupMEFIfRequired();

            return SafeDirectoryCatalog.GetAllTypes();
        }

        /// <summary>
        /// Creates an instance of the named class whcih must have a blank constructor
        /// 
        /// IMPORTANT: this will create classes from the MEF Exports ONLY i.e. not any loaded type but has to be an explicitly labled Export of a LoadModuleAssembly
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
        /// IMPORTANT: this will create classes from the MEF Exports ONLY i.e. not any loaded type but has to be an explicitly labled Export of a LoadModuleAssembly
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


        public Type GetTypeByNameFromAnyLoadedAssembly(string name, Type expectedBaseClassType = null)
        {
            SetupMEFIfRequired();

            if (string.IsNullOrWhiteSpace(name))
                return null;

            //could be custom imported type
            foreach (Type type in GetAllTypes())
            {
                if (type == null)
                    throw new InvalidOperationException("The type array produced by GetAllTypes should not contain any nulls");

                if (type.FullName.Equals(name))
                    return type;
            }

            List<Type> matches = new List<Type>();
            List<Type> fullMatches = new List<Type>();

            //could be basic type
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        //type doesn't match base type
                        if (expectedBaseClassType != null)
                            if (!expectedBaseClassType.IsAssignableFrom(type))
                                continue;

                        if (type.FullName.Equals(name))
                            fullMatches.Add(type);
                        else
                            if (type.Name.Equals(name))
                                matches.Add(type);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (fullMatches.Any())
                return fullMatches.Single();

            if (matches.Any())
                if(matches.Count > 1)
                    throw new Exception("Found " + matches.Count + " classes called '" + name + "':" + string.Join("," + Environment.NewLine,matches.Select(m=>m.FullName)));
                else
                    return matches.Single();

            return null;
        }



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
                    toReturn.AddRange(e.Types);
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
