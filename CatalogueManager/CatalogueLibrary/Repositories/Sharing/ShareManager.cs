using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories.Sharing;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Repositories
{
    public class ShareManager
    {
        private readonly ICatalogueRepository _catalogueRepository;

        public ShareManager(ICatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
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
            var import = _catalogueRepository.GetAllObjects<ObjectImport>("WHERE SharingUID = '" + sharingUID + "'").SingleOrDefault();

            if (import == null)
                return null;

            return import.GetLocalObject(repositoryLocator);
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
    }
}
