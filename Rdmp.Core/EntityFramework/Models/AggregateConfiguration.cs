using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("AggregateConfiguration")]
    public class AggregateConfiguration : DatabaseObject, ICollectSqlParameters, IOrderable, IHasQuerySyntaxHelper,ICheckable, IHasDependencies, INamed
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

        public int? OverrideFiltersByUsingParentAggregateConfigurationInstead_ID { get; set; }
        public string HavingSQL { get; set; }
        public bool IsDisabled { get; set; }
        [NotMapped]
        public int Order { get; set; } //todo this doesn't exist

        //[ForeignKey("RootFilterContainer_ID")]
        [NotMapped]
        public virtual IContainer RootFilterContainer { get; set; }//TODO

        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }

        public CohortIdentificationConfiguration GetCohortIdentificationConfigurationIfAny()
        {
            return null;
        }

        public virtual IEnumerable<AggregateDimension> AggregateDimensions { get; set; }//TODO

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
        [NotMapped]
        public JoinableCohortAggregateConfiguration JoinableCohortAggregateConfiguration { get; internal set; }


        [NotMapped]
        public JoinableCohortAggregateConfigurationUse[] PatientIndexJoinablesUsed { get; internal set; }
        [NotMapped]
        public AggregateDimension PivotDimension { get; set; }

        public CohortAggregateContainer GetCohortAggregateContainerIfAny() => null;//TODO

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new NotImplementedException();
        }

        internal AggregateConfiguration ShallowClone()
        {
            throw new NotImplementedException();
        }

        internal void ReFetchOrder()
        {
            throw new NotImplementedException();
        }

        public AggregateContinuousDateAxis GetAxisIfAny()
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public void AdjustGraphDataTable(DataTable dt)
        {
            throw new NotImplementedException();
        }

        internal Catalogue GetCatalogue()
        {
            throw new NotImplementedException();
        }

        internal AggregateDimension AddDimension(ExtractionInformation chosen)
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }
    }
}
