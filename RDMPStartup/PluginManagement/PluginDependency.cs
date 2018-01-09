using System;
using System.Collections.Generic;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// A method declared on a MEF exportable class in a dll that is part of a Plugin being analysed in a PluginAnalyser.  Contains a list of all
    /// the Instructions that could be identified using Mono.Reflection.Instruction.  If a method had code that was loadable but could not be resolved 
    /// at runtime it means there has been a breaking API change since the plugin was last compiled (e.g. the RDMP has been updated).  You can see this
    /// if the Exception property has been populated.
    /// </summary>
    public class PluginDependency
    {
        public PluginDependency(string name)
        {
            Name = name;
            Instructions = new List<string>();
        }

        public string Name { get; set; }
        public List<string> Instructions { get; private set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return Name;
        }

        
    }
}