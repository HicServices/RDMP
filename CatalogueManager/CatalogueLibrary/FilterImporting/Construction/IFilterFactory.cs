using System;
using CatalogueLibrary.Data;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.FilterImporting.Construction
{
    public interface IFilterFactory
    {
        IFilter CreateNewFilter(string name);
        ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL);

        Type GetRootOwnerType();
        Type GetIContainerTypeIfAny();
    }
}
