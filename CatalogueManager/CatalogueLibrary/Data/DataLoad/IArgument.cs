using System;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface IArgument
    {
        string Name { get; set; }
        string Description { get; set; }
        string Value { get; }

        void SetValue(object o);

        object GetValueAsSystemType();
        Type GetSystemType();
        void SetType(Type t);
    }
}