using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace Sharing.Dependency.Gathering
{
    /// <summary>
    /// The described Object is only tenously related to the original object and you shouldn't worry too much if during refactoring you don't find any references. 
    /// An example of this would be all Filters in a Catalogue where a single ColumnInfo is being renamed.  Any filter in the catalogue could contain a reference to
    /// the ColumnInfo but most won't.
    ///
    /// <para>Describes an RDMP object that is related to another e.g. a ColumnInfo can have 0+ CatalogueItems associated with it.  This differs from IHasDependencies by the fact that
    /// it is a more constrained set rather than just spider webbing out everywhere.</para>
    /// </summary>
    public class GatheredObject : IHasDependencies, IMasqueradeAs
    {
        public IMapsDirectlyToDatabaseTable Object { get; set; }
        public string ForeignKeyPropertyIfAny { get; set; }

        public List<GatheredObject> Dependencies { get; private set; }

        public GatheredObject(IMapsDirectlyToDatabaseTable o,string foreignKeyPropertyIfAny = null)
        {
            Object = o;
            ForeignKeyPropertyIfAny = foreignKeyPropertyIfAny;
            Dependencies = new List<GatheredObject>();
        }

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

        public object MasqueradingAs()
        {
            return Object;
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
            return Dependencies.ToArray();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new IHasDependencies[0];
        }

        /// <summary>
        /// Returns all Dependencies recursively as IMapsDirectlyToDatabaseTable
        /// </summary>
        /// <returns></returns>
        public HashSet<IMapsDirectlyToDatabaseTable> Flatten()
        {
            return Flatten(new HashSet<IMapsDirectlyToDatabaseTable>());
        }

        private HashSet<IMapsDirectlyToDatabaseTable> Flatten(HashSet<IMapsDirectlyToDatabaseTable> set)
        {
            set.Add(Object);
            foreach (GatheredObject gatheredObject in Dependencies)
                foreach (var o in gatheredObject.Flatten())
                    set.Add(o);

            return set;
        }
    }
}
