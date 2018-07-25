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
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode.Annotations;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Sometimes when extracting data in an ExtractionConfiguration of a Project you don't want to extract all the available (extractable) columns in a dataset.  For example you might
    /// have some columns which require 'special approval' to be released and most extracts will not include the columns.  ExtractableColumn is the object which records which columns in
    /// a given ExtractionConfiguration are being released to the researcher.  It also allows you to change the implementation of the column, for example a given researcher might want 
    /// all values UPPERd or he might want the Value field of Prescribing to be passed through his adjustment Scalar Valued Function to normalise or some other wierdness.
    /// 
    /// <para>When selecting a column for extraction in ExtractionConfigurationUI an ExtractableColumn will be created with a pointer to the original ExtractionInformation 
    /// (CatalogueExtractionInformation_ID) in the Catalogue database.  The ExtractionInformations SelectSQL will also be copied out.  The ExtractionQueryBuilder will use these records to
    /// assemble the correct SQL for each Catalogue in your ExtractionConfiguration.</para>
    /// 
    /// <para>The ExtractableColumn 'copy' process allows not only for you to modify the SelectSQL on a 'per extraction' basis but also it means that if you ever delete an ExtractionInformation
    /// from the Catalogue or change the implementation then the record in DataExportManager still reflects the values that were actually used to execute the extraction.  This means
    /// that if you clone a 10 year old extraction you will still get the same SQL (along with lots of warnings about orphan CatalogueExtractionInformation_ID etc).  It even allows you
    /// to delete entire datasets (Catalogues) without breaking old extractions (this is not a good idea though - you should always just deprecate the Catalogue instead).</para>
    /// </summary>
    public class ExtractableColumn : ConcreteColumn, IComparable, IInjectKnown<CatalogueItem>,IInjectKnown<ColumnInfo>, IInjectKnown<ExtractionInformation>
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
            set
            {
                SetField(ref _catalogueExtractionInformation_ID, value);
                ClearAllInjections();
            }
        }

        #endregion
        
        #region Relationships

        /// <inheritdoc cref="CatalogueExtractionInformation_ID"/>
        [CanBeNull]
        /// <inheritdoc cref="CatalogueExtractionInformation_ID"/>
        [NoMappingToDatabase]
        public ExtractionInformation CatalogueExtractionInformation
        {
            get
            {
                return _knownExtractionInformation.Value;
            }
        }

        [CanBeNull]
        [NoMappingToDatabase]
        public override ColumnInfo ColumnInfo
        {
            get
            {
                return _knownColumnInfo.Value;
            }
        }

        #endregion

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
            
            ClearAllInjections();
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
            
            ClearAllInjections();
        }

        #region value caching and injection
        private Lazy<CatalogueItem> _knownCatalogueItem;
        private Lazy<ColumnInfo> _knownColumnInfo;
        private Lazy<ExtractionInformation> _knownExtractionInformation;

        public void InjectKnown(CatalogueItem instance)
        {
            _knownCatalogueItem = new Lazy<CatalogueItem>(()=>instance);
        }

        public void InjectKnown(ColumnInfo instance)
        {
            _knownColumnInfo = new Lazy<ColumnInfo>(()=>instance);
        }
        public void InjectKnown(ExtractionInformation extractionInformation)
        {
            //_knownExtractionInformation = new Lazy<ExtractionInformation>(() => extractionInformation);

            if (extractionInformation == null)
            {
                InjectKnown((CatalogueItem)null);
                InjectKnown((ColumnInfo)(null));
            }
            else
            {
                InjectKnown(extractionInformation.CatalogueItem);
                InjectKnown(extractionInformation.ColumnInfo);
            }
        }

        public void ClearAllInjections()
        {
            _knownCatalogueItem = new Lazy<CatalogueItem>(FetchCatalogueItem);
            _knownExtractionInformation = new Lazy<ExtractionInformation>(FetchExtractionInformation);
            _knownColumnInfo = new Lazy<ColumnInfo>(FetchColumnInfo);
        }
        #endregion 

        public override string ToString()
        {
            if(!string.IsNullOrWhiteSpace(Alias))
                return Alias;

            return SelectSQL;
        }

        public int CompareTo(object obj)
        {
            if (obj is ExtractableColumn)
                return this.Order - (obj as ExtractableColumn).Order;

            throw new NotSupportedException("ExtractableColumn can only be compared with other ExtractableColumns");
        }
        
        public bool HasOriginalExtractionInformationVanished()
        {
            return ColumnInfo == null;
        }

        private ColumnInfo FetchColumnInfo()
        {
            var ci = _knownCatalogueItem.Value;
            if (ci == null || ci.ColumnInfo_ID == null)
                return null;

            return ci.ColumnInfo;
        }

        private CatalogueItem FetchCatalogueItem()
        {
            ExtractionInformation ei = _knownExtractionInformation.Value;

            if (ei == null)
                return null;
            
            return ei.CatalogueItem;
        }

        private ExtractionInformation FetchExtractionInformation()
        {
            //it's not based on a Catalogue column
            if (!CatalogueExtractionInformation_ID.HasValue)
                return null;
            
            try
            {
                return ((DataExportRepository)Repository).CatalogueRepository.GetObjectByID<ExtractionInformation>(CatalogueExtractionInformation_ID.Value);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the current state of the ExtractableColumn is different from the current state of the original <see cref="ExtractionInformation"/> that 
        /// it was cloned from.
        /// </summary>
        /// <returns></returns>
        public bool IsOutOfSync()
        {
            var ei = CatalogueExtractionInformation;

            if (ei != null)
                if (ei.IsExtractionIdentifier != IsExtractionIdentifier || ei.SelectSQL != SelectSQL)
                    return true;

            return false;
        }

        /// <summary>
        /// Copies all values (SelectSQL, Order, IsPrimaryKey etc from the specified <see cref="IColumn"/>) then saves to database.
        /// </summary>
        /// <param name="item"></param>
        public void UpdateValuesToMatch(IColumn item)
        {
            //Add new things you want to copy from the Catalogue here
            HashOnDataRelease = item.HashOnDataRelease;
            IsExtractionIdentifier = item.IsExtractionIdentifier;
            IsPrimaryKey = item.IsPrimaryKey;
            Order = item.Order;
            Alias = item.Alias;
            SelectSQL = item.SelectSQL;
            SaveToDatabase();
        }
    }
}
