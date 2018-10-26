using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Referencing
{
    public interface IReferenceOtherObject
    {
        //from 
        bool IsReferenceTo(Type type);
        bool IsReferenceTo(IMapsDirectlyToDatabaseTable o);
    }
}
