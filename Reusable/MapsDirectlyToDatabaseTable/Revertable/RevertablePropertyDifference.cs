using System.Reflection;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    public class RevertablePropertyDifference
    {
        public RevertablePropertyDifference(PropertyInfo property,object localValue,object databaseValue)
        {
            Property = property;
            LocalValue = localValue;
            DatabaseValue = databaseValue;
        }

        public PropertyInfo Property { get;private set; }
        public object LocalValue { get; private set; }
        public object DatabaseValue { get;private set; }
    }
}