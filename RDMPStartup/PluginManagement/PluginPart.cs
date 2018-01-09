using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Describes a Type declared in a dll (LoadModuleAssembly) that is part of a Plugin downloaded into the MEF directory and being analysed by a PluginAnalyser.
    /// Includes the MEF ComposablePartDefinition, Type and a list of all the decompiled methods on the class (PluginDependency). 
    /// </summary>
    public class PluginPart
    {
        public List<PluginDependency> Dependencies { get; private set; }
        
        public ComposablePartDefinition ExportPart { get; set; }
        public Type PartType;

        private PluginPart()
        {
            Dependencies = new List<PluginDependency>();
        }
        public PluginPart(ComposablePartDefinition part):this()
        {
            ExportPart = part;
        }

        public PluginPart(Type type): this()
        {
            PartType = type;
        }

        public override string ToString()
        {
            if (PartType != null)
                return PartType.FullName;
            
            return ExportPart.ToString();
        }
    }
}