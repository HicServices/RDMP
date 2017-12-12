using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    ///  Constructs IFilters etc for data extraction via SelectedDataSets (See IFilterFactory).  Each SelectedDataSets in an ExtractionConfiguration has (optionally)
    ///  it's own root container IFilters, subcontainers etc.
    /// </summary>
    public class DeployedExtractionFilterFactory : IFilterFactory
    {
        private readonly IDataExportRepository _repository;

        public DeployedExtractionFilterFactory(IDataExportRepository repository)
        {
            _repository = repository;
        }
    
        public IFilter CreateNewFilter(string name)
        {
            return new DeployedExtractionFilter(_repository,name,null);
        }

        public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
        {
            return new DeployedExtractionFilterParameter(_repository,parameterSQL,filter);
        }

        public Type GetRootOwnerType()
        {
            return typeof (SelectedDataSets);
        }

        public Type GetIContainerTypeIfAny()
        {
            return typeof (FilterContainer);
        }
    }
}