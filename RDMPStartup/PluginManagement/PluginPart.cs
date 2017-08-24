using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace RDMPStartup.PluginManagement
{
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