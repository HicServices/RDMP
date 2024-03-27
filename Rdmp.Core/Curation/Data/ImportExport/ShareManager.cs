// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.ImportExport;

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

    private const char PersistenceSeparator = '|';

    /// <summary>
    /// Delegate method for populating environment specific properties e.g. <see cref="ICatalogue.LiveLoggingServer_ID"/> when importing
    /// <see cref="ShareDefinition"/> since this ID will be different from the origin.
    /// </summary>
    internal LocalReferenceGetterDelegate LocalReferenceGetter;

    /// <summary>
    /// Creates a new manager for importing and exporting objects from the given platform databases
    /// </summary>
    /// <param name="repositoryLocator"></param>
    /// <param name="localReferenceGetter"></param>
    public ShareManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        LocalReferenceGetterDelegate localReferenceGetter = null)
    {
        RepositoryLocator = repositoryLocator;
        _catalogueRepository = repositoryLocator.CatalogueRepository;
        LocalReferenceGetter = localReferenceGetter ?? DefaultLocalReferenceGetter;
    }

    private int? DefaultLocalReferenceGetter(PropertyInfo property, RelationshipAttribute relationshipattribute,
        ShareDefinition sharedefinition)
    {
        var defaults = RepositoryLocator.CatalogueRepository;


        if (property.Name is "LiveLoggingServer_ID" or "TestLoggingServer_ID")
        {
            var server = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            return server?.ID;
        }

        throw new SharingException(
            $"No default implementation exists for LocalReferenceGetterDelegate for property {property.Name}");
    }

    /// <summary>
    /// Gets a serialized representation of the object, this is a reference to the object by ID / SharingUID (if it has one) not a list of all its property values.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public string GetPersistenceString(IMapsDirectlyToDatabaseTable o)
    {
        var sb = new StringBuilder();
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

        var elements = persistenceString.Split(new[] { PersistenceSeparator }, StringSplitOptions.None);

        if (elements.Length < 4)
            throw new Exception($"Malformed persistenceString:{persistenceString}");

        //elements[0];//type name of the class we are fetching
        //elements[1]; //ID of the class
        //elements[2]; // Repository Type name
        //elements[3]; // SharingUI if it has one

        //if it has a sharing UID
        if (!string.IsNullOrWhiteSpace(elements[3]))
        {
            var localImport = GetExistingImport(elements[3]);

            //which was imported as a local object
            if (localImport != null)
                return localImport.GetReferencedObject(RepositoryLocator); //get the local object
        }

        //otherwise get the existing master object
        var o = RepositoryLocator.GetArbitraryDatabaseObject(elements[2], elements[0], int.Parse(elements[1])) ??
                throw new Exception($"Could not find object for persistenceString:{persistenceString}");
        return o;
    }

    /// <summary>
    /// Returns true if there is an <see cref="ObjectExport"/> declared which matches the provided object <paramref name="o"/>
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public bool IsExportedObject(IMapsDirectlyToDatabaseTable o) =>
        _catalogueRepository.GetReferencesTo<ObjectExport>(o).Any();


    /// <summary>
    /// Returns true if there is an <see cref="ObjectImport"/> declared which matches the provided object <paramref name="o"/>
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public bool IsImportedObject(IMapsDirectlyToDatabaseTable o) =>
        _catalogueRepository.GetReferencesTo<ObjectImport>(o).Any();

    /// <summary>
    /// Returns true if an <see cref="ObjectImport"/> has been declared for the given shared object identified by its <paramref name="sharingUID"/>
    /// </summary>
    /// <param name="sharingUID"></param>
    /// <returns></returns>
    public bool IsImported(string sharingUID) =>
        //empty guids are never imported
        !Guid.Empty.ToString().Equals(sharingUID) &&
        _catalogueRepository.GetAllObjectsWhere<ObjectImport>("SharingUID", sharingUID).Any();

    /// <summary>
    /// Returns an existing export definition for the object o or generates a new one.  This will give you a SharingUID and
    /// enable the object for sharing with other users who have RDMP.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public ObjectExport GetNewOrExistingExportFor(IMapsDirectlyToDatabaseTable o)
    {
        var existingExport =
            _catalogueRepository.GetAllObjects<ObjectExport>().SingleOrDefault(e => e.IsReferenceTo(o));

        if (existingExport != null)
            return existingExport;

        var existingImport =
            _catalogueRepository.GetAllObjects<ObjectImport>().SingleOrDefault(e => e.IsReferenceTo(o));

        return existingImport != null
            ? new ObjectExport(_catalogueRepository, o, existingImport.SharingUIDAsGuid)
            : new ObjectExport(_catalogueRepository, o, Guid.NewGuid());
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

        return import?.GetReferencedObject(RepositoryLocator);
    }

    /// <inheritdoc cref="GetExistingImportObject(string)"/>
    public object GetExistingImportObject(Guid sharingGuid) => GetExistingImportObject(sharingGuid.ToString());

    /// <summary>
    /// Returns the local object which was exported under the given <paramref name="sharingUID"/> (or null if the object has never
    /// been exported)
    /// </summary>
    /// <param name="sharingUID"></param>
    /// <returns></returns>
    public IMapsDirectlyToDatabaseTable GetExistingExportObject(string sharingUID)
    {
        var export = GetExistingExport(sharingUID);

        return export?.GetReferencedObject(RepositoryLocator);
    }

    /// <inheritdoc cref="GetExistingExportObject(string)"/>
    public object GetExistingExportObject(Guid sharingGuid) => GetExistingExportObject(sharingGuid.ToString());


    /// <summary>
    /// Returns a matching ObjectImport for the provided sharingUID or null if the UID has never been imported
    /// </summary>
    /// <param name="sharingUID"></param>
    /// <returns></returns>
    public ObjectImport GetExistingImport(string sharingUID) => _catalogueRepository
        .GetAllObjectsWhere<ObjectImport>("SharingUID", sharingUID).SingleOrDefault();

    /// <inheritdoc cref="GetExistingImport(string)"/>
    public ObjectImport GetExistingImport(Guid sharingUID) => GetExistingImport(sharingUID.ToString());

    /// <summary>
    /// Returns a matching ObjectExport for the provided sharingUID or null if the UID has never been imported
    /// </summary>
    /// <param name="sharingUID"></param>
    /// <returns></returns>
    public ObjectExport GetExistingExport(string sharingUID) => _catalogueRepository
        .GetAllObjectsWhere<ObjectExport>("SharingUID", sharingUID).SingleOrDefault();

    /// <inheritdoc cref="GetExistingExport(string)"/>
    public ObjectExport GetExistingExport(Guid sharingUID) => GetExistingExport(sharingUID.ToString());

    /// <summary>
    /// Marks the given local object <paramref name="o"/> as an imported instance of a shared object (identified by its <paramref name="sharingUID"/>)
    /// </summary>
    /// <param name="sharingUID"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    public ObjectImport GetImportAs(string sharingUID, IMapsDirectlyToDatabaseTable o) =>
        GetExistingImport(sharingUID) ?? new ObjectImport(_catalogueRepository, sharingUID, o);

    /// <summary>
    /// Gets all import definitions (ObjectImport) defined in the Catalogue database
    /// </summary>
    /// <returns></returns>
    public ObjectImport[] GetAllImports() => _catalogueRepository.GetAllObjects<ObjectImport>();

    /// <summary>
    /// Deletes all import definitions (ObjectImport) for which the referenced object (IMapsDirectlyToDatabaseTable) no longer exists (has been deleted)
    /// </summary>
    public void DeleteAllOrphanImportDefinitions()
    {
        foreach (var import in GetAllImports())
            if (!import.ReferencedObjectExists(RepositoryLocator))
                import.DeleteInDatabase();
    }

    /// <summary>
    /// Reads and deserializes the .so file into objects in the database
    /// </summary>
    /// <param name="sharedObjectsFile"></param>
    /// <param name="deleteExisting"></param>
    /// <returns></returns>
    public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(Stream sharedObjectsFile,
        bool deleteExisting = false)
    {
        var sr = new StreamReader(sharedObjectsFile);
        var text = sr.ReadToEnd();

        return ImportSharedObject(text);
    }

    /// <summary>
    /// Creates imported objects from a serialized list of <see cref="ShareDefinition"/> - usually loaded from a .so file (See Sharing.Dependency.Gathering.Gatherer)
    /// </summary>
    /// <param name="sharedObjectsFileText"></param>
    /// <param name="deleteExisting"></param>
    /// <returns></returns>
    public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(string sharedObjectsFileText,
        bool deleteExisting = false)
    {
        var toImport = GetShareDefinitionList(sharedObjectsFileText);

        return ImportSharedObject(toImport);
    }

    /// <summary>
    /// Deserializes the json which must be the contents of a .sd file i.e. a ShareDefinitionList
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public List<ShareDefinition> GetShareDefinitionList(string json) =>
        (List<ShareDefinition>)JsonConvertExtensions.DeserializeObject(json, typeof(List<ShareDefinition>),
            RepositoryLocator);

    /// <summary>
    /// Imports a list of shared objects and creates local copies of the objects as well as marking them as <see cref="ObjectImport"/>s
    /// </summary>
    /// <param name="toImport"></param>
    /// <returns></returns>
    public IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(List<ShareDefinition> toImport) =>
        ImportSharedObject(toImport, false);

    /// <summary>
    /// Imports a list of shared objects and creates local copies of the objects as well as marking them as <see cref="ObjectImport"/>s
    /// </summary>
    /// <param name="toImport"></param>
    /// <param name="deleteExisting">Deletes the object if the object has already been imported previously (not a good idea).</param>
    /// <returns></returns>
    internal IEnumerable<IMapsDirectlyToDatabaseTable> ImportSharedObject(List<ShareDefinition> toImport,
        bool deleteExisting)
    {
        var created = new List<IMapsDirectlyToDatabaseTable>();
        foreach (var sd in toImport)

            try
            {
                if (deleteExisting)
                {
                    var actual = (IMapsDirectlyToDatabaseTable)GetExistingImportObject(sd.SharingGuid);
                    actual?.DeleteInDatabase();
                }

                var instance = (IMapsDirectlyToDatabaseTable)ObjectConstructor.ConstructIfPossible(sd.Type, this, sd) ??
                               throw new ObjectLacksCompatibleConstructorException(
                                   $"Could not find a ShareManager constructor for '{sd.Type}'");
                if (instance.GetType() == typeof(LoadMetadataCatalogueLinkage))
                {
                    //find most recent lmd and all catalogues since, then link them
                    var latestLMD = created.OfType<LoadMetadata>().LastOrDefault();
                    if (latestLMD != null)
                    {
                        var index = created.IndexOf(latestLMD);
                        var cataloguesToLink = created.Skip(index).OfType<Catalogue>();
                        foreach (var catalogue in cataloguesToLink)
                        {
                            latestLMD.LinkToCatalogue(catalogue);

                        }
                        continue;
                    }
                }
                created.Add(instance);

            }
            catch (Exception e)
            {
                throw new Exception($"Error constructing {sd.Type}", e);
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
    public int? GetLocalReference(PropertyInfo property, RelationshipAttribute relationshipAttribute,
        ShareDefinition shareDefinition)
    {
        if (property.DeclaringType == null)
            throw new Exception($"DeclaringType on Property '{property}' is null");

        if (relationshipAttribute.Type != RelationshipType.LocalReference)
            throw new Exception(
                $"Relationship was of Type {relationshipAttribute.Type} expected {RelationshipType.LocalReference}");

        return LocalReferenceGetter == null
            ? throw new Exception(
                $"No LocalReferenceGetter has been set, cannot populate Property {property.Name}  on class {property.DeclaringType.Name}")
            : LocalReferenceGetter(property, relationshipAttribute, shareDefinition);
    }

    /// <summary>
    /// Updates the user configurable (non ID) properties of the object <pararef name="o"/> to match the <paramref name="shareDefinition"/>
    /// </summary>
    /// <param name="o"></param>
    /// <param name="shareDefinition"></param>
    public static void ImportPropertiesOnly(IMapsDirectlyToDatabaseTable o, ShareDefinition shareDefinition)
    {
        if (shareDefinition.Type != o.GetType())
            throw new Exception($"Share Definition is not for a {o.GetType()}");

        var relationshipPropertyFinder = new AttributePropertyFinder<RelationshipAttribute>(o);
        var skipPropertyFinder = new AttributePropertyFinder<DoNotImportDescriptionsAttribute>(o);


        //for each property that isn't [NoMappingToDatabase]
        foreach (var kvp in shareDefinition.GetDictionaryForImport())
        {
            if (kvp.Key == "Name")
                continue;

            var prop = o.GetType().GetProperty(kvp.Key);

            //If the property is a relationship e.g. _ID skip it
            if (relationshipPropertyFinder.GetAttribute(prop) != null)
                continue;

            //do we skip this property?
            var skip = skipPropertyFinder.GetAttribute(prop);
            if (skip != null)
            {
                //yes but only if blank
                if (skip.AllowOverwriteIfBlank)
                {
                    //is it currently not null? (if so skip)
                    var oldVal = prop.GetValue(o);

                    if (!(oldVal == DBNull.Value || oldVal == null || string.IsNullOrWhiteSpace(oldVal.ToString())))
                        continue;
                }
                else
                {
                    continue; //always skip
                }
            }

            SetValue(prop, kvp.Value, o);
        }
    }

    public void UpsertAndHydrate<T>(T toCreate, ShareDefinition shareDefinition)
        where T : class, IMapsDirectlyToDatabaseTable
    {
        IRepository repo;

        if (RepositoryLocator.CatalogueRepository.SupportsObjectType(typeof(T)))
            repo = RepositoryLocator.CatalogueRepository;
        else if (RepositoryLocator.DataExportRepository.SupportsObjectType(typeof(T)))
            repo = RepositoryLocator.DataExportRepository;
        else
            throw new NotSupportedException($"No Repository supported object type '{typeof(T)}'");

        //Make a dictionary of the normal properties we are supposed to be importing
        var propertiesDictionary = shareDefinition.GetDictionaryForImport();

        //for finding properties decorated with [Relationship]
        var finder = new AttributePropertyFinder<RelationshipAttribute>(toCreate);

        //If we have already got a local copy of this shared object?
        //either as an import or as an export
        var actual = (T)GetExistingImportObject(shareDefinition.SharingGuid) ??
                     (T)GetExistingExportObject(shareDefinition.SharingGuid);

        //we already have a copy imported of the shared object
        if (actual != null)
        {
            //It's an UPDATE i.e. take the new shared properties and apply them to the database copy / memory copy

            //copy all the values out of the share definition / database copy
            foreach (var prop in TableRepository.GetPropertyInfos(typeof(T)))
                //don't update any ID columns or any with relationships on UPDATE
                if (propertiesDictionary.TryGetValue(prop.Name, out var value) && finder.GetAttribute(prop) == null)
                {
                    SetValue(prop, value, toCreate);
                }
                else
                    prop.SetValue(toCreate,
                        prop.GetValue(actual)); //or use the database one if it isn't shared (e.g. ID, MyParent_ID etc)

            toCreate.Repository = actual.Repository;

            //commit the updated values to the database
            repo.SaveToDatabase(toCreate);
        }
        else
        {
            //It's an INSERT i.e. create a new database copy with the correct foreign key values and update the memory copy

            //for each relationship property on the class we are trying to hydrate
            foreach (var property in TableRepository.GetPropertyInfos(typeof(T)))
            {
                var relationshipAttribute = finder.GetAttribute(property);

                //if it has a relationship attribute then we would expect the ShareDefinition to include a dependency relationship with the sharing UID of the parent
                //and also that we had already imported it since dependencies must be imported in order
                if (relationshipAttribute != null)
                {
                    int? newValue;

                    switch (relationshipAttribute.Type)
                    {
                        case RelationshipType.OptionalSharedObject:
                        case RelationshipType.SharedObject:

                            //Confirm that the share definition includes the knowledge that there's a parent class to this object
                            if (!shareDefinition.RelationshipProperties.ContainsKey(relationshipAttribute))
                                //if it doesn't but the field is optional, ignore it
                                if (relationshipAttribute.Type == RelationshipType.OptionalSharedObject)
                                {
                                    newValue = null;
                                    break;
                                }
                                else
                                //otherwise we are missing a required shared object being referenced. That's bad news.
                                {
                                    throw new Exception(
                                        $"Share Definition for object of Type {typeof(T)} is missing an expected RelationshipProperty called {property.Name}");
                                }

                            //Get the SharingUID of the parent for this property
                            var importGuidOfParent = shareDefinition.RelationshipProperties[relationshipAttribute];

                            //Confirm that we have a local import of the parent
                            var parentImport = GetExistingImport(importGuidOfParent);

                            //if we don't have a share reference
                            if (parentImport == null)
                                //and it isn't optional
                                if (relationshipAttribute.Type == RelationshipType.SharedObject)
                                    throw new Exception(
                                        $"Cannot import an object of type {typeof(T)} because the ShareDefinition specifies a relationship to an object that has not yet been imported (A {relationshipAttribute.Cref} with a SharingUID of {importGuidOfParent}");
                                else
                                    newValue = null; //it was optional and missing so just set to null
                            else
                                newValue = parentImport.ReferencedObjectID; //we have the shared object
                            break;
                        case RelationshipType.LocalReference:
                            newValue = GetLocalReference(property, relationshipAttribute, shareDefinition);
                            break;
                        case RelationshipType.IgnoreableLocalReference:
                            newValue = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    //get the ID of the local import of the parent
                    propertiesDictionary[property.Name] = newValue;
                }
            }

            //insert the full dictionary into the database under the Type
            repo.InsertAndHydrate(toCreate, propertiesDictionary);

            //document that a local import of the share now exists and should be updated/reused from now on when that same GUID comes in / gets used by child objects
            GetImportAs(shareDefinition.SharingGuid.ToString(), toCreate);
        }
    }


    public static void SetValue(PropertyInfo prop, object value, IMapsDirectlyToDatabaseTable onObject)
    {
        //sometimes json decided to swap types on you e.g. int64 for int32
        var propertyType = prop.PropertyType;

        //if it is a nullable int etc
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            propertyType = propertyType.GetGenericArguments()[0]; //let's pretend it's just int / whatever

        if (value != null && value != DBNull.Value && !propertyType.IsInstanceOfType(value))
            if (propertyType == typeof(Uri))
                value = value is string s ? new Uri(s) : (Uri)value;
            else if (typeof(Enum).IsAssignableFrom(propertyType))
                value = Enum.ToObject(propertyType, value); //if the property is an enum
            else
                value = UsefulStuff.ChangeType(value, propertyType); //the property is not an enum

        prop.SetValue(onObject,
            value); //if it's a shared property (most properties) use the new shared value being imported
    }
}

/// <inheritdoc cref="ShareManager.LocalReferenceGetter"/>
public delegate int? LocalReferenceGetterDelegate(PropertyInfo property, RelationshipAttribute relationshipAttribute,
    ShareDefinition shareDefinition);