using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    /// <summary>
    /// Object (usually a IMapsDirectlyToDatabaseTable) which can have it's state saved into a database but also have it's current state compared with the 
    /// database state and (if nessesary) unsaved changes can be discarded.
    /// </summary>
    public interface IRevertable : IMapsDirectlyToDatabaseTable, ISaveable
    {
        void RevertToDatabaseState();
        RevertableObjectReport HasLocalChanges();
        bool Exists();
    }
}