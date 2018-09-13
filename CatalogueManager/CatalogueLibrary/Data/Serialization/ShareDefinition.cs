using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// Describes a DatabaseEntity which has been made exportable from RDMP via <see cref="CatalogueLibrary.Data.ImportExport.ObjectExport"/>.  This class includes the properties that are
    /// directly recorded for the object e.g. Name, SelectSQL etc.  For Foreign Key columns (See <see cref="RelationshipAttribute"/>) e.g. <see cref="CatalogueItem.Catalogue_ID"/> the Guid 
    /// of another <see cref="ShareDefinition"/> is given (e.g. of the <see cref="Catalogue"/>).  This means that a <see cref="ShareDefinition"/> is only valid when all it's dependencies are
    /// also available (See Sharing.Dependency.Gathering.Gatherer for how to do this)
    /// </summary>
    [Serializable]
    public class ShareDefinition
    {
        /// <summary>
        /// The unique number that identifies this shared object.  This is created when the object is first shared as an <see cref="CatalogueLibrary.Data.ImportExport.ObjectExport"/> and 
        /// persisted by all other systems that import the object as an <see cref="CatalogueLibrary.Data.ImportExport.ObjectImport"/>.
        /// </summary>
        public Guid SharingGuid { get; set; }

        /// <summary>
        /// The <see cref="DatabaseEntity.ID"/> of the object in the original database the share was created from (this will be different to the ID it has when imported elsewhere)
        /// </summary>
        [JsonIgnore]
        public int ID { get; set; }

        /// <summary>
        /// The System.Type and therefore database table of the <see cref="DatabaseEntity"/> that is being shared
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The values of all non public properties on the <see cref="DatabaseEntity"/>.  This does not include any foreign key ID properties e.g. <see cref="CatalogueItem.Catalogue_ID"/>
        /// which will instead be stored in <see cref="RelationshipProperties"/>
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// The values of all foreign key properties on the <see cref="DatabaseEntity"/> (e.g. <see cref="CatalogueItem.Catalogue_ID"/>).  This is the SharingGuid of the referenced object.
        /// An object cannot be shared unless it is also shared with all such dependencies.
        /// </summary>
        public JsonCompatibleDictionary<RelationshipAttribute, Guid> RelationshipProperties = new JsonCompatibleDictionary<RelationshipAttribute, Guid>();

        /// <inheritdoc cref="ShareDefinition"/>
        public ShareDefinition(Guid sharingGuid, int id, Type type, Dictionary<string, object> properties, Dictionary<RelationshipAttribute, Guid> relationshipProperties)
        {
            if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                throw new ArgumentException("Type must be IMapsDirectlyToDatabaseTable", "type");
            
            SharingGuid = sharingGuid;
            ID = id;
            Type = type;
            Properties = properties;
            
            RelationshipProperties = new JsonCompatibleDictionary<RelationshipAttribute, Guid>();
            
            foreach (var kvp in relationshipProperties)
                RelationshipProperties.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Removes null entries and fixes problematic value types e.g. <see cref="CatalogueFolder"/> which is better imported as a string
        /// </summary>
        public Dictionary<string, object> GetDictionaryForImport()
        {
            //Make a dictionary of the normal properties we are supposed to be importing
            Dictionary<string, object> newDictionary = Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            //remove null arguments they won't help us here
            foreach (string key in newDictionary.Keys.ToArray())
            {
                if (newDictionary[key] is CatalogueFolder)
                    newDictionary[key] = newDictionary[key].ToString();

                if (newDictionary[key] == null)
                    newDictionary.Remove(key);
            }

            return newDictionary;
        }
    }
}