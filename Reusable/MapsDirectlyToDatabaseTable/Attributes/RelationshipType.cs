namespace MapsDirectlyToDatabaseTable.Attributes
{
    public enum RelationshipType
    {
        /// <summary>
        /// The decorated property reflects a reference to another shared object which must be supplied as part of the gathered objects in a ShareDefinition.
        /// For example when sharing a CatalogueItem you must already have shared and imported the Catalogue it came from.
        /// </summary>
        SharedObject,

        /// <summary>
        /// The decorated property reflects a system boundary between shared objects and local objects.  The decorated property should not
        /// be a reference to a shared object.  Instead it should be maped to a local object on import.  For example when sharing a CatalogueItem,
        /// the associated ColumnInfo is not something that should be transmitted but it must exist and be selected before CatalogueItem can be imported.
        /// </summary>
        LocalReference,

        /// <summary>
        /// The decorated property reflects a system boundary between shared objects and local objects.  The decorated property should not
        /// be a reference to a shared object.  Instead it should be skipped entirely.  For example when sharing a Catalogue, the associated 
        /// LoadMetadata is irrelevant and should not be shared (it should be left as null in the imported destination).
        /// </summary>
        IgnoreableLocalReference
    }
}