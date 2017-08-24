namespace CatalogueLibrary.Nodes
{
    public abstract class SingletonNode
    {
        protected readonly string Caption;

        protected SingletonNode(string caption)
        {
            Caption = caption;
        }

        public override string ToString()
        {
            return Caption;
        }

        protected bool Equals(SingletonNode other)
        {
            return string.Equals(Caption, other.Caption);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as SingletonNode;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Caption.GetHashCode();
        }
    }
}
