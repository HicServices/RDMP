using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("CohortaggregateContainer_AggregateConfiguration")]
    public class CohortAggregateContainerAggregateConfiguration : DatabaseObject
    {
        [NotMapped]
        public override int ID { get; set; }

        public int CohortAggregateContainer_ID { get; set; }
        public int AggregateConfiguration_ID { get; set; }

        [ForeignKey("AggregateConfiguration_ID")]
        public virtual AggregateConfiguration Child { get; set; }

        [ForeignKey("CohortAggregateContainer_ID")]
        public virtual CohortAggregateContainer Parent { get; set; }
    }
}
