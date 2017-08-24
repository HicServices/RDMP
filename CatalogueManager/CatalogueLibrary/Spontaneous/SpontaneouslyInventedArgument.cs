using System;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Spontaneous
{
    public class SpontaneouslyInventedArgument : SpontaneousObject, IArgument
    {
        private readonly object _value;

        public SpontaneouslyInventedArgument(string name, object value)
        {
            Name = name;
            _value = value;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Value { get { return _value.ToString(); }}


        public void SetValue(object o)
        {
            throw new NotSupportedException();
        }

        public object GetValueAsSystemType()
        {
            return _value;
        }

        public Type GetSystemType()
        {
            return _value.GetType();
        }

        public void SetType(Type t)
        {
            throw new NotImplementedException();
        }
    }
}