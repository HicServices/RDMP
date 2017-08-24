using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
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