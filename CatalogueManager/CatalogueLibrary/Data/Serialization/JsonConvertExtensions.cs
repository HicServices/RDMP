using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    public static class JsonConvertExtensions
    {
        public static string SerializeObject(object value, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            var databaseEntityJsonConverter = new DatabaseEntityJsonConverter(repositoryLocator);
            
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new JsonConverter[] {databaseEntityJsonConverter}
            };
            
            return JsonConvert.SerializeObject(value, settings);
        }

        public static object DeserializeObject(string value, Type type,IRDMPPlatformRepositoryServiceLocator repositoryLocator, params object[] objectsForConstructingStuffWith)
        {
            var databaseEntityJsonConverter = new DatabaseEntityJsonConverter(repositoryLocator);
            var lazyJsonConverter = new PickAnyConstructorJsonConverter(new[] {repositoryLocator}.Union(objectsForConstructingStuffWith).ToArray());

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new JsonConverter[] {databaseEntityJsonConverter, lazyJsonConverter}
            };
            
            return JsonConvert.DeserializeObject(value, type, settings);
        }
    }
}
