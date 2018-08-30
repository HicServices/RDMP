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

        /// <summary>
        /// True if the <see cref="ColumnInfo"/> should be wrapped with a standard hashing algorithmn (e.g. MD5) when extracted to researchers in a data extract.
        /// <para>Hashing algorithmn must be defined in data export database</para>
        /// </summary>
        public bool HashOnDataRelease
        {
            get { return _hashOnDataRelease; }
            set { SetField(ref _hashOnDataRelease , value);}
        }

        /// <summary>
        /// Indicates whether this column holds patient identifiers which can be used for cohort creation and which must be substituted for anonymous release
        /// identifiers on data extraction (to a researcher).
        /// </summary>
        public bool IsExtractionIdentifier
        {
            get { return _isExtractionIdentifier; }
            set { SetField(ref _isExtractionIdentifier , value); }
        }

        /// <summary>
        /// Indicates whether this column is the Primary Key (or part of a composite Primary Key) when extracted.  This flag is not copied / imputed from 
        /// <see cref="CatalogueLibrary.Data.ColumnInfo.IsPrimaryKey"/> because primary keys can often contain sensitive information (e.g. lab number) and
        /// you may have a transform or hash configured or your <see cref="Catalogue"/> may involve joining multiple <see cref="TableInfo"/> together.
        /// </summary>
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
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
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
