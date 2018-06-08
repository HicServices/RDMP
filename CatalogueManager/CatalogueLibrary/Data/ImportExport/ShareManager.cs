using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;

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
        public readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        private readonly ICatalogueRepository _catalogueRepository;

        private const string PersistenceSeparator = "|";

        public LocalReferenceGetterDelegate LocalReferenceGetter;

        public ShareManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
            _catalogueRepository = RepositoryLocator.CatalogueRepository;
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
                sb.Append(GetNewOrExistingExportFor(o).SharingUID);

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
                    return localImport.GetLocalObject(RepositoryLocator); //get the local object
            }

            //otherwise get the existing master object
            var o = RepositoryLocator.GetArbitraryDatabaseObject(elements[2], elements[0], int.Parse(elements[1]));

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

        public ObjectExport GetNewOrExistingExportFor(IMapsDirectlyToDatabaseTable o)
        {
            var existingExport = _catalogueRepository.GetAllObjects<ObjectExport>().SingleOrDefault(e => e.IsExportedObject(o));

            if (existingExport != null)
                return existingExport;

            var existingImport = _catalogueRepository.GetAllObjects<ObjectImport>().SingleOrDefault(e => e.IsImportedObject(o));
            
            if (existingImport != null)
                return new ObjectExport(_catalogueRepository, o, existingImport.SharingUIDAsGuid);
            
            return new ObjectExport(_catalogueRepository, o,Guid.NewGuid());
        }

        public IMapsDirectlyToDatabaseTable GetExistingImportObject(string sharingUID)
        {
            var import = GetExistingImport(sharingUID);

            if (import == null)
                return null;

            return import.GetLocalObject(RepositoryLocator);
        }
        public object GetExistingImportObject(Guid sharingGuid)
        {
            return GetExistingImportObject(sharingGuid.ToString());
        }

        public IMapsDirectlyToDatabaseTable GetExistingExportObject(string sharingUID)
        {
            var export = GetExistingExport(sharingUID);

            if (export == null)
                return null;

            return export.GetLocalObject(RepositoryLocator);
        }
        public object GetExistingExportObject(Guid sharingGuid)
        {
            return GetExistingExportObject(sharingGuid.ToString());
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

        /// <inheritdoc cref="GetExistingImport(string)"/>
        public ObjectImport GetExistingImport(Guid sharingUID)
        {
            return GetExistingImport(sharingUID.ToString());
        }

        /// <summary>
        /// Returns a matching ObjectExport for the provided sharingUID or null if the UID has never been imported
        /// </summary>
        /// <param name="sharingUID"></param>
        /// <returns></returns>
        public ObjectExport GetExistingExport(string sharingUID)
        {
            return _catalogueRepository.GetAllObjects<ObjectExport>("WHERE SharingUID = '" + sharingUID + "'").SingleOrDefault();
        }

        /// <inheritdoc cref="GetExistingExport(string)"/>
        public ObjectExport GetExistingExport(Guid sharingUID)
        {
            return GetExistingExport(sharingUID.ToString());
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
                if (!import.LocalObjectStillExists(RepositoryLocator))
                    import.DeleteInDatabase();
        }

        /// <summary>
        /// Reads and deserializes the .so file into objects in the database
        /// </summary>
        /// <param name="sharedObjectsFile"></param>
        /// <returns></returns>
        public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(Stream sharedObjectsFile, bool deleteExisting = false)
        {
            var sr = new StreamReader(sharedObjectsFile);
            var text = sr.ReadToEnd();

            return ImportSharedObject(text);
        }

        /// <summary>
        /// Creates imported objects from a serialized list of <see cref="ShareDefinition"/> - usually loaded from a .so file (See <see cref="Gatherer"/>)
        /// </summary>
        /// <param name="sharedObjectsFileText"></param>
        /// <returns></returns>
        public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(string sharedObjectsFileText, bool deleteExisting = false)
        {
            var toImport = (List<ShareDefinition>)JsonConvertExtensions.DeserializeObject(sharedObjectsFileText, typeof(List<ShareDefinition>), RepositoryLocator);

            return ImportSharedObject(toImport);
        }

        /// <summary>
        /// Imports a list of shared objects and creates local copies of the objects as well as marking them as <see cref="ObjectImport"/>s
        /// </summary>
        /// <param name="toImport"></param>
        /// <returns></returns>
        public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(List<ShareDefinition> toImport)
        {
            return ImportSharedObject(toImport, false);
        }

        /// <summary>
        /// Imports a list of shared objects and creates local copies of the objects as well as marking them as <see cref="ObjectImport"/>s
        /// </summary>
        /// <param name="toImport"></param>
        /// <param name="deleteExisting"></param>
        /// <returns></returns>
        internal IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(List<ShareDefinition> toImport, bool deleteExisting)
        {
            List<IMapsDirectlyToDatabaseTable> created = new List<IMapsDirectlyToDatabaseTable>();

            foreach (ShareDefinition sd in toImport)
            {
                try
                {
                    if (deleteExisting)
                    {
                        var actual = (IMapsDirectlyToDatabaseTable)GetExistingImportObject(sd.SharingGuid);
                        if (actual != null)
                            actual.DeleteInDatabase();
                    }
                    var objectConstructor = new ObjectConstructor();
                    created.Add((IMapsDirectlyToDatabaseTable)objectConstructor.ConstructIfPossible(sd.Type, this, sd));
                }
                catch (Exception e)
                {
                    throw new Exception("Error constructing " + sd.Type, e);
                }
            }

            return created;
        }

        public int? GetLocalReference(PropertyInfo property, RelationshipAttribute relationshipAttribute, ShareDefinition shareDefinition)
        {
            if(property.DeclaringType == null)
                throw new Exception("DeclaringType on Property '" + property + "' is null");

            if (relationshipAttribute.Type != RelationshipType.LocalReference)
                throw new Exception("Relationship was of Type " + relationshipAttribute.Type + " expected " + RelationshipType.LocalReference);

            if(LocalReferenceGetter == null)
                throw new Exception(
                    string.Format("No LocalReferenceGetter has been set, cannot populate Property {0} {1}",
                     property.Name,
                     " on class " + property.DeclaringType.Name));

            return LocalReferenceGetter(property, relationshipAttribute, shareDefinition);
        }
    }

    public delegate int? LocalReferenceGetterDelegate(PropertyInfo property, RelationshipAttribute relationshipAttribute, ShareDefinition shareDefinition);
}
