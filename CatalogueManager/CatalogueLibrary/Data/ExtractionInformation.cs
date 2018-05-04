using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes in a single line of SELECT SQL a transform to perform on an underlying ColumnInfo.  ExtractionInformation is the technical implementation 
    /// of what is described by a CatalogueItem.  Most ExtractionInformations in your database will just be direct extraction (verbatim) of the ColumnInfo
    /// however you might have simple transformations e.g. 'UPPER([MyDatabase]..[Users].[Name]' or even call complex SQL scalar functions for example
    /// 'fn_CleanDrugCode([Prescribing]..[Items].[DrugCode])'
    /// 
    /// <para>Note that alias is stored separately because it is useful for GetRuntimeName().  Also note that you should not have newlines in your SelectSQL 
    /// since this will likely confuse QueryBuilder.</para>
    /// 
    /// <para>The interface ExtractionInformationUI handles all of these requirements transparentely.  Also recorded in ExtractionInformation is ExtractionCategory
    /// which lets you flag the sensitivity of the data being extracted e.g. SpecialApprovalRequired</para>
    /// 
    /// <para>Finally one ExtractionInformation (and only one) in each CatalogueItem set (of parent Catalogue) must be flagged as IsExtractionIdentifier.  This 
    /// is the column which will be joined against cohorts in data extraction linkages.  This should be the private identifier you use to identify people
    /// in your datasets (e.g. Community Health Index or NHS Number).</para>
    /// </summary>
    public class ExtractionInformation : ConcreteColumn, IDeleteable, IComparable, IHasDependencies, IInjectKnown<ColumnInfo>,IInjectKnown<CatalogueItem>
    {
        private bool _columnInfoFoundToBeNull = false;

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int SelectSQL_MaxLength = -1;
        
        

        #region Properties 
        
        private int _catalogueItemID;
        private ExtractionCategory _extractionCategory;
        
        //For other properties see ConcreteColumn
        public int CatalogueItem_ID
        {
            get { return _catalogueItemID; }
            set { SetField(ref _catalogueItemID , value); }
        }

        public ExtractionCategory ExtractionCategory
        {
            get { return _extractionCategory; }
            set
            {
                if (_extractionCategory == ExtractionCategory.Any)
                    throw new ArgumentException("Any is only usable as an extraction argument and cannot be assigned to an ExtractionInformation");

                SetField(ref _extractionCategory, value);
            }
        }
        
        #endregion

        #region Relationships
        //These fields are fetched (cached version) from lookup link table - ExtractionInformation can only exist where there is a relationship between a CatalogueItem and a ColumnInfo
        /// <inheritdoc cref="CatalogueItem_ID"/>
        [NoMappingToDatabase]
        public CatalogueItem CatalogueItem
        {
            get
            {
                //Cache answer the first time it is requested (or injected)
                return _knownCatalogueItem.Value;
            }
        }

        /// <summary>
        /// The ColumnInfo that underlies this extractable column.  ExtractionInformation allows for transforms, governance rules and indicates extractability (Core / Supplemental etc)
        /// while the ColumnInfo is the concrete/immutable reference to the underlying column in the database from which the SelectSQL is executed.  This determines what tables are 
        /// joined on during query generation and which servers are connected to during query execution etc.  
        /// 
        /// <para>This field can be null only if the <see cref="ColumnInfo"/> has been deleted rendering this an orphan and broken.  This is considered a problem by 
        /// <see cref="CatalogueLibrary.Providers.CatalogueProblemProvider"/> and as such it is the users responsibility to fix it, you shouldn't worry too much about null
        /// checking this field.</para> 
        /// </summary>
        [NoMappingToDatabase]
        public override ColumnInfo ColumnInfo
        {
            get
            {
                return _knownColumninfo.Value;
            }
        }

        [NoMappingToDatabase]
        public IEnumerable<ExtractionFilter> ExtractionFilters {
            get { return Repository.GetAllObjectsWithParent<ExtractionFilter>(this); }
        }
        #endregion


        public ExtractionInformation(ICatalogueRepository repository, CatalogueItem catalogueItem, ColumnInfo column, string selectSQL)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"SelectSQL", string.IsNullOrWhiteSpace(selectSQL) ? column.Name : selectSQL},
                {"Order", 1},
                {"ExtractionCategory", "Core"},
                {"CatalogueItem_ID",catalogueItem.ID}
            });

            if (catalogueItem.ColumnInfo_ID == null)
                repository.SaveSpecificPropertyOnlyToDatabase(catalogueItem, "ColumnInfo_ID", column.ID);
            else
                if (catalogueItem.ColumnInfo_ID != column.ID)
                    throw new ArgumentException("Cannot create an ExtractionInformation for CatalogueItem " +
                                                catalogueItem + " with ColumnInfo " + column +
                                                " because the CatalogueItem is already associated with a different ColumnInfo: " +
                                                catalogueItem.ColumnInfo);
            ClearAllInjections();
        }

        internal ExtractionInformation(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            SelectSQL = r["SelectSQL"].ToString();

            ExtractionCategory cat;
            if (ExtractionCategory.TryParse(r["ExtractionCategory"].ToString(), out cat))
                ExtractionCategory = cat;
            else
                throw new Exception("Unrecognised ExtractionCategory \"" + r["ExtractionCategory"] + "\"");

            Order = int.Parse(r["Order"].ToString());

            Alias = r["Alias"] as string;

            HashOnDataRelease = (bool)r["HashOnDataRelease"];
            IsExtractionIdentifier = (bool) r["IsExtractionIdentifier"];
            IsPrimaryKey = (bool) r["IsPrimaryKey"];
            CatalogueItem_ID = (int) r["CatalogueItem_ID"];

            ClearAllInjections();
        }

        private Lazy<ColumnInfo> _knownColumninfo;
        private Lazy<CatalogueItem> _knownCatalogueItem;

        public void ClearAllInjections()
        {
            _knownColumninfo = new Lazy<ColumnInfo>(()=>CatalogueItem.ColumnInfo);
            _knownCatalogueItem = new Lazy<CatalogueItem>(() => Repository.GetObjectByID<CatalogueItem>(CatalogueItem_ID));
        }


        public void InjectKnown(ColumnInfo instance)
        {
            _knownColumninfo = new Lazy<ColumnInfo>(()=>instance);
        }

        public void InjectKnown(CatalogueItem instance)
        {
            _knownCatalogueItem = new Lazy<CatalogueItem>(() => instance);
        }
        
        public override string ToString()
        {
            //prefer alias, then prefer catalogue name
            if (!string.IsNullOrWhiteSpace(Alias))
                return Alias;
            try
            {
                return GetRuntimeName();
            }
            catch (Exception)
            {
                return "BROKEN ExtractionInformation:" + SelectSQL;
            }
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public int CompareTo(object obj)
        {
            if(obj is ExtractionInformation)
            {
                return this.Order - (obj as ExtractionInformation).Order;
            }
            else
                return 0;
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return ColumnInfo != null? new IHasDependencies[] {ColumnInfo}: new IHasDependencies[0];
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            List<IHasDependencies> dependencies = new List<IHasDependencies>();
            
            dependencies.AddRange(ExtractionFilters);
            dependencies.Add(CatalogueItem);

            return dependencies.ToArray();
        }

        public bool IsProperTransform()
        {
            if (string.IsNullOrWhiteSpace(SelectSQL))
                return false;

            if (ColumnInfo == null)
                return false;

            //if the selct sql is different from the column underlying it then it is a proper transform (not just a copy paste)
            return !SelectSQL.Equals(ColumnInfo.Name);
        }

    }
}
