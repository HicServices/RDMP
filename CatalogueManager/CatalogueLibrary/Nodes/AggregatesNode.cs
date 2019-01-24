using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all <see cref="AggregateConfiguration"/> graphs for visualising data in the <see cref="Catalogue"/>.
    /// </summary>
    public class AggregatesNode
    {

        /// <summary>
        /// The <see cref="Catalogue"/> to which all the <see cref="AggregateConfiguration"/> belong
        /// </summary>
        public Catalogue Catalogue { get; set; }

        public AggregatesNode(Catalogue c, AggregateConfiguration[] regularAggregates)
        {
            Catalogue = c;
        }

        public override string ToString()
        {
            return "Aggregate Graphs";
        }

        protected bool Equals(AggregatesNode other)
        {
            return Catalogue.Equals(other.Catalogue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AggregatesNode) obj);
        }

        public override int GetHashCode()
        {
            return Catalogue.GetHashCode() * this.GetType().GetHashCode();
        }
    }
}
