using System;
using CatalogueLibrary.ExternalDatabaseServerPatching;

namespace RDMPStartup.Events
{
    /// <summary>
    /// EventArgs for finding Plugin IPatchers during Startup.cs
    /// 
    /// <para>IPatchers identify databases that are managed by a .Database assembly and as such need to be patched/updated when the host assembly is updated.  For 
    /// plugins this is done by declaring a IPluginPatcher and listing the host/database assemblies but there can be Type loading errors or other Exceptions 
    /// around locating databases that must be patched, this event system supports reporting those.</para>
    /// </summary>
    public class PluginPatcherFoundEventArgs
    {
        public PluginPatcherFoundEventArgs(Type type, IPatcher instance, PluginPatcherStatus status, Exception exception=null)
        {
            Type = type;
            Instance = instance;
            Status = status;
            Exception = exception;
        }

        public Type Type { get; set; }
        public IPatcher Instance { get; set; } 
        public PluginPatcherStatus Status {get;set;}
        public Exception Exception { get; set; }
    }
}
