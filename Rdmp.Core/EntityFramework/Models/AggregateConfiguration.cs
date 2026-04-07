using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.QueryBuilding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design.Serialization;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models
{
    public class AggregateConfiguration : DatabaseObject, ICollectSqlParameters
    {
        [Key]
        public override int ID { get; set; }

        public string Name { get; set; }

        public int Catalogue_ID { get; set; }
        public int? RootFilterContainer_ID { get; set; }
        public string CountSQL { get; set; }
        public string Description { get; set; }

        public int? PivotOnDimensionID { get; set; }
        public bool IsExtractable { get; set; }

        public int OverrideFiltersByUsingParentAggregateConfigurationInstead_ID { get; set; }
        public string HavingSQL { get; set; }
        public bool IsDisabled { get; set; }
        public int Order { get; set; } //todo this doesn't exist

        [ForeignKey("RootFilterContainer_ID")]
        public virtual IContainer RootFilterContainer { get; set; }//TODO

        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }

        public CohortIdentificationConfiguration GetCohortIdentificationConfigurationIfAny()
        {
            return null;
        }

        public virtual List<AggregateDimension> AggregateDimensions { get; set; }//TODO

        public bool IsCohortIdentificationAggregate => false;//todo

        public AggregateBuilder GetQueryBuilder()
        {
            //TODO
            return new AggregateBuilder("", CountSQL, this);
        }

        public AggregateTopX GetTopXIfAny() => null;//todo

        public ISqlParameter[] GetAllParameters()
        {
            throw new NotImplementedException();
        }
        public bool IsJoinablePatientIndexTable() => false; //todo

        public bool IsEnabled() => !IsDisabled;

        public bool IsAcceptableAsCohortGenerationSource(out string reason) {
            reason = "TODO";
            return true;
        }
        public virtual ITableInfo[] ForcedJoins => null;// CatalogueDbContext.AggregateForcedJoinManager.GetAllForcedJoinsFor(this);

        public CohortAggregateContainer GetCohortAggregateContainerIfAny() => null;//TODO

    }
}
