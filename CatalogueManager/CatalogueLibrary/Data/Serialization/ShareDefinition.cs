using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
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