using System;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// Handles Serialization of Database Entity classes.  Writing is done by by storing the ID, Type and RepositoryType where the object is stored.  Reading is done by
    /// using the IRDMPPlatformRepositoryServiceLocator to fetch the instance out of the database.  
    /// 
    /// Also stores the ExportSharingUID if available which will allow deserializing shared objects that might only exist in a local import form i.e. with a different ID
    /// (<see cref="ShareManager"/>)
    /// </summary>
    public class DatabaseEntityJsonConverter:JsonConverter
    {
        private readonly ShareManager _shareManager;

        public DatabaseEntityJsonConverter(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _shareManager = new ShareManager(repositoryLocator);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("PersistenceString");
            writer.WriteRawValue('"' + _shareManager.GetPersistenceString((DatabaseEntity)value) + '"');
            writer.WriteEndObject();
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            
            if (reader.TokenType != JsonToken.StartObject) 
                throw new JsonReaderException("Malformed json");
            
            //instance to populate
            reader.Read();

            if(!reader.Value.Equals("PersistenceString"))
                throw new JsonReaderException("Malformed json, expected single property PersistenceString");

            //read the value
            reader.Read();
            var o = _shareManager.GetObjectFromPersistenceString((string) reader.Value);

            reader.Read();

            //read the end object
            if(reader.TokenType != JsonToken.EndObject)
                throw new JsonReaderException("Did not find EndObject");


            return o;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DatabaseEntity).IsAssignableFrom(objectType);
        }
    }
}
