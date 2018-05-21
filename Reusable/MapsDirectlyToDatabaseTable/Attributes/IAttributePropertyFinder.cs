using System.Collections.Generic;
using System.Reflection;

namespace MapsDirectlyToDatabaseTable.Attributes
{
    /// <summary>
    /// Helper for finding public properties on objects which are decorated with a given Attribute.  Includes
    /// support for set/get and rapidly determining which Types have relevant properties.
    /// </summary>
    public interface IAttributePropertyFinder
    {
        IEnumerable<PropertyInfo> GetProperties(IMapsDirectlyToDatabaseTable o);
        bool ObjectContainsProperty(IMapsDirectlyToDatabaseTable arg);
    }
}