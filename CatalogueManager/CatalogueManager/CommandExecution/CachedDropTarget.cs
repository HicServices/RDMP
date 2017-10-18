using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    internal class CachedDropTarget
    {
        public object Target { get; private set; }
        public InsertOption RelativeLocation { get; private set; }

        public CachedDropTarget(object target, InsertOption relativeLocation)
        {
            Target = target;
            RelativeLocation = relativeLocation;
        }

        protected bool Equals(CachedDropTarget other)
        {
            return Equals(Target, other.Target) && RelativeLocation == other.RelativeLocation;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CachedDropTarget) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Target != null ? Target.GetHashCode() : 0)*397) ^ (int) RelativeLocation;
            }
        }
    }
}