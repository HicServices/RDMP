using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    ///  Constructs IFilters etc for data extraction via SelectedDataSets (See IFilterFactory).  Each SelectedDataSets in an ExtractionConfiguration has (optionally)
    ///  it's own root container IFilters, subcontainers etc.
    /// </summary>
    public class DeployedExtractionFilterFactory : IFilterFactory
    {
        private readonly IDataExportRepository _repository;

        /// <summary>
        /// Prepares to create extraction filters for project datasets int eh provided <paramref name="repository"/>
        /// </summary>
        /// <param name="repository"></param>
        public DeployedExtractionFilterFactory(IDataExportRepository repository)
        {
            _repository = repository;
        }
    
        /// <inheritdoc/>
        public IFilter CreateNewFilter(string name)
        {
            return new DeployedExtractionFilter(_repository,name,null);
        }

        /// <inheritdoc/>
        public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
        {
            return new DeployedExtractionFilterParameter(_repository,parameterSQL,filter);
        }

        /// <inheritdoc/>
        public Type GetRootOwnerType()
        {
            return typeof (SelectedDataSets);
        }

        /// <inheritdoc/>
        public Type GetIContainerTypeIfAny()
        {
            return typeof (FilterContainer);
        }
    }
}