using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Providers
{
    public interface IChildProvider
    {
        object[] GetChildren(object model);
        
    }
}