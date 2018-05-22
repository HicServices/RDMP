using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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
    /// <summary>
    /// Pointer to the Data Export Repository database in which all DatabaseEntities declared in DataExportLibrary.dll are stored.  Ever DatabaseEntity class must exist in a
    /// Microsoft Sql Server Database (See DatabaseEntity) and each object is compatible only with a specific type of TableRepository (i.e. the database that contains the
    /// table matching their name).  CatalogueLibrary.dll objects in CatalogueRepository, DataExportLibrary.dll objects in DataExportRepository, DataQualityEngine.dll objects
    /// in DQERepository etc.
    /// 
    /// <para>This class allows you to fetch objects and should be passed into constructors of classes you want to construct in the Data Export database.  This includes extraction
    /// Projects, ExtractionConfigurations, ExtractableCohorts etc.</para>
    /// 
    /// <para>Data Export databases are only valid when you have a CatalogueRepository database too and are always paired to a specific CatalogueRepository database (i.e. there are
    /// IDs in the data export database that specifically map to objects in the Catalogue database).  You can use the CatalogueRepository property to fetch/create objects
    /// in the paired Catalogue database.</para>
    /// </summary>
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

        public IEnumerable<ISupplementalExtractionResults> GetAllGlobalExtractionResultsFor(IExtractionConfiguration configuration)
        {
            return GetAllObjects<SupplementalExtractionResults>("WHERE ExtractionConfiguration_ID=" + configuration.ID + "AND ExtractableDataSet_ID IS NULL");
        }

        readonly ObjectConstructor _constructor = new ObjectConstructor();
        protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
        {
            return _constructor.ConstructIMapsDirectlyToDatabaseObject<IDataExportRepository>(t, this, reader);
            
        }

        public CatalogueExtractabilityStatus GetExtractabilityStatus(ICatalogue c)
        {
            var eds = GetAllObjectsWithParent<ExtractableDataSet>(c).SingleOrDefault();
            if (eds == null)
                return new CatalogueExtractabilityStatus(false, false);

            return eds.GetCatalogueExtractabilityStatus();
        }
    }
}
