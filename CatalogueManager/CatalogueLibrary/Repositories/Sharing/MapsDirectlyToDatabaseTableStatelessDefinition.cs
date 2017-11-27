using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories.Sharing
{
    [Serializable]
    public class MapsDirectlyToDatabaseTableStatelessDefinition
    {
        public Type Type { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        protected MapsDirectlyToDatabaseTableStatelessDefinition(Type type) : this(type, new Dictionary<string, object>())
        { }

        public MapsDirectlyToDatabaseTableStatelessDefinition(Type type, Dictionary<string, object> properties)
        {
            if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                throw new ArgumentException("Type must be IMapsDirectlyToDatabaseTable", "type");

            Type = type;
            Properties = properties;
        }
    }

    [Serializable]
    public class MapsDirectlyToDatabaseTableStatelessDefinition<T> : MapsDirectlyToDatabaseTableStatelessDefinition where T : IMapsDirectlyToDatabaseTable
    {
        public MapsDirectlyToDatabaseTableStatelessDefinition(T mappedObject) : base(typeof(T))
        {
            Properties = TableRepository.GetPropertyInfos(mappedObject.GetType()).ToDictionary(p => p.Name, p2 => p2.GetValue(mappedObject));
        }

        public MapsDirectlyToDatabaseTableStatelessDefinition(Dictionary<string, object> properties) : base(typeof(T), properties)
        {
        }

        public void Rehydrate(T instance)
        {
            foreach (var property in Properties)
            {
                //HACK:
                if(property.Key.EndsWith("_ID"))
                    continue;
                
                instance.GetType().GetProperty(property.Key).GetSetMethod().Invoke(instance, new [] {property.Value});
            }
        }
    }
}