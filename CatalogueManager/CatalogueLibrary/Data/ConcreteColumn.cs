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
    public abstract class ConcreteColumn : VersionedDatabaseEntity, IColumn,IOrderable
    {
        #region Database Properties
 
        private string _selectSql;
        private string _alias;
        private bool _hashOnDataRelease;
        private bool _isExtractionIdentifier;
        private bool _isPrimaryKey;
        private int _order;

        public int Order
        {
            get { return _order; }
            set { SetField(ref _order, value); }
        }

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

        public string Alias
        {
            get { return _alias; }
            set { SetField(ref _alias , value);}
        }

        public bool HashOnDataRelease
        {
            get { return _hashOnDataRelease; }
            set { SetField(ref _hashOnDataRelease , value);}
        }

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

        [NoMappingToDatabase]
        public abstract ColumnInfo ColumnInfo { get; }

        #endregion

        protected ConcreteColumn(IRepository repository, DbDataReader r):base(repository,r)
        {
            
        }

        protected ConcreteColumn()
        {
            
        }

        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }


        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }
}
