using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDMPStartup.PluginManagement
{
    /// <summary>
    /// Describes all the MEF exports in the current dll (LoadModuleAssembly) which is part of a Plugin.  Each MEF exposed Type has a PluginPart created for it
    /// which documents the methods / API compatibility issues (See PluginPart).
    /// 
    /// Overall the dll is given a PluginAssemblyStatus indicating whether it was loadable and whether the Types declared resolved correctly and were loaded into
    /// the AppDomain.
    /// 
    /// If a given dll is unloadable it won't have any PluginParts but should have a BadAssemblyException
    /// 
    /// All dlls are evaluated even when they are bundled with the plugin as dependencies of other dlls and do not contain any MEF classes themselves.  For example
    /// if you have a plugin for managing DICOM images you might have a third party dll for interacting with DICOM files as part of the Plugin.  This would also 
    /// have a PluginAnalyserReport which should indicate Health status and no PluginParts (assuming it has not MEF exports).
    /// </summary>
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