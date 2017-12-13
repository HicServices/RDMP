using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Returns children for a given model object (any object in an RDMPCollectionUI).  This should be fast and your IChildProvider should pre load all the objects
    /// and then return them as needed when GetChildren is called.
    /// </summary>
    public interface IChildProvider
    {
        object[] GetChildren(object model);
        
    }
}