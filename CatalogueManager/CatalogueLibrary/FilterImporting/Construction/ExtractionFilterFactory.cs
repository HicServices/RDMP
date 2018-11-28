using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.FilterImporting.Construction
{
    /// <summary>
    /// Constructs IFilters etc for main Catalogue database filter (ExtractionFilter).  These are the master filters which are copied out as needed for cohort identification,
    /// extraction etc and therefore do not have any IContainer type (AND/OR).
    /// </summary>
    public class ExtractionFilterFactory : IFilterFactory
    {
        private readonly ICatalogueRepository _repository;
        private readonly ExtractionInformation _extractionInformation;

        /// <summary>
        /// Prepares to create master extraction filters at <see cref="Catalogue"/> level which can be reused in cohort generation, project extractions etc.  Filters created
        /// will be stored under the specific <paramref name="extractionInformation"/> (extractable column) provided.
        /// </summary>
        /// <param name="extractionInformation"></param>
        public ExtractionFilterFactory(ExtractionInformation extractionInformation)
        {
            _repository = (ICatalogueRepository)extractionInformation.Repository;
            _extractionInformation = extractionInformation;
        }

        /// <inheritdoc/>
        public IFilter CreateNewFilter(string name)
        {
            return new ExtractionFilter(_repository, name, _extractionInformation);
        }

        /// <inheritdoc/>
        public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
        {
            return new ExtractionFilterParameter(_repository, parameterSQL, (ExtractionFilter)filter);
        }

        /// <inheritdoc/>
        public Type GetRootOwnerType()
        {
            return typeof(ExtractionInformation);
        }

        /// <inheritdoc/>
        public Type GetIContainerTypeIfAny()
        {
            return null;
        }
    }
}