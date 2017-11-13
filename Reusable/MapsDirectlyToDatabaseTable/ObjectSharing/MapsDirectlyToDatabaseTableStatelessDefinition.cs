using System;
using System.Collections.Generic;
using System.Linq;

namespace MapsDirectlyToDatabaseTable.ObjectSharing
{
    public class MapsDirectlyToDatabaseTableStatelessDefinition
    {
        public Type Type { get; set; }
        public Dictionary<string, object> Properties { get; private set; }

        //Json!
        public MapsDirectlyToDatabaseTableStatelessDefinition()
        {
            Properties = new Dictionary<string, object>();
        }

        public MapsDirectlyToDatabaseTableStatelessDefinition(Type type, Dictionary<string,object> properties)
        {
            if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                throw new ArgumentException("Type must be IMapsDirectlyToDatabaseTable","type");

            Type = type;
            Properties = properties;
        }

        public MapsDirectlyToDatabaseTableStatelessDefinition(IMapsDirectlyToDatabaseTable m)
        {
            Type = m.GetType();
            Properties = TableRepository.GetPropertyInfos(m.GetType()).ToDictionary(p => p.Name, p2 => p2.GetValue(m));
        }
    }
}