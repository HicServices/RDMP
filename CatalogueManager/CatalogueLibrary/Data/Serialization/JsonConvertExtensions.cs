using System;
using System.Linq;
using CatalogueLibrary.Repositories;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// Facilitates the use of <see cref="DatabaseEntityJsonConverter"/> and <see cref="PickAnyConstructorJsonConverter"/> by configuring appropriate <see cref="JsonSerializerSettings"/> etc
    /// </summary>
    public static class JsonConvertExtensions
    {
        /// <summary>
        /// Serialize the given object resolving any properties which are <see cref="DatabaseEntity"/> into pointers using <see cref="DatabaseEntityJsonConverter"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="repositoryLocator"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deserializes a string created with <see cref="SerializeObject(object,IRDMPPlatformRepositoryServiceLocator)"/>.  This involves two additional areas of functionality
        /// beyond basic JSON:
        /// 
        /// <para>1. Any database pointer (e.g. Catalogue 123 0xab1d) will be fetched and returned from the appropriate platform database (referenced by <see cref="repositoryLocator"/>)</para>
        /// <para>2. Objects do not need a default constructor, instead <see cref="PickAnyConstructorJsonConverter"/> will be used with <see cref="objectsForConstructingStuffWith"/></para>
        /// <para>3. Any objects implementing <see cref="IPickAnyConstructorFinishedCallback"/> will have <see cref="IPickAnyConstructorFinishedCallback.AfterConstruction"/> called</para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="repositoryLocator"></param>
        /// <param name="objectsForConstructingStuffWith"></param>
        /// <returns></returns>
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
