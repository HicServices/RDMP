using System;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Records the user configured value of a property marked with [DemandsInitialization] declared on a data flow/dle component (including plugin components).
    ///  See Argument for full description.
    /// </summary>
    public interface IArgument:IMapsDirectlyToDatabaseTable,ISaveable
    {
        string Name { get; set; }
        string Description { get; set; }
        string Value { get; }
        string Type { get; }

        void SetValue(object o);

        object GetValueAsSystemType();
        Type GetSystemType();
        void SetType(Type t);
    }
}