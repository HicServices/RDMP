using System;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Refreshing
{
    public class RefreshObjectEventArgs
    {
        public DatabaseEntity Object { get; set; }
        public bool Exists { get; private set; }

        public RefreshObjectEventArgs(DatabaseEntity o)
        {
            Object = o;

            if(o == null)
                throw new ArgumentException("You cannot create a refresh on a null object","o");

            Exists = Object.Exists();
        }
    }
}