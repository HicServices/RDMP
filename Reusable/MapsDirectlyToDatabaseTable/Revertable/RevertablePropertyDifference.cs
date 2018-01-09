using System.Reflection;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    /// <summary>
    /// Summarises the difference in a single Property of an IRevertable object vs the corresponding currently saved database record.  Changes
    /// can be the result of local in memory changes the user has made but not saved yet or changes other users have made and saved since the IRevertable 
    /// was fetched.
    /// </summary>
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