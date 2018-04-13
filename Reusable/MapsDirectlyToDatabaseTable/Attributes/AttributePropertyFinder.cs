using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapsDirectlyToDatabaseTable.Attributes
{
    /// <summary>
    /// Implementation of <see cref="IAttributePropertyFinder"/> in which a specific Attribute only is looked for.  The
    /// Attribute is specified by the generic T
    /// </summary>
    /// <typeparam name="T">The specific attribute you are looking for e.g. SqlAttribute</typeparam>
    public class AttributePropertyFinder<T> : IAttributePropertyFinder where T : Attribute
    {
       readonly Dictionary<Type, HashSet<PropertyInfo>> _properties = new Dictionary<Type, HashSet<PropertyInfo>>();

        public AttributePropertyFinder(IEnumerable<IMapsDirectlyToDatabaseTable> objects) 
        {
            foreach (Type type in objects.Select(o => o.GetType()).Distinct())
            {
                PropertyInfo[] propertyInfos = type.GetProperties();

                foreach (PropertyInfo property in propertyInfos)
                {
                    //if property has sql flag
                    if (property.GetCustomAttributes(typeof(T), true).Any())
                    {
                        if(!_properties.ContainsKey(type))
                            _properties.Add(type, new HashSet<PropertyInfo>());

                        if(!_properties[type].Contains(property))
                            _properties[type].Add(property);
                    }
                }
            }
        }

        public AttributePropertyFinder(IMapsDirectlyToDatabaseTable o): this(new[] { o})
        {
            
        }

        public IEnumerable<PropertyInfo> GetProperties(IMapsDirectlyToDatabaseTable o)
        {
            var t = o.GetType();

            if (_properties.ContainsKey(t))
                return _properties[t];

            return new PropertyInfo[0];
        }

        /// <summary>
        /// Returns true if the provided object has a property that matches the expected attribute
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool ObjectContainsProperty(IMapsDirectlyToDatabaseTable arg)
        {
            return _properties.ContainsKey(arg.GetType());
        }

        public T GetAttribute(PropertyInfo property)
        {
            return (T) property.GetCustomAttributes(typeof (T), true).SingleOrDefault();
        }
    }
}
