using System.Collections.Generic;
using System.Reflection;
using MapsDirectlyToDatabaseTable;

namespace Sharing.Dependency.Gathering
{
    public interface IAttributePropertyFinder
    {
        IEnumerable<PropertyInfo> GetProperties(IMapsDirectlyToDatabaseTable o);
        bool ObjectContainsProperty(IMapsDirectlyToDatabaseTable arg);
    }
}