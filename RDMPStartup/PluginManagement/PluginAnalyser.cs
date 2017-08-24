using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using Mono.Reflection;

namespace RDMPStartup.PluginManagement
{
    public class PluginAnalyser
    {
        public Plugin Plugin { get; set; }
        public DirectoryInfo PluginDirectory { get; set; }
        public SafeDirectoryCatalog Catalog { get; set; }

        public Dictionary<LoadModuleAssembly,PluginAnalyserReport> Reports = new Dictionary<LoadModuleAssembly, PluginAnalyserReport>();
        public event PluginAnalyserProgressEventHandler ProgressMade = delegate { };

        public PluginAnalyser(Plugin plugin,DirectoryInfo pluginDirectory,SafeDirectoryCatalog catalog)
        {
            Plugin = plugin;
            PluginDirectory = pluginDirectory;
            Catalog = catalog;
        }

        public void Analyse()
        {
            int progress = 0;
            int maxProgress = Plugin.LoadModuleAssemblies.Count();

            foreach (LoadModuleAssembly lma in Plugin.LoadModuleAssemblies)
            {
                ProgressMade(this, new PluginAnalyserProgressEventArgs(progress, maxProgress, lma));

                var report = new PluginAnalyserReport();
                Reports.Add(lma, report);

                if(!PluginDirectory.Exists)
                    PluginDirectory.Create();

                var file = PluginDirectory.GetFiles().SingleOrDefault(f => f.Name.Equals(lma.Name));

                
                //file exists?
                if (file == null)
                    report.Status = PluginAssemblyStatus.Healthy;
                else
                if(file.Name.Equals("src.zip"))
                    report.Status = PluginAssemblyStatus.Healthy;
                else if (Catalog.BadAssembliesDictionary.ContainsKey(file.FullName)) //is bad assembly?
                {
                    report.Status = PluginAssemblyStatus.BadAssembly;
                    report.BadAssemblyException = Catalog.BadAssembliesDictionary[file.FullName];
                }
                else
                {
                    if (!Catalog.GoodAssemblies.ContainsKey(file.FullName))
                        throw new Exception("File " + file.Name + " is not in the Good or Bad Assembly lists of the SafeDirectoryCatalog, why?");

                    report.Assembly = Catalog.GoodAssemblies[file.FullName];

                    //if it has parts
                    if (Catalog.PartsByFileDictionary.ContainsKey(file.FullName))
                        foreach (ComposablePartDefinition part in Catalog.PartsByFileDictionary[file.FullName].Parts)
                            report.Parts.Add(new PluginPart(part));

                    //its a good assembly lets record that -although FigureOutDependencies might change that if it is resolvable Instructions in the MISL
                    report.Status = PluginAssemblyStatus.Healthy;

                    FigureOutDependencies(report);
                }
                progress++;
            }

            //done
            ProgressMade(this, new PluginAnalyserProgressEventArgs(progress, maxProgress, null));
        }

        private void FigureOutDependencies(PluginAnalyserReport report)
        {
            //lets look at all the types in the assembly
            foreach (TypeInfo type in report.Assembly.DefinedTypes)
            {
                //Primarily we are interested in the Exports
                PluginPart pluginPart = report.Parts.SingleOrDefault(p => p.ExportPart != null && p.ExportPart.ToString().Equals(type.FullName));

                if (pluginPart == null)
                    pluginPart = new PluginPart(type); //it's not an Export but it could still be dodgy we are also interested in the Type if its unloadable e.g. a utility class they have written that uses our API
                else
                    pluginPart.PartType = type;
                
                foreach (var method in type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (method.DeclaringType == typeof(System.Object))
                        continue;

                    if (method.DeclaringType == typeof(MarshalByRefObject))
                        continue;

                    var dependency = new PluginDependency(method.DeclaringType + "." + method.Name);
                    pluginPart.Dependencies.Add(dependency);
                    try
                    {
                        var instructions = method.GetInstructions();
                        foreach (Instruction instruction in instructions)
                        {
                            MethodInfo methodInfo = instruction.Operand as MethodInfo;

                            if (methodInfo != null)
                            {
                                var t = methodInfo.DeclaringType;
                                ParameterInfo[] parameters = methodInfo.GetParameters();


                                dependency.Instructions.Add(
                                    string.Format("{0}.{1}({2});",
                                        t != null ? t.FullName : "Unknown",
                                        methodInfo.Name,
                                        String.Join(", ",
                                            parameters.Select(p => p.ParameterType.FullName + " " + p.Name).ToArray())
                                        ));
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        if (e.Message.Equals("Method has no body"))
                            break;

                        dependency.Exception = e;
                        report.Status = PluginAssemblyStatus.IncompatibleAssembly;

                        //its a utility class (or some other random class) they have written that isn't an Export but it is unloadable anyway so lets track it
                        if (!report.Parts.Contains(pluginPart))
                            report.Parts.Add(pluginPart);
                    }
                }

                
            }
            
        }
    }
}
