using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents.Annotations;

namespace CatalogueLibrary.Data
{
    public abstract class ConcreteColumn : VersionedDatabaseEntity, IColumn
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

        /// <summary>
        /// Do not use this in SELECT commands, instead use GetExtractionSelectSQL as this method will deal with cases where SelectSQL is null
        /// </summary>
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


        
    }
}