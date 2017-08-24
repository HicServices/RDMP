using System;
using CatalogueLibrary.ExternalDatabaseServerPatching;

namespace RDMPStartup.Events
{
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