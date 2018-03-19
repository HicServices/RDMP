using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace Sharing.Dependency.Gathering
{
    /// <summary>
    /// The described Object is only tenously related to the original object and you shouldn't worry too much if during refactoring you don't find any references. 
    /// An example of this would be all Filters in a Catalogue where a single ColumnInfo is being renamed.  Any filter in the catalogue could contain a reference to
    /// the ColumnInfo but most won't.
    ///
    /// Describes an RDMP object that is related to another e.g. a ColumnInfo can have 0+ CatalogueItems associated with it.  This differs from IHasDependencies by the fact that
    /// it is a more constrained set rather than just spider webbing out everywhere.
    /// </summary>
    public class GatheredObject:IHasDependencies
    {
        public List<GatheredObject> Dependencies { get; private set; }

        public GatheredObject(IMapsDirectlyToDatabaseTable o)
        {
            Object = o;
            Dependencies = new List<GatheredObject>();
        }

        public IMapsDirectlyToDatabaseTable Object { get; set; }

        public bool TenousRelationship { get; set; }

        /// <summary>
        /// True if the gathered object is a data export object (e.g. it is an ExtractableColumn or DeployedExtractionFilter) and it is part of a frozen (released)
        /// ExtractionConfiguration 
        /// </summary>
        public bool IsReleased { get; set; }

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

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return Dependencies.ToArray();
        }
    }
}
