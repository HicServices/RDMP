using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CatalogueLibrary.Data.ImportExport.Exceptions;
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
        /// <summary>
        /// Tells the location of the platform databases to create objects/import references in
        /// </summary>
        public readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        private readonly ICatalogueRepository _catalogueRepository;

        private const string PersistenceSeparator = "|";

        /// <summary>
        /// Delegate method for populating environment specific properties e.g. <see cref="ICatalogue.LiveLoggingServer_ID"/> when importing 
        /// <see cref="ShareDefinition"/> since this ID will be different from the origin.
        /// </summary>
        public LocalReferenceGetterDelegate LocalReferenceGetter;

        /// <summary>
        /// Creates a new manager for importing and exporting objects from the given platform databases
        /// </summary>
        /// <param name="repositoryLocator"></param>
        /// <param name="localReferenceGetter"></param>
        public ShareManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator, LocalReferenceGetterDelegate localReferenceGetter = null)
        {
            RepositoryLocator = repositoryLocator;
            _catalogueRepository = RepositoryLocator.CatalogueRepository;
            LocalReferenceGetter = localReferenceGetter ?? DefaultLocalReferenceGetter;
        }

        private int? DefaultLocalReferenceGetter(PropertyInfo property, RelationshipAttribute relationshipattribute, ShareDefinition sharedefinition)
        {
            var defaults = new ServerDefaults(RepositoryLocator.CatalogueRepository);


            if(property.Name == "LiveLoggingServer_ID" || property.Name == "TestLoggingServer_ID")
            {
                var server = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
                if (server == null)
                    return null;

                return server.ID;
            }

            throw new SharingException("No default implementation exists for LocalReferenceGetterDelegate for property " + property.Name);

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

        /// <summary>
        /// Deserializes the given persistence string (created by <see cref="GetPersistenceString"/>) into an actual database object.  The 
        /// <paramref name="persistenceString"/> is a pointer (ID / SharingUI) of the object not a value serialization.  If you want to export the
        /// definition use <see cref="ShareDefinition"/> or Gatherer instead
        /// </summary>
        /// <param name="persistenceString"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns true if there is an <see cref="ObjectExport"/> declared which matches the provided object <paramref name="o"/>
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool IsExportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return _catalogueRepository.GetAllObjects<ObjectExport>("WHERE ObjectID = " + o.ID + " AND ObjectTypeName = '" + o.GetType().Name + "' AND RepositoryTypeName = '" + o.Repository.GetType().Name + "'").Any();
        }


        /// <summary>
        /// Returns true if there is an <see cref="ObjectImport"/> declared which matches the provided object <paramref name="o"/>
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool IsImportedObject(IMapsDirectlyToDatabaseTable o)
        {
            return _catalogueRepository.GetAllObjects<ObjectImport>("WHERE LocalObjectID = " + o.ID + " AND LocalTypeName = '" + o.GetType().Name + "' AND RepositoryTypeName = '" + o.Repository.GetType().Name + "'").Any();
        }

        /// <summary>
        /// Returns true if an <see cref="ObjectImport"/> has been declared for the given shared object identified by it's <paramref name="sharingUID"/>
        /// </summary>
        /// <param name="sharingUID"></param>
        /// <returns></returns>
        public bool IsImported(string sharingUID)
        {
            //empty guids are never imported
            if (Guid.Empty.ToString().Equals(sharingUID))
                return false;

            return _catalogueRepository.GetAllObjects<ObjectImport>("WHERE SharingUID = '" + sharingUID + "'").Any();
        }

        /// <summary>
        /// Returns an existing export definition for the object o or generates a new one.  This will give you a SharingUID and 
        /// enable the object for sharing with other users who have RDMP.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Returns the local object which was imported under the given <paramref name="sharingUID"/> (or null if the object has never
        /// been imported)
        /// </summary>
        /// <param name="sharingUID"></param>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable GetExistingImportObject(string sharingUID)
        {
            var import = GetExistingImport(sharingUID);

            if (import == null)
                return null;

            return import.GetLocalObject(RepositoryLocator);
        }

        /// <inheritdoc cref="GetExistingImportObject(string)"/>
        public object GetExistingImportObject(Guid sharingGuid)
        {
            return GetExistingImportObject(sharingGuid.ToString());
        }

        /// <summary>
        /// Returns the local object which was exported under the given <paramref name="sharingUID"/> (or null if the object has never
        /// been exported)
        /// </summary>
        /// <param name="sharingUID"></param>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable GetExistingExportObject(string sharingUID)
        {
            var export = GetExistingExport(sharingUID);

            if (export == null)
                return null;

            return export.GetLocalObject(RepositoryLocator);
        }

        /// <inheritdoc cref="GetExistingExportObject(string)"/>
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

        /// <summary>
        /// Marks the given local object <paramref name="o"/> as an imported instance of a shared object (identified by it's <paramref name="sharingUID"/>)
        /// </summary>
        /// <param name="sharingUID"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public ObjectImport GetImportAs(string sharingUID, IMapsDirectlyToDatabaseTable o)
        {
            return GetExistingImport(sharingUID) ?? new ObjectImport(_catalogueRepository, sharingUID, o);
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
        /// Creates imported objects from a serialized list of <see cref="ShareDefinition"/> - usually loaded from a .so file (See Sharing.Dependency.Gathering.Gatherer)
        /// </summary>
        /// <param name="sharedObjectsFileText"></param>
        /// <returns></returns>
        public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(string sharedObjectsFileText, bool deleteExisting = false)
        {
            var toImport = GetShareDefinitionList(sharedObjectsFileText);

            return ImportSharedObject(toImport);
        }

        /// <summary>
        /// Deserializes the json which must be the contents of a .sd file i.e. a ShareDefinitionList
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public List<ShareDefinition> GetShareDefinitionList(string json)
        {
            return (List<ShareDefinition>)JsonConvertExtensions.DeserializeObject(json, typeof (List<ShareDefinition>), RepositoryLocator);
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
        /// <param name="deleteExisting">Deletes the object if the object has already been imported previously (not a good idea).</param>
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

        /// <summary>
        /// When importing a <paramref name="shareDefinition"/> for a child class with a parent, this method will return the ID of parent for the given <paramref name="property"/>
        /// on the child.  For example if you are importing a <see cref="ShareDefinition"/> for a <see cref="CatalogueItem"/> then the property <see cref="CatalogueItem.Catalogue_ID"/> should 
        /// have the ID of the locally held <see cref="Catalogue"/> to which it will become a part of.
        /// </summary>
        /// <param name="property">The child class property you need to fill e.g. <see cref="CatalogueItem.Catalogue_ID"/></param>
        /// <param name="relationshipAttribute">The attribute that decorates the <paramref name="property"/> which indicates what type of object the parent is etc</param>
        /// <param name="shareDefinition">The serialization of the child you are trying to import</param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the user configurable (non ID) properties of the object <pararef name="o"/> to match the <paramref name="shareDefinition"/>
        /// </summary>
        /// <param name="o"></param>
        /// <param name="shareDefinition"></param>
        public void ImportPropertiesOnly(IMapsDirectlyToDatabaseTable o, ShareDefinition shareDefinition)
        {
            if (shareDefinition.Type != o.GetType())
                throw new Exception("Share Definition is not for a " + o.GetType());

            //for each property that isn't [NoMappingToDatabase]
            foreach (var kvp in shareDefinition.GetDictionaryForImport())
            {
                var prop = o.GetType().GetProperty(kvp.Key);
                RepositoryLocator.CatalogueRepository.SetValue(prop,kvp.Value,o);   
            }
        }
    }

    /// <inheritdoc cref="ShareManager.LocalReferenceGetter"/>
    public delegate int? LocalReferenceGetterDelegate(PropertyInfo property, RelationshipAttribute relationshipAttribute, ShareDefinition shareDefinition);
}
