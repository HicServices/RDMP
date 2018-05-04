using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// Describes a DatabaseEntity which has been made exportable from RDMP via <see cref="ObjectExport"/>.  This class includes the properties that are directly recorded for the object
    /// e.g. Name, SelectSQL etc.  For Foreign Key columns (See <see cref="RelationshipAttribute"/>) e.g. <see cref="CatalogueItem.Catalogue_ID"/> the Guid of another 
    /// <see cref="ShareDefinition"/> is given (e.g. of the <see cref="Catalogue"/>).  This means that a <see cref="ShareDefinition"/> is only valid when all it's dependencies are
    /// also available (See <see cref="Sharing.Dependency.Gathering.Gatherer"/> for how to do this)
    /// </summary>
    [Serializable]
    public class ShareDefinition
    {
        public Guid SharingGuid { get; set; }

        [JsonIgnore]
        public int ID { get; set; }

        public Type Type { get; set; }


        public Dictionary<string, object> Properties { get; set; }
        public JsonCompatibleDictionary<RelationshipAttribute, Guid> RelationshipProperties = new JsonCompatibleDictionary<RelationshipAttribute, Guid>();
        
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
    }
}