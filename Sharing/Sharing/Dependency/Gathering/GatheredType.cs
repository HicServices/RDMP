using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace Sharing.Dependency.Gathering
{
    public class GatheredType :IHasDependencies,IMasqueradeAs
    {
        public Type Type { get; set; }
        public List<GatheredObject> Dependencies { get; private set; }

        public GatheredType(Type type)
        {
            Type = type;
            Dependencies = new List<GatheredObject>();
        }

        #region Equality
        protected bool Equals(GatheredType other)
        {
            return Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GatheredType) obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public object MasqueradingAs()
        {
            return Type;
        }

        public static bool operator ==(GatheredType left, GatheredType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GatheredType left, GatheredType right)
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

        public void Add(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
        {
            Dependencies.Add(new GatheredObject(mapsDirectlyToDatabaseTable));
        }
    }
}
