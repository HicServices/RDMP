using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MapsDirectlyToDatabaseTable
{
    public class ObjectDeletedException : Exception
    {
        public IMapsDirectlyToDatabaseTable DeletedObject { get; set; }

        public ObjectDeletedException(IMapsDirectlyToDatabaseTable deletedObject, Exception inner = null) : base("Object " + deletedObject + " has been deleted from the database", inner)
        {
            DeletedObject = deletedObject;
        }
    }
}