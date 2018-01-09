using System;
using System.Collections.Generic;
using System.Reflection;

namespace HIC.Common.Validation.Dependency
{
    /// <summary>
    /// Regex pattern for finding references in ValidatorXML without having to deserialize it.  This is used to identify rules which reference columns and ensure
    /// that they cannot be deleted (See ValidationXMLObscureDependencyFinder)
    /// </summary>
    public class Suspect
    {
        public string Pattern { get;private set; }
        public Type Type { get; private set; }
        public List<PropertyInfo> SuspectProperties = new List<PropertyInfo>();
        public List<FieldInfo> SuspectFields = new List<FieldInfo>();

        public Suspect(string pattern, Type type, List<PropertyInfo> suspectProperties, List<FieldInfo> suspectFields)
        {
            Pattern = pattern;
            Type = type;
            SuspectProperties = suspectProperties;
            SuspectFields = suspectFields;
        }
    }
}