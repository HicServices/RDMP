using System;
using CatalogueLibrary.Data;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.FilterImporting.Construction
{
    /// <summary>
    /// Facilitates the creation of IFilter (lines of WHERE Sql) and ISqlParameter (sql parameters - DECLARE @bob as varchar(10)) instances.
    /// </summary>
    public interface IFilterFactory
    {
        IFilter CreateNewFilter(string name);
        ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL);

        /// <summary>
        /// The object Type which owns the root container e.g. if the IFilter is AggregateFilter then the IContainer Type is AggregateFilterContainers and the
        /// Root Owner Type is AggregateConfiguration
        /// </summary>
        /// <returns></returns>
        Type GetRootOwnerType();

        /// <summary>
        /// If the IFilter Type is designed to be held in IContainers then this method should return the Type of IContainer e.g. AggregateFilters belong in 
        /// AggregateFilterContainers
        /// </summary>
        /// <returns></returns>
        Type GetIContainerTypeIfAny();
    }
}
