using System;
using CatalogueLibrary.Data.Referencing;
using MapsDirectlyToDatabaseTable;

namespace RDMPStartup
{
    /// <summary>
    /// Plugin databases can have an IRepository for interacting with them (the easiest way to implement this is to inherit from TableRepository).  However
    /// in order to construct the IRepository you likely need a connection string which might be stored in the catalogue database (e.g. as an 
    /// ExternalDatabaseServer).  
    /// 
    /// <para>Plugin authors should inherit from PluginRepositoryFinder and return a suitable TableRepository for saving/loading objects into the database at runtime.</para>
    /// </summary>
    public interface IPluginRepositoryFinder
    {
        /// <summary>
        /// Returns an instance capable of loading and saving objects into the database.
        /// </summary>
        /// <returns></returns>
        PluginRepository GetRepositoryIfAny();

        /// <summary>
        /// Returns the Type of object returned by <see cref="GetRepositoryIfAny"/>.  This is used before constructing an actual instance to decide whether or not a given
        /// unknown object reference should be resolved by your <see cref="IRepository"/> or somebody elses (See <see cref="IReferenceOtherObject"/>).  
        /// </summary>
        /// <returns></returns>
        Type GetRepositoryType();
    }
}
