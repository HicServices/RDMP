using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    public interface IRevertable : IMapsDirectlyToDatabaseTable, ISaveable
    {
        void RevertToDatabaseState();
        RevertableObjectReport HasLocalChanges();
        bool Exists();
    }
}