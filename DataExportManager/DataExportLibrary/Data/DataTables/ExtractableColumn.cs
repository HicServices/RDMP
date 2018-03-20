using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Sometimes when extracting data in an ExtractionConfiguration of a Project you don't want to extract all the available (extractable) columns in a dataset.  For example you might
    /// have some columns which require 'special approval' to be released and most extracts will not include the columns.  ExtractableColumn is the object which records which columns in
    /// a given ExtractionConfiguration are being released to the researcher.  It also allows you to change the implementation of the column, for example a given researcher might want 
    /// all values UPPERd or he might want the Value field of Prescribing to be passed through his adjustment Scalar Valued Function to normalise or some other wierdness.
    /// 
    /// When selecting a column for extraction in ExtractionConfigurationUI an ExtractableColumn will be created with a pointer to the original ExtractionInformation 
    /// (CatalogueExtractionInformation_ID) in the Catalogue database.  The ExtractionInformations SelectSQL will also be copied out.  The ExtractionQueryBuilder will use these records to
    /// assemble the correct SQL for each Catalogue in your ExtractionConfiguration.
    /// 
    /// The ExtractableColumn 'copy' process allows not only for you to modify the SelectSQL on a 'per extraction' basis but also it means that if you ever delete an ExtractionInformation
    /// from the Catalogue or change the implementation then the record in DataExportManager still reflects the values that were actually used to execute the extraction.  This means
    /// that if you clone a 10 year old extraction you will still get the same SQL (along with lots of warnings about orphan CatalogueExtractionInformation_ID etc).  It even allows you
    /// to delete entire datasets (Catalogues) without breaking old extractions (this is not a good idea though - you should always just deprecate the Catalogue instead).
    /// </summary>
    public class ExtractableColumn : ConcreteColumn, IComparable
    {
        #region Database Properties
        private int _extractableDataSet_ID;
        private int _extractionConfiguration_ID;
        private int? _catalogueExtractionInformation_ID;

        public int ExtractableDataSet_ID
        {
            get { return _extractableDataSet_ID; }
            set { SetField(ref _extractableDataSet_ID, value); }
        }
        public int ExtractionConfiguration_ID
        {
            get { return _extractionConfiguration_ID; }
            set { SetField(ref _extractionConfiguration_ID, value); }
        }
        public int? CatalogueExtractionInformation_ID
        {
            get { return _catalogueExtractionInformation_ID; }
            set { SetField(ref _catalogueExtractionInformation_ID, value); }
        }

        #endregion
        
        private ColumnInfo _columnInfo = null;

        #region Relationships
        
        [NoMappingToDatabase]
        public ExtractionInformation CatalogueExtractionInformation
        {
            get
            {
                if (CatalogueExtractionInformation_ID == null)
                    return null;

                return ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<ExtractionInformation>(CatalogueExtractionInformation_ID.Value);
            }
        }

        [NoMappingToDatabase]
        public override ColumnInfo ColumnInfo
        {
            get
            {
                return _columnInfo;
            }
        }

        #endregion
        
        private string _catalogueItemName;

        public ExtractableColumn(IDataExportRepository repository, IExtractableDataSet dataset, ExtractionConfiguration configuration, ExtractionInformation extractionInformation, int order, string selectSQL)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ExtractableDataSet_ID", dataset.ID},
                {"ExtractionConfiguration_ID", configuration.ID},
                {"CatalogueExtractionInformation_ID", extractionInformation == null ? DBNull.Value : (object)extractionInformation.ID},
                {"Order", order},
                {"SelectSQL", string.IsNullOrWhiteSpace(selectSQL) ? DBNull.Value : (object)selectSQL}
            });

            LookupCatalogueStuff();
        }

        internal ExtractableColumn(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractableDataSet_ID = int.Parse(r["ExtractableDataSet_ID"].ToString());
            ExtractionConfiguration_ID = int.Parse(r["ExtractionConfiguration_ID"].ToString());

            if (r["CatalogueExtractionInformation_ID"] == DBNull.Value)
                CatalogueExtractionInformation_ID = null;
            else
                CatalogueExtractionInformation_ID = int.Parse(r["CatalogueExtractionInformation_ID"].ToString());

            SelectSQL = r["SelectSQL"] as string;
            Order = int.Parse(r["Order"].ToString());
            Alias = r["Alias"] as string;
            HashOnDataRelease = (bool)r["HashOnDataRelease"];
            IsExtractionIdentifier = (bool)r["IsExtractionIdentifier"];
            IsPrimaryKey = (bool) r["IsPrimaryKey"];

            LookupCatalogueStuff();

        }

        private void LookupCatalogueStuff()
        {
            CatalogueItem catalogueItem = null;

            if (CatalogueExtractionInformation_ID != null)
            {
                bool exists = ((DataExportRepository)Repository).CatalogueRepository.StillExists<ExtractionInformation>(CatalogueExtractionInformation_ID.Value);

                //We have been orphaned! we have an CatalogueExtractionInformation_ID which should point to the Catalogue object but the Catalogue object has been deleted
                if (!exists)
                    CatalogueExtractionInformation_ID = null;
                else
                    catalogueItem = CatalogueExtractionInformation.CatalogueItem;
            }

            //the ExtractionInformation has been deleted in the Catalogue!
            if (catalogueItem == null)
                _catalogueItemName = null;
            else
                _catalogueItemName = catalogueItem.Name;//it hasn't, copy down the name of it

            LookupColumnInfo();
        }

        private void LookupColumnInfo()
        {
            if (_columnInfo == null && CatalogueExtractionInformation_ID != null)
                _columnInfo = CatalogueExtractionInformation.CatalogueItem.ColumnInfo;
        }
        
        public override string ToString()
        {
            if(!string.IsNullOrWhiteSpace(Alias))
                return Alias;

            if (_catalogueItemName == null)
                return SelectSQL;

            return _catalogueItemName;
        }

        public int CompareTo(object obj)
        {
            if (obj is ExtractableColumn)
                return this.Order - (obj as ExtractableColumn).Order;

            throw new NotSupportedException("ExtractableColumn can only be compared with other ExtractableColumns");
        }
        
        public bool HasOriginalExtractionInformationVanished()
        {
            LookupCatalogueStuff();
            return _columnInfo == null;
        }
    }
}
