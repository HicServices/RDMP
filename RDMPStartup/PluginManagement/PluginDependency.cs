using System;
using System.Collections.Generic;

namespace RDMPStartup.PluginManagement
{
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