using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MapsDirectlyToDatabaseTable;

namespace Sharing.Dependency.Gathering
{
    internal class SqlPropertyFinder
    {
        readonly Dictionary<Type, HashSet<PropertyInfo>> _properties = new Dictionary<Type, HashSet<PropertyInfo>>();

        public SqlPropertyFinder(IEnumerable<IMapsDirectlyToDatabaseTable> objects)
        {
            foreach (Type type in objects.Select(o => o.GetType()).Distinct())
            {
                PropertyInfo[] propertyInfos = type.GetProperties();

                foreach (PropertyInfo property in propertyInfos)
                {
                    //if property has sql flag
                    if(property.GetCustomAttributes(typeof(SqlAttribute), true).Any())
                    {
                        if(!_properties.ContainsKey(type))
                            _properties.Add(type, new HashSet<PropertyInfo>());

                        if(!_properties[type].Contains(property))
                            _properties[type].Add(property);
                    }
                }
            }
        }

        public IEnumerable<PropertyInfo> GetSqlProperties(IMapsDirectlyToDatabaseTable o)
        {
            var t = o.GetType();

            if (_properties.ContainsKey(t))
                return _properties[t];

            return new PropertyInfo[0];
        }
    }
}
