using System.Collections.Generic;
using CatalogueLibrary.Data;

namespace DataExportLibrary.Providers.Nodes.UsedByNodes
{
    public class ObjectUsedByOtherObjectNode<T, T2> : IObjectUsedByOtherObjectNode<T,T2> 
        where T:class 
        where T2:class 
    {
        public T User { get; set; }
        public T2 ObjectBeingUsed { get; private set; }

        public bool IsEmptyNode
        {
            get { return ObjectBeingUsed == null; }
        }

        public ObjectUsedByOtherObjectNode(T user, T2 objectBeingUsed)
        {
            User = user;
            ObjectBeingUsed = objectBeingUsed;
        }

        public object MasqueradingAs()
        {
            return ObjectBeingUsed;
        }
        public override string ToString()
        {
            if (IsEmptyNode)
                return "???";

            return ObjectBeingUsed.ToString();
        }

        #region Equality
        protected bool Equals(ObjectUsedByOtherObjectNode<T, T2> other)
        {
            return EqualityComparer<T>.Default.Equals(User, other.User) && EqualityComparer<T2>.Default.Equals(ObjectBeingUsed, other.ObjectBeingUsed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectUsedByOtherObjectNode<T, T2>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(User)*397) ^ EqualityComparer<T2>.Default.GetHashCode(ObjectBeingUsed);
            }
        }
        #endregion
    }
}
