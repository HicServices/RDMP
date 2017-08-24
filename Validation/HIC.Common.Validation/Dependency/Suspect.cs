using System;
using System.Collections.Generic;
using System.Reflection;

namespace HIC.Common.Validation.Dependency
{
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