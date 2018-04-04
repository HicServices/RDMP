using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.AggregationUIs;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManagerLibrary.QueryBuilding;

namespace CohortManager.SubComponents.Graphs
{
    /// <summary>
    /// Allows you to execute a Frankenstein AggregateGraph that combines the dimensions, pivots, joins etc of a regular Aggregate Chart with the results of a 'Cohort Set'.  This allows you
    /// to compare live vs cohort and easily visualise cohorts you are building in a CohortIdentificationConfiguration.  For example if you have an 'Aggregate Chart' which shows demography
    /// records for 5 healthboards over time then you combine it with a 'cohort set' "People who have lived in Tayside or Fife for at least 5 years" you should expect to see the graph only
    /// show Tayside and Fife records.
    /// 
    /// <para>There are 2 ways of combining the two queries (cohort and original graph):
    /// WhereExtractionIdentifiersIn - provides an identical query to the original graph with an extra restriction that the patient identifier must appear in the 'Cohort Set'.  This lets you
    /// have a 'Cohort Set' "Prescriptions for Morphine" but generate a graph of "All drug prescriptions over time" and have it show all the drugs that those patients are on over time (this
    /// should show a high favouritism for Morphine but also show other drugs Morphine users also take).</para>
    /// 
    /// <para>WhereRecordsIn - provides an identical query to the original graph but also applies the Filters that are on the 'Cohort Set'.  This means that the same 'Cohort Set' "Prescriptions for
    /// Morphine" combined with the 'Aggregate Chart' "All drug prescriptions over time" would show ONLY Morphine prescriptions (since that is what the records that are returned by the cohort
    /// query).</para>
    ///  
    /// </summary>
    public class CohortSummaryAggregateGraph:AggregateGraph, IObjectCollectionControl
    {
        private CohortSummaryAggregateGraphObjectCollection _collection;

        public CohortSummaryAggregateGraph()
        {
            AssociatedCollection = RDMPCollection.Cohort;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            bool shouldCloseInstead;
            _collection.RevertIfMatchedInCollectionObjects(e.Object,out shouldCloseInstead);

            if (shouldCloseInstead)
            {
                if(ParentForm != null)
                    ParentForm.Close();
            }
            else
                //now reload the graph because the change was to a relevant object
                LoadGraphAsync();
            
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _collection = (CohortSummaryAggregateGraphObjectCollection) collection;
            _activator = activator;
            base.SetAggregate(_collection.Graph);
            LoadGraphAsync();
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public override string GetTabName()
        {
            if(_collection.CohortIfAny != null)
                return "Cohort Graph " + _collection.CohortIfAny + "(" + _collection.Adjustment + ")";

            if(_collection.CohortContainerIfAny != null)
                return "Cohort Container Graph " + _collection.CohortContainerIfAny;

            return "Loading...";
        }

        protected override string GetDescription()
        {
            string orig = base.GetDescription();

            string restriction;
            switch (_collection.Adjustment)
            {
                case CohortSummaryAdjustment.WhereExtractionIdentifiersIn:
                    restriction = "Only showing records for people in cohort set '" + (_collection.CohortIfAny ?? (object)_collection.CohortContainerIfAny)+ "')";
                    break;
                case CohortSummaryAdjustment.WhereRecordsIn:
                    restriction = "Only showing records returned by the query defining cohort set '" + (_collection.CohortIfAny ?? (object)_collection.CohortContainerIfAny) + "')";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_collection.SingleFilterOnly != null)
                restriction += ". Only showing Filter " + _collection.SingleFilterOnly + ".";

            var toReturn = orig + " (RESULTS RESTRICTED:" + restriction;
            return toReturn.Trim();
        }

        protected override object[] GetRibbonObjects()
        {
            List<object> toReturn = new List<object>();
            
            
            if(_collection.SingleFilterOnly!= null)
                toReturn.Add(_collection.SingleFilterOnly);

            toReturn.Add((object) _collection.CohortIfAny ?? _collection.CohortContainerIfAny);

            toReturn.Add(AggregateConfiguration);
            toReturn.Add(GetAdjustmentDescription(_collection.Adjustment));

            return toReturn.ToArray();
        }

        private string GetAdjustmentDescription(CohortSummaryAdjustment adjustment)
        {
            switch (adjustment)
            {
                case CohortSummaryAdjustment.WhereExtractionIdentifiersIn:
                    return "Graphing All Records For Patients";
                case CohortSummaryAdjustment.WhereRecordsIn:
                    return "Graphing Cohort Query Result";
                default:
                    throw new ArgumentOutOfRangeException("adjustment");
            }
        }

        protected override AggregateBuilder GetQueryBuilder(AggregateConfiguration summary)
        {
            CohortSummaryQueryBuilder builder;

            if (_collection.CohortIfAny != null)
                builder = new CohortSummaryQueryBuilder(summary, _collection.CohortIfAny);
            else
                builder = new CohortSummaryQueryBuilder(summary, _collection.CohortContainerIfAny);

            return builder.GetAdjustedAggregateBuilder(_collection.Adjustment,_collection.SingleFilterOnly);
        }

    }
}
