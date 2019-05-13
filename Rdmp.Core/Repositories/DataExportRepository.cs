// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.Repositories
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
        /// <summary>
        /// The paired Catalogue database which contains non extract metadata (i.e. datasets, aggregates, data loads etc).  Some objects in this database
        /// contain references to objects in the CatalogueRepository.
        /// </summary>
        public ICatalogueRepository CatalogueRepository { get; private set; }

        public IFilterManager FilterManager { get; private set; }

        public IDataExportPropertyManager DataExportPropertyManager { get; private set; }

        public IExtractableDataSetPackageManager PackageManager { get; set; }


        public DataExportRepository(DbConnectionStringBuilder connectionString, ICatalogueRepository catalogueRepository) : base(null, connectionString)
        {
            CatalogueRepository = catalogueRepository;
            
            FilterManager = new DataExportFilterManager(this);

            DataExportPropertyManager = new DataExportPropertyManager(false,this);
            PackageManager = new ExtractableDataSetPackageManager(this);

            Constructors.Add(typeof(SupplementalExtractionResults),(rep,r)=>new SupplementalExtractionResults((IDataExportRepository)rep,r));
            Constructors.Add(typeof(CumulativeExtractionResults),(rep,r)=>new CumulativeExtractionResults((IDataExportRepository)rep,r));
            Constructors.Add(typeof(DeployedExtractionFilter),(rep,r)=>new DeployedExtractionFilter((IDataExportRepository)rep,r));
            Constructors.Add(typeof(DeployedExtractionFilterParameter),(rep,r)=>new DeployedExtractionFilterParameter((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ExternalCohortTable),(rep,r)=>new ExternalCohortTable((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ExtractableCohort),(rep,r)=>new ExtractableCohort((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ExtractableColumn),(rep,r)=>new ExtractableColumn((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ExtractableDataSet),(rep,r)=>new ExtractableDataSet((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ExtractionConfiguration),(rep,r)=>new ExtractionConfiguration((IDataExportRepository)rep,r));
            Constructors.Add(typeof(FilterContainer),(rep,r)=>new FilterContainer((IDataExportRepository)rep,r));
            Constructors.Add(typeof(GlobalExtractionFilterParameter),(rep,r)=>new GlobalExtractionFilterParameter((IDataExportRepository)rep,r));
            Constructors.Add(typeof(Project),(rep,r)=>new Project((IDataExportRepository)rep,r));
            Constructors.Add(typeof(SelectedDataSets),(rep,r)=>new SelectedDataSets((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ExtractableDataSetPackage),(rep,r)=>new ExtractableDataSetPackage((IDataExportRepository)rep,r));
            Constructors.Add(typeof(ProjectCohortIdentificationConfigurationAssociation),(rep,r)=>new ProjectCohortIdentificationConfigurationAssociation((IDataExportRepository)rep,r));
            Constructors.Add(typeof(SelectedDataSetsForcedJoin), (rep, r) => new SelectedDataSetsForcedJoin((IDataExportRepository)rep, r));
        }
        
        public IEnumerable<ICumulativeExtractionResults> GetAllCumulativeExtractionResultsFor(IExtractionConfiguration configuration, IExtractableDataSet dataset)
        {
            return GetAllObjects<CumulativeExtractionResults>("WHERE ExtractionConfiguration_ID=" + configuration.ID + "AND ExtractableDataSet_ID=" + dataset.ID);
        }

        public IEnumerable<ISupplementalExtractionResults> GetAllGlobalExtractionResultsFor(IExtractionConfiguration configuration)
        {
            return GetAllObjects<SupplementalExtractionResults>("WHERE ExtractionConfiguration_ID=" + configuration.ID + "AND CumulativeExtractionResults_ID IS NULL");
        }

        readonly ObjectConstructor _constructor = new ObjectConstructor();
        protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
        {
            if (Constructors.ContainsKey(t))
                return Constructors[t](this, reader);

            return _constructor.ConstructIMapsDirectlyToDatabaseObject<IDataExportRepository>(t, this, reader);
        }
        
        public CatalogueExtractabilityStatus GetExtractabilityStatus(ICatalogue c)
        {
            var eds = GetAllObjectsWithParent<ExtractableDataSet>(c).SingleOrDefault();
            if (eds == null)
                return new CatalogueExtractabilityStatus(false, false);

            return eds.GetCatalogueExtractabilityStatus();
        }
        
        public ISelectedDataSets[] GetSelectedDatasetsWithNoExtractionIdentifiers()
        {
            return SelectAll<SelectedDataSets>(@"
SELECT ID  FROM SelectedDataSets sds
where not exists (
select 1 FROM ExtractableColumn ec where 
ec.ExtractableDataSet_ID = sds.ExtractableDataSet_ID
AND
ec.IsExtractionIdentifier = 1
)","ID").ToArray();
        }
    }
}
