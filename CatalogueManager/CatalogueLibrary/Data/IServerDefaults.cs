using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Server defaults let you identify a role a server plays (e.g. IdentifierDumpServer) and make it the default one of it's type for all rows created which have an IdentifierDump.
    /// For example TableInfo.IdentifierDumpServer_ID defaults to whichever IdentifierDump ExternalDatabaseServer is configured (can be DBNull.Value).
    /// 
    /// <para>A scalar valued function GetDefaultExternalServerIDFor is used to retrieve defaults so that even if the user creates a new record in the TableInfo table himself manually without
    /// using our library (very dangerous anyway btw) it will still have the default.</para>
    /// </summary>
    public interface IServerDefaults
    {
        /// <summary>
        /// Pass in an enum to have it mapped to the scalar GetDefaultExternalServerIDFor function input that provides default values for columns that reference the given value - now note that this 
        /// might be a scalability issue at some point if there are multiple references from separate tables (or no references at all! like in DQE) 
        /// </summary>
        /// <param name="field"></param>
        /// <returns>the currently configured ExternalDatabaseServer the user wants to use as the default for the supplied role or null if no default has yet been picked</returns>
        IExternalDatabaseServer GetDefaultFor(ServerDefaults.PermissableDefaults field);

        /// <summary>
        /// The repository the defaults are configured on
        /// </summary>
        CatalogueRepository Repository { get; }
    }
}