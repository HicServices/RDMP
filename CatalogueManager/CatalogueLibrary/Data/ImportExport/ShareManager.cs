using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data.ImportExport
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

        private const string PersistenceSeparator = "|";

        public ShareManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
            _catalogueRepository = _repositoryLocator.CatalogueRepository;
        }

        /// <summary>
        /// Gets a serializated representation of the object, this is a reference to the object by ID / SharingUID (if it has one) not a list of all it's property values.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string GetPersistenceString(IMapsDirectlyToDatabaseTable o)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(o.GetType().Name);
            sb.Append(PersistenceSeparator);
            sb.Append(o.ID);
            sb.Append(PersistenceSeparator);
            sb.Append(o.Repository.GetType().Name);
            sb.Append(PersistenceSeparator);

            if (IsExportedObject(o))
                sb.Append(GetExportFor(o).SharingUID);

            return sb.ToString();
        }

        public IMapsDirectlyToDatabaseTable GetObjectFromPersistenceString(string persistenceString)
        {
            if (string.IsNullOrWhiteSpace(persistenceString))
                return null;

            var elements = persistenceString.Split(new []{PersistenceSeparator},StringSplitOptions.None);
            
            if(elements.Length < 4)
                throw new Exception("Malformed persistenceString:" + persistenceString);
            
            //elements[0];//type name of the class we are fetching
            //elements[1]; //ID of the class
            //elements[2]; // Repository Type name
            //elements[3]; // SharingUI if it has one

            //if it has a sharing UID
            if(!string.IsNullOrWhiteSpace(elements[3]))
            {
                var localImport = GetExistingImport(elements[3]);

                //which was imported as a local object
                if (localImport != null)
                    return localImport.GetLocalObject(_repositoryLocator); //get the local object
            }

            //otherwise get the existing master object
            var o = _repositoryLocator.GetArbitraryDatabaseObject(elements[2], elements[0], int.Parse(elements[1]));

            if(o == null)
                throw new Exception("Could not find object for persistenceString:" + persistenceString);
            
            return o;
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
            //empty guids are never imported
            if (Guid.Empty.ToString().Equals(sharingUID))
                return false;

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
        /// <summary>
        /// Returns a matching ObjectImport for the provided sharingUID or null if the UID has never been imported
        /// </summary>
        /// <param name="sharingUID"></param>
        /// <returns></returns>
        public ObjectImport GetExistingImport(string sharingUID)
        {
            return _catalogueRepository.GetAllObjects<ObjectImport>("WHERE SharingUID = '" + sharingUID + "'").SingleOrDefault();
        }

        public ObjectImport GetImportAs(string sharingUID, IMapsDirectlyToDatabaseTable o)
        {
            var existing = _catalogueRepository.GetAllObjects<ObjectImport>().SingleOrDefault(e => e.IsImportedObject(o));

            return existing ?? new ObjectImport(_catalogueRepository, sharingUID, o);
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
