using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.FilterImporting.Construction
{
    /// <summary>
    /// Constructs IFilters etc for AggregateConfigurations (See IFilterFactory)
    /// </summary>
    public class AggregateFilterFactory : IFilterFactory
    {
        private readonly ICatalogueRepository _repository;

        /// <summary>
        /// Sets class up to create <see cref="AggregateFilter"/> objects in the provided <paramref name="repository"/>
        /// </summary>
        /// <param name="repository"></param>
        public AggregateFilterFactory(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public IFilter CreateNewFilter(string name)
        {
            return new AggregateFilter(_repository,name);
        }

        /// <inheritdoc/>
        public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
        {
            return new AggregateFilterParameter(_repository,parameterSQL,(AggregateFilter)filter);
        }

        /// <inheritdoc/>
        public Type GetRootOwnerType()
        {
            return typeof (AggregateConfiguration);
        }

        /// <inheritdoc/>
        public Type GetIContainerTypeIfAny()
        {
            return typeof (AggregateFilterContainer);
        }
    }
}
