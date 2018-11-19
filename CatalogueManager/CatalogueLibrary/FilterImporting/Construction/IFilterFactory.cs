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
        /// <summary>
        /// Creates a new blank <see cref="IFilter"/> with the provided <paramref name="name"/>.  Each implementation of this method may return a 
        /// different Type of filter but should be consistent with a given implementation.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFilter CreateNewFilter(string name);

        /// <summary>
        /// Creates a new <see cref="ISqlParameter"/> with the provided <paramref name="parameterSQL"/> for use with the provided <paramref name="filter"/>.
        /// Each implementation of this method may return a different Type of <see cref="ISqlParameter"/> but should be consistent with a given implementation.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameterSQL"></param>
        /// <returns></returns>
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
