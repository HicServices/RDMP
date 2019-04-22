// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Evaluates a single Plugin that is committed in the current Catalogue database (and downloaded into the MEF folder - PluginDirectory).  All the files
    /// (.dlls) which make up the plugin (LoadModuleAssembly) are evaluated and a PluginAnalyserReport is produced for each.
    /// 
    /// <para>Used for debug purposes to identify unloadable dlls in a plugin or breaking API changes in different versions of loaded dlls.  Decompiles all methods
    /// in all MEF exportable Types using Mono.Reflection to ensure that even though the Type can be loaded that the methods themselves can also be linked up
    /// correctly (enumerating Instructions identifies any breaking changes in the API even when they don't result in unloadable Type errors).</para>
    /// 
    /// <para>See PluginManagementForm for a friendly UI for this class.  </para>
    /// </summary>
    public class PluginAnalyser
    {
        public CatalogueLibrary.Data.Plugin Plugin { get; set; }
        public DirectoryInfo PluginDirectory { get; set; }
        public SafeDirectoryCatalog Catalog { get; set; }

        public Dictionary<LoadModuleAssembly,PluginAnalyserReport> Reports = new Dictionary<LoadModuleAssembly, PluginAnalyserReport>();
        public event PluginAnalyserProgressEventHandler ProgressMade = delegate { };

        public PluginAnalyser(CatalogueLibrary.Data.Plugin plugin,DirectoryInfo pluginDirectory,SafeDirectoryCatalog catalog)
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
                else if (!Catalog.GoodAssemblies.ContainsKey(file.FullName))
                {
                    report.Status = PluginAssemblyStatus.FileMissing;
                    report.BadAssemblyException =
                        new Exception("File " + file.Name +
                                      " is not in the Good or Bad Assembly lists of the SafeDirectoryCatalog.");
                    ProgressMade(this, new PluginAnalyserProgressEventArgs(progress, maxProgress, null));
                }
                else
                {
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
                        var instructions = method.GetMethodBody();

                        instructions?.GetILAsByteArray();

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
