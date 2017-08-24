using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Interfaces.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Repositories
{
    public class DataExportRepository : TableRepository, IDataExportRepository
    {
        public CatalogueRepository CatalogueRepository { get; private set; }
        
        public DataExportRepository(DbConnectionStringBuilder connectionString, CatalogueRepository catalogueRepository) : base(null, connectionString)
        {
            CatalogueRepository = catalogueRepository;

            //add our documentation to the repository too
            catalogueRepository.AddToHelp(GetType().Assembly);
        }
        
        public IEnumerable<ICumulativeExtractionResults> GetAllCumulativeExtractionResultsFor(IExtractionConfiguration configuration, IExtractableDataSet dataset)
        {
            return GetAllObjects<CumulativeExtractionResults>("WHERE ExtractionConfiguration_ID=" + configuration.ID + "AND ExtractableDataSet_ID=" + dataset.ID);
        }

        readonly ObjectConstructor _constructor = new ObjectConstructor();
        protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
        {
            return _constructor.ConstructIMapsDirectlyToDatabaseObject<IDataExportRepository>(t, this, reader);
            
        }

    }
}
