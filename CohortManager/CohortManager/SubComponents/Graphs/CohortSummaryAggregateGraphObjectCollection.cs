using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Dashboarding;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CohortManager.SubComponents.Graphs
{
    public class CohortSummaryAggregateGraphObjectCollection:IPersistableObjectCollection
    {
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        public CohortSummaryAdjustment Adjustment;
        public AggregateFilter SingleFilterOnly { get { return DatabaseObjects.OfType<AggregateFilter>().SingleOrDefault(); } }

        public CohortAggregateContainer CohortContainerIfAny { get { return DatabaseObjects[0] as CohortAggregateContainer; } }
        public AggregateConfiguration CohortIfAny { get { return DatabaseObjects[0] as AggregateConfiguration; } }

        public AggregateConfiguration Graph { get { return (AggregateConfiguration)DatabaseObjects[1]; } }

        /// <summary>
        /// Do not use this constructor, it is used only for deserialization during persistence on form loading after application closing
        /// </summary>
        public CohortSummaryAggregateGraphObjectCollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        /// <summary>
        /// Use this constructor at runtime
        /// </summary>
        /// <param name="cohort"></param>
        /// <param name="graph"></param>
        /// <param name="adjustment"></param>
        public CohortSummaryAggregateGraphObjectCollection(AggregateConfiguration cohort, AggregateConfiguration graph,CohortSummaryAdjustment adjustment):this()
        {
            if(!cohort.IsCohortIdentificationAggregate)
                throw new ArgumentException("Parameter cohort was AggregateConfiguration '" + cohort + "' which is not a Cohort Aggregate (not allowed)","cohort");
            if (graph.IsCohortIdentificationAggregate)
                throw new ArgumentException("Parameter graph was AggregateConfiguration '" + graph + "' which is a Cohort Aggregate (not allowed)", "graph");

            DatabaseObjects.Add(cohort);
            DatabaseObjects.Add(graph);
            Adjustment = adjustment;
        }
        /// <summary>
        /// Overload that does the operation on a container with (WhereExtractionIdentifiersIn - the only permissable option)
        /// </summary>
        /// <param name="cohort"></param>
        /// <param name="graph"></param>
        public CohortSummaryAggregateGraphObjectCollection(CohortAggregateContainer container, AggregateConfiguration graph)
            : this()
        {
            if (graph.IsCohortIdentificationAggregate)
                throw new ArgumentException("Parameter graph was AggregateConfiguration '" + graph + "' which is a Cohort Aggregate (not allowed)", "graph");

            DatabaseObjects.Add(container);
            DatabaseObjects.Add(graph);
            Adjustment = CohortSummaryAdjustment.WhereExtractionIdentifiersIn;
        }

        public CohortSummaryAggregateGraphObjectCollection(AggregateConfiguration cohort, AggregateConfiguration graph, CohortSummaryAdjustment adjustment, AggregateFilter singleFilterOnly):this(cohort,graph,adjustment)
        {
            DatabaseObjects.Add(singleFilterOnly);
        }

        public string SaveExtraText()
        {
            return Adjustment.ToString();
        }

        public void LoadExtraText(string s)
        {
            CohortSummaryAdjustment a;
            
            if(!CohortSummaryAdjustment.TryParse(s, out a))
                throw new Exception("Could not parse '" + s + "' into a valid CohortSummaryAdjustment");

            Adjustment = a;
        }

        public void RevertIfMatchedInCollectionObjects(DatabaseEntity oTriggeringRefresh)
        {
            var matchingObject = DatabaseObjects.SingleOrDefault(o => o.Equals(oTriggeringRefresh));

            if(matchingObject != null)
                ((IRevertable)matchingObject).RevertToDatabaseState();
        }
    }
}