using System;
using System.Reflection;
using ReusableLibraryCode.DataAccess;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    public interface IExternalDatabasePatcher
    {
        void CheckAndPatch(IDataAccessPoint serverToPatch, Type rootType, string databaseAssemblyName);
        void CheckAndPatch(IDataAccessPoint serverToPatch, Type rootType, Assembly databaseAssembly);
    }
}