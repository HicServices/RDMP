using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;

namespace CatalogueLibrary.Nodes
{
    public class CohortSetsNode
    {
        public Catalogue Catalogue { get; set; }

        public CohortSetsNode(Catalogue catalogue, AggregateConfiguration[] cohortAggregates)
        {
            Catalogue = catalogue;
        }

        public override string ToString()
        {
            return "Cohort Sets";
        }

        protected bool Equals(CohortSetsNode other)
        {
            return Catalogue.Equals(other.Catalogue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CohortSetsNode) obj);
        }

        public override int GetHashCode()
        {
            return Catalogue.GetHashCode() * this.GetType().GetHashCode();
        }
    }
}
