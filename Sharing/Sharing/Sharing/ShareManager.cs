using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace Sharing.Sharing
{
    /// <summary>
    /// Handles querying/updating the ObjectExport and ObjectImport tables (See ObjectExport and ObjectImport classes).  These tables record which objects have
    /// been shared externally (with a SharingUID) or imported locally.  This table handles tasks such as identifying whether a given object is shared or not
    /// as well as handling the import process (in which a MapsDirectlyToDatabaseTableStatelessDefinition is translated into a local object and an ObjectImport
    /// record is created - to allow updating/synchronising later on).
    /// </summary>
    public class ShareManager
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly ICatalogueRepository _catalogueRepository;

        public ShareManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
            _catalogueRepository = _repositoryLocator.CatalogueRepository;
        }

        public bool IsExportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return _catalogueRepository.GetAllObjects<ObjectExport>("WHERE ObjectID = " + o.ID + " AND ObjectTypeName = '" + o.GetType().Name + "' AND RepositoryTypeName = '" + o.Repository.GetType().Name + "'").Any();
        }

        public bool IsImportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return _catalogueRepository.GetAllObjects<ObjectImport>("WHERE LocalObjectID = " + o.ID + " AND LocalTypeName = '" + o.GetType().Name + "' AND RepositoryTypeName = '" + o.Repository.GetType().Name + "'").Any();
        }
        public bool IsImported(string sharingUID)
        {
            return _catalogueRepository.GetAllObjects<ObjectImport>("WHERE SharingUID = '" + sharingUID + "'").Any();
        }

        public ObjectExport GetExportFor(IMapsDirectlyToDatabaseTable o)
        {
            var existing = _catalogueRepository.GetAllObjects<ObjectExport>().SingleOrDefault(e => e.IsExportedObject(o));

            return existing ?? new ObjectExport(_catalogueRepository, o);
        }

        public IMapsDirectlyToDatabaseTable GetExistingImport(string sharingUID, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            var import = GetExistingImport(sharingUID);

            if (import == null)
                return null;

            return import.GetLocalObject(repositoryLocator);
        }
        public ObjectImport GetExistingImport(string sharingUID)
        {
            return _catalogueRepository.GetAllObjects<ObjectImport>("WHERE SharingUID = '" + sharingUID + "'").SingleOrDefault();
        }

        public ObjectImport GetImportAs(string sharingUID, IMapsDirectlyToDatabaseTable o)
        {
            var existing = _catalogueRepository.GetAllObjects<ObjectImport>().SingleOrDefault(e => e.IsImportedObject(o));

            return existing ?? new ObjectImport(_catalogueRepository, sharingUID, o);
        }

        public IMapsDirectlyToDatabaseTable ImportObject(MapsDirectlyToDatabaseTableStatelessDefinition definition, string sharingUIDIfAny)
        {
            using (var con = _catalogueRepository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM " + definition.Type.Name, con.Connection);
                var cmdbuilder = new DiscoveredServer(_catalogueRepository.ConnectionStringBuilder).Helper.GetCommandBuilder(cmd);

                DbCommand cmdInsert = cmdbuilder.GetInsertCommand(true);
                cmdInsert.CommandText += ";SELECT @@IDENTITY;";

                ((TableRepository)_catalogueRepository).PrepareCommand(cmdInsert, definition.Properties);

                var id = Convert.ToInt32(cmdInsert.ExecuteScalar());

                var newObj =  _catalogueRepository.GetObjectByID(definition.Type, id);

                if (sharingUIDIfAny != null)
                    GetImportAs(sharingUIDIfAny, newObj);

                return newObj;

            }
        }

        /// <summary>
        /// Gets all import definitions (ObjectImport) defined in the Catalogue database
        /// </summary>
        /// <returns></returns>
        public ObjectImport[] GetAllImports()
        {
            return _catalogueRepository.GetAllObjects<ObjectImport>();
        }

        /// <summary>
        /// Deletes all import definitions (ObjectImport) for which the referenced object (IMapsDirectlyToDatabaseTable) no longer exists (has been deleted)
        /// </summary>
        public void DeleteAllOrphanImportDefinitions()
        {
            foreach (var import in GetAllImports())
                if (!import.LocalObjectStillExists(_repositoryLocator))
                    import.DeleteInDatabase();
        }
    }
}
