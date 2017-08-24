using System;
using System.Collections.Generic;

namespace CatalogueLibrary.Providers
{
    public interface IChildProvider
    {
        List<Exception> Exceptions { get; }
        object[] GetChildren(object model);
    }
}