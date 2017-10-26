using System;
using System.Collections.Generic;

namespace CatalogueLibrary.Providers
{
    public interface IChildProvider
    {
        object[] GetChildren(object model);
    }
}