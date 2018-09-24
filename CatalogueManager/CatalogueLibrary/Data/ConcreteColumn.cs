using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Common abstract base class for ExtractionInformation (how to extract a given ColumnInfo) and ExtractableColumn (clone into data export database of an 
    /// ExtractionInformation - i.e. 'extract column A on for Project B Configuration 'Cases' where A would be an ExtractionInformation defined in the Catalogue 
    /// database and copied out for use in the data extraction configuration).
    /// 
    /// <para>Provides an implementation of IColumn whilst still being a DatabaseEntity (saveable / part of a database repository etc)</para>
    /// </summary>
    public abstract class ConcreteColumn : VersionedDatabaseEntity, IColumn,IOrderable,IComparable
    {
        #region Database Properties
 
        private string _selectSql;
        private string _alias;
        private bool _hashOnDataRelease;
        private bool _isExtractionIdentifier;
        private bool _isPrimaryKey;
        private int _order;

        /// <summary>
        /// The order the column should be in when part of a SELECT statement built by an <see cref="CatalogueLibrary.QueryBuilding.ISqlQueryBuilder"/>
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { SetField(ref _order, value); }
        }

        /// <inheritdoc/>
        [Sql]
        public string SelectSQL
        {
            get { return _selectSql; }
            set
            {
                //never allow annoying whitespace on this field
                if (value != null)
                    value = value.Trim();

                SetField(ref _selectSql, value);
            }
        }

        /// <inheritdoc/>
        public string Alias
        {
            get { return _alias; }
            set { SetField(ref _alias , value);}
        }

        /// <inheritdoc/>
        public bool HashOnDataRelease
        {
            get { return _hashOnDataRelease; }
            set { SetField(ref _hashOnDataRelease , value);}
        }

        /// <inheritdoc/>
        public bool IsExtractionIdentifier
        {
            get { return _isExtractionIdentifier; }
            set { SetField(ref _isExtractionIdentifier , value); }
        }

        /// <inheritdoc/>
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { SetField(ref _isPrimaryKey , value);}
        }

        #endregion

        #region Relationships

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public abstract ColumnInfo ColumnInfo { get; }

        #endregion

        protected ConcreteColumn(IRepository repository, DbDataReader r):base(repository,r)
        {
            
        }

        protected ConcreteColumn()
        {
            
        }

        /// <inheritdoc/>
        public string GetRuntimeName()
        {
            var helper = ColumnInfo.GetQuerySyntaxHelper();
            if (!String.IsNullOrWhiteSpace(Alias))
                return helper.GetRuntimeName(Alias);//.GetRuntimeName(); RDMPQuerySyntaxHelper.GetRuntimeName(this);

            if (!String.IsNullOrWhiteSpace(SelectSQL))
                return helper.GetRuntimeName(SelectSQL);

            return ColumnInfo.GetRuntimeName();
        }

        /// <inheritdoc cref="ColumnSyntaxChecker"/>
        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }

        /// <summary>
        /// Compares columns by <see cref="ConcreteColumn.Order"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is IColumn)
                return this.Order - (obj as IColumn).Order;

            return 0;
        }
    }
}
