using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    [Serializable]
    public class ShareDefinition
    {
        public Guid SharingGuid { get; set; }
        public string RepositoryTypeName { get; set; }

        [JsonIgnore]
        public int ID { get; set; }

        public Type Type { get; set; }


        public Dictionary<string, object> Properties { get; set; }
        public JsonCompatibleDictionary<RelationshipAttribute, Guid> RelationshipProperties = new JsonCompatibleDictionary<RelationshipAttribute, Guid>();
        
        public ShareDefinition(Guid sharingGuid, int id, string repositoryTypeName, Type type, Dictionary<string, object> properties, Dictionary<RelationshipAttribute, Guid> relationshipProperties)
        {
            if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                throw new ArgumentException("Type must be IMapsDirectlyToDatabaseTable", "type");
            
            SharingGuid = sharingGuid;
            ID = id;
            RepositoryTypeName = repositoryTypeName;
            Type = type;
            Properties = properties;
            
            RelationshipProperties = new JsonCompatibleDictionary<RelationshipAttribute, Guid>();
            
            foreach (var kvp in relationshipProperties)
                RelationshipProperties.Add(kvp.Key, kvp.Value);
        }
    }
}