using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Refactoring.Gathering
{
    /// <summary>
    /// The described Object is only tenously related to the original object and you shouldn't worry too much if during refactoring you don't find any references. 
    /// An example of this would be all Filters in a Catalogue where a single ColumnInfo is being renamed.  Any filter in the catalogue could contain a reference to
    /// the ColumnInfo but most won't.
    /// </summary>
    public class GatheredObject
    {
        public GatheredObject(IMapsDirectlyToDatabaseTable o)
        {
            Object = o;
        }

        public IMapsDirectlyToDatabaseTable Object { get; set; }

        public bool TenousRelationship { get; set; }


        #region Equality
        protected bool Equals(GatheredObject other)
        {
            return Equals(Object, other.Object);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GatheredObject) obj);
        }

        public override int GetHashCode()
        {
            return (Object != null ? Object.GetHashCode() : 0);
        }

        public static bool operator ==(GatheredObject left, GatheredObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GatheredObject left, GatheredObject right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
