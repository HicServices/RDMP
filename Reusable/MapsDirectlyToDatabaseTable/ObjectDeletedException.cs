using System;
using System.Diagnostics;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Thrown when you attempt an operation on an IMapsDirectlyToDatabaseTable object that has had it's database entry deleted (either by you or another system 
    /// user).  All IMapsDirectlyToDatabaseTable objects must exist both in memory and in the database and once DeleteInDatabase is called on an IDeletable (or
    /// however else the database copy disapears) then the memory copy becomes invalid and should not be used for anything.
    /// </summary>
    public class ObjectDeletedException : Exception
    {
        public IMapsDirectlyToDatabaseTable DeletedObject { get; set; }

        public ObjectDeletedException(IMapsDirectlyToDatabaseTable deletedObject, Exception inner = null) : base("Object " + deletedObject + " has been deleted from the database", inner)
        {
            DeletedObject = deletedObject;
        }
    }
}