using System;
using MapsDirectlyToDatabaseTable;

namespace RDMPStartup
{
    /// <summary>
    /// Plugin databases can have an IRepository for interacting with them (the easiest way to implement this is to inherit from TableRepository).  However
    /// in order to construct the IRepository you likely need a connection string which might be stored in the catalogue database (e.g. as an 
    /// ExternalDatabaseServer).  
    /// 
    /// Plugin authors should inherit from PluginRepositoryFinder and return a suitable TableRepository for saving/loading objects into the database at runtime.
    /// </summary>
    public interface IPluginRepositoryFinder
    {
        IRepository GetRepositoryIfAny();
        Type GetRepositoryType();
    }
}