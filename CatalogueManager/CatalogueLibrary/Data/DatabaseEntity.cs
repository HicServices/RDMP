using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableUIComponents.Annotations;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Base class for all objects which are stored in database repositories (e.g. Catalogue database , Data Export database).  This is the abstract implementation of
    /// IMapsDirectlyToDatabaseTable.  You must always have two constructors, one that takes a DbDataReader and is responsible for constructing an instance from a 
    /// record in the database and one that takes the minimum parameters required to satisfy database constraints and is used to create new objects.
    /// 
    /// A DatabaseEntity instance cannot exist without there being a matching record in the database repository.  This is the RDMP design pattern for object permenance, 
    /// sharing and allowing advanced users to update the data model via database queries running directly on the object repository database.
    /// 
    /// A DatabaseEntity must have the same name as a Table in in the IRepository and must only have public properties that match columns in that table.  This enforces
    /// a transparent mapping between code and database.  If you need to add other public properties you must decorate them with [NoMappingToDatabase]
    /// 
    /// </summary>
    public abstract class DatabaseEntity : IRevertable, IDeleteable, INotifyPropertyChanged
    {
        // Does this need DoNotExtract set?
        // It was present for the following, but no others: Catalogue, CatalogueItem, CatalogueItemIssue
        public int ID { get; set; }

        protected bool MaxLengthSet = false;
        private bool _readonly;

        [NoMappingToDatabase]
        public IRepository Repository { get; set; }

        protected DatabaseEntity()
        {
        }

        protected DatabaseEntity(IRepository repository, DbDataReader r)
        {
            Repository = repository;

            if (!HasColumn(r, "ID"))
                throw new InvalidOperationException("The DataReader passed to this type (" + GetType().Name + ") does not contain an 'ID' column. This is a requirement for all IMapsDirectlyToDatabaseTable implementing classes.");

            ID = int.Parse(r["ID"].ToString()); // gets around decimals and other random crud number field types that sql returns

            if (!MaxLengthSet)
            {
                Repository.FigureOutMaxLengths(this);
                MaxLengthSet = true;
            }
        }

        private bool HasColumn(IDataRecord reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public Uri ParseUrl(DbDataReader r, string fieldName)
        {
            object uri = r[fieldName];

            if (uri == null || uri == DBNull.Value)
                return null;

            return new Uri(uri.ToString());
        }

        public override int GetHashCode()
        {
            return Repository.GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            return Repository.AreEqual(this, obj);
        }

        public virtual void SaveToDatabase()
        {
            Repository.SaveToDatabase(this);
        }


        public virtual void DeleteInDatabase()
        {
            Repository.DeleteFromDatabase(this);
        }

        public virtual void RevertToDatabaseState()
        {
            Repository.RevertToDatabaseState(this);
        }

        public RevertableObjectReport HasLocalChanges()
        {
            return Repository.HasLocalChanges(this);
        }

        public bool Exists()
        {
            return Repository.StillExists(this);
        }

        protected DateTime? ParseDateFromReader(DbDataReader r, string columnName)
        {
            if (r[columnName] is DBNull) return null;
            return DateTime.Parse(r[columnName].ToString());
        }

        protected int? ParseIntFromReader(DbDataReader r, string columnName)
        {
            if (r[columnName] is DBNull) return null;
            return Convert.ToInt32(r[columnName].ToString());
        }
        public DateTime? ObjectToNullableDateTime(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return (DateTime)o;
        }

        public static int? ObjectToNullableInt(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return int.Parse(o.ToString());
        }
        public static bool? ObjectToNullableBool(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return Convert.ToBoolean(o);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            if (_readonly)
                throw new Exception("An attempt was made to modify Property '" + propertyName + "' of Database Object of Type '" + GetType().Name + "' while it was in read only mode.  Object was called '" + this + "'");

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void SetReadOnly()
        {
            _readonly = true;
        }
    }
}