using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    public abstract class PersistableObjectCollection : IPersistableObjectCollection
    {
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

        public PersistableObjectCollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        public virtual string SaveExtraText()
        {
            return "";
        }

        public virtual void LoadExtraText(string s)
        {
            
        }

        protected bool Equals(PersistableObjectCollection other)
        {
            return DatabaseObjects.SequenceEqual(other.DatabaseObjects) && Equals(SaveExtraText(), other.SaveExtraText());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersistableObjectCollection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return
                    (397 * (this.DatabaseObjects != null ?
                        this.DatabaseObjects.Aggregate(0, (old, curr) =>
                            (old * 397) ^ (curr != null ? curr.GetHashCode() : 0)) :
                        0)
                        ) ^
                    (this.SaveExtraText() != null ? this.SaveExtraText().GetHashCode() : 0);
            } 
        }
    }
}