using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all <see cref="AggregateConfiguration"/> in a <see cref="Catalogue"/> that are involved in cohort creation (See <see cref="CohortIdentificationConfiguration"/>).
    /// </summary>
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
