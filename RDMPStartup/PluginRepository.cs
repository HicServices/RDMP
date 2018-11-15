using System;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace RDMPStartup
{
    /// <summary>
    /// Repository for constructing/saving/deleting <see cref="DatabaseEntity"/> objects that are are stored in your plugin database.
    /// The assembly containing your <see cref="PluginRepository"/> must be the same assembly that contains the class definitions.
    /// </summary>
    public abstract class PluginRepository:TableRepository
    {
        public ExternalDatabaseServer ExternalDatabaseServer { get; set; }

        /// <summary>
        /// Sets up the repository for reading and writing objects out of the given <paramref name="externalDatabaseServer"/>.  
        /// </summary>
        /// <param name="externalDatabaseServer">The database to connect to</param>
        /// <param name="dependencyFinder">Optional class that can forbid deleting objects because you have dependencies on them in your database (e.g. if your custom object has a field Catalogue_ID)</param>
        protected PluginRepository(ExternalDatabaseServer externalDatabaseServer, IObscureDependencyFinder dependencyFinder):base(dependencyFinder,externalDatabaseServer.Discover(DataAccessContext.InternalDataProcessing).Server.Builder)
        {
            ExternalDatabaseServer = externalDatabaseServer;
        }

        readonly ObjectConstructor _constructor = new ObjectConstructor();
        protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
        {
            return _constructor.ConstructIMapsDirectlyToDatabaseObject(t, this, reader);
        }

        protected override bool IsCompatibleType(Type type)
        {
            return typeof (DatabaseEntity).IsAssignableFrom(type);
        }
    }
}