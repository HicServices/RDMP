using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDMPStartup.PluginManagement
{
    public class PluginAnalyserReport
    {
        public PluginAssemblyStatus Status { get; set; }
        public Assembly Assembly { get; set; }
        public Exception BadAssemblyException { get; set; }

        public List<PluginPart> Parts = new List<PluginPart>();
        
        public PluginAnalyserReport()
        {
            Parts = new List<PluginPart>();
        }
    }
}