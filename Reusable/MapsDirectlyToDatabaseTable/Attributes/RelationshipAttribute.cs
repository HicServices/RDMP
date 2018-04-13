using System;
using System.Runtime.CompilerServices;

namespace MapsDirectlyToDatabaseTable.Attributes
{
    /// <summary>
    /// Used to indicate when an ID column contains the ID of another RDMP object.  Decorate the foreign key object. This can be involve going 
    /// between databases or even servers e.g. between DataExport and Catalogue libraries or between Catalogue and plugin databases
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class RelationshipAttribute : Attribute
    {
        /// <summary>
        /// The other class whose ID is stored in decorated property
        /// </summary>
        public Type Cref { get; set; }

        /// <summary>
        /// The decorated property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Declares that the decorated property contains the ID of the specified Type of object
        /// </summary>
        /// <param name="cref"></param>
        /// <param name="propertyName"></param>
        public RelationshipAttribute(Type cref,[CallerMemberName] string propertyName=null)
        {
            Cref = cref;
            PropertyName = propertyName;
        }

        #region Equality Members
        protected bool Equals(RelationshipAttribute other)
        {
            return base.Equals(other) && Equals(Cref, other.Cref) && string.Equals(PropertyName, other.PropertyName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RelationshipAttribute) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (Cref != null ? Cref.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (PropertyName != null ? PropertyName.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}