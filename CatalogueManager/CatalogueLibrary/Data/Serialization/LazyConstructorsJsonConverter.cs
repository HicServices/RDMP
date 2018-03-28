using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories.Construction;
using CatalogueLibrary.Repositories.Construction.Exceptions;
using Newtonsoft.Json;

namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// Supports Json deserialization of objects which don't have default (blank) constructors.  Pass the objects you want to use for constructor
    /// arguments to classes you want to deserialize.  This JsonConverter will assert that it CanConvert any object for which it finds no default constructor and
    /// a single constructor which is compatible with the constructorObjects (or a subset of them)
    /// </summary>
    public class LazyConstructorsJsonConverter:JsonConverter
    {
        private readonly object[] _constructorObjects;
        private ObjectConstructor _objectConstructor;

        public LazyConstructorsJsonConverter(params object[] constructorObjects)
        {
            _constructorObjects = constructorObjects;
            _objectConstructor = new ObjectConstructor();
        }

        /// <summary>
        /// Cannot write, this class is for deserialization only
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var constructor = GetConstructors(objectType).Single();

            var instance = constructor.Key.Invoke(constructor.Value.ToArray());

            serializer.Populate(reader,instance);
            
            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            //we do not handle strings,ints etc!
            if (objectType.IsValueType)
                return false;

            //if there is one compatible constructor
            var constructors = GetConstructors(objectType);

            if (constructors.Count == 0)
                return false;

            if (constructors.Count == 1)
                return true;

            throw new ObjectLacksCompatibleConstructorException("There were " + constructors.Count + " compatible constructors for the constructorObjects provided");
        }

        private Dictionary<ConstructorInfo, List<object>> GetConstructors(Type objectType)
        {
            return _objectConstructor.GetConstructors(objectType, false, false, _constructorObjects);
        }
    }
}
