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
    public class PickAnyConstructorJsonConverter:JsonConverter
    {
        private readonly object[] _constructorObjects;
        private ObjectConstructor _objectConstructor;

        /// <summary>
        /// Creates a JSON deserializer that can use any constructors on the class which match <see cref="constructorObjects"/>
        /// </summary>
        /// <param name="constructorObjects"></param>
        public PickAnyConstructorJsonConverter(params object[] constructorObjects)
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

        /// <summary>
        /// Cannot write, throws NotImplementedException
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a hydrated object from <see cref="reader"/> by invoking the appropriate constructor identified by <see cref="ObjectConstructor.GetConstructors"/> 
        /// whitch matches the parameters provided to <see cref="PickAnyConstructorJsonConverter"/> when it was constructed.
        /// 
        /// <para>If the <see cref="objectType"/> is <see cref="IPickAnyConstructorFinishedCallback"/> then <see cref="IPickAnyConstructorFinishedCallback.AfterConstruction"/>
        /// will be called</para>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var constructor = GetConstructors(objectType).Single();

            var instance = constructor.Key.Invoke(constructor.Value.ToArray());

            serializer.Populate(reader,instance);

            var callback = instance as IPickAnyConstructorFinishedCallback;
            if(callback != null)
                callback.AfterConstruction();

            return instance;
        }

        /// <summary>
        /// Returns true if the <see cref="objectType"/> is a non value Type with one constructor compatible with the parameters provided to
        /// <see cref="PickAnyConstructorJsonConverter"/> when it was constructed.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
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
