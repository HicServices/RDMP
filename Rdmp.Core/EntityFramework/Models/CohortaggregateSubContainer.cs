using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("CohortAggregateSubContainer")]
    public class CohortaggregateSubContainer: DatabaseObject, IOrderable
    {
        public int CohortAggregateContainer_ParentID { get; set; }
        public int CohortAggregateContainer_ChildID { get; set; }

        [ForeignKey("CohortAggregateContainer_ChildID")]
        public virtual CohortAggregateContainer Child { get; set; }

        [ForeignKey("CohortAggregateContainer_ParentID")]
        public virtual CohortAggregateContainer Parent { get; set; }
        public int Order { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
