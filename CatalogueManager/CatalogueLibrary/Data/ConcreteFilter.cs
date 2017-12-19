using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Abstract base class for all IFilters which are database entities (Stored in the Catalogue/Data Export database as objects). 
    /// 
    /// ConcreteFilter is used to provide UI editing of an IFilter without having to add persistence / VersionedDatabaseEntity logic to IFilter (which would break 
    /// SpontaneouslyInventedFilters)
    /// </summary>
    public abstract class ConcreteFilter :  VersionedDatabaseEntity,IFilter
    {
        protected ConcreteFilter(IRepository repository,DbDataReader r) : base(repository, r)
        {
            
        }

        protected ConcreteFilter():base()
        {
            
        }

        #region Database Properties

        
        private string _whereSQL;
        private string _name;
        private string _description;
        private bool _isMandatory;


        public string WhereSQL
        {
            get { return _whereSQL; }
            set { SetField(ref  _whereSQL, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        

        //mandatory filters only applies to the catalogues so setting it on AggregateFilters doesn't make much sense... at least for now anyway
        public bool IsMandatory
        {
            get { return _isMandatory; }
            set { SetField(ref  _isMandatory, value); }
        }

        #endregion

        public abstract int? ClonedFromExtractionFilter_ID { get; set; }
        public abstract int? FilterContainer_ID { get; set; }

        public abstract ISqlParameter[] GetAllParameters();

        #region Relationships
        
        [NoMappingToDatabase]
        public abstract IContainer FilterContainer { get; }
        
        #endregion

        public abstract ColumnInfo GetColumnInfoIfExists();
        public abstract IFilterFactory GetFilterFactory();
        public abstract Catalogue GetCatalogue();


        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new QuerySyntaxHelperFactory().Create(GetDatabaseType());
        }

        private DatabaseType? _cachedDatabaseTypeAnswer;

        protected DatabaseType GetDatabaseType()
        {
            if (_cachedDatabaseTypeAnswer != null)
                return _cachedDatabaseTypeAnswer.Value;

            var col = GetColumnInfoIfExists();
            if (col != null)
                _cachedDatabaseTypeAnswer = col.TableInfo.DatabaseType;
            else
                _cachedDatabaseTypeAnswer = GetCatalogue().GetDistinctLiveDatabaseServerType();

            
            return _cachedDatabaseTypeAnswer.Value;
        }
    }
}