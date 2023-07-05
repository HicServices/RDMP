// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.AggregationUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SubComponents.Graphs;

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
public class CohortSummaryAggregateGraphUI:AggregateGraphUI, IObjectCollectionControl
{
    private CohortSummaryAggregateGraphObjectCollection _collection;

    public CohortSummaryAggregateGraphUI()
    {
        AssociatedCollection = RDMPCollection.Cohort;
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        _collection.RevertIfMatchedInCollectionObjects(e.Object,out var shouldCloseInstead);

        if (shouldCloseInstead)
        {
            ParentForm?.Close();
        }
        else
            //now reload the graph because the change was to a relevant object
            LoadGraphAsync();
            
    }

    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        _collection = (CohortSummaryAggregateGraphObjectCollection) collection;
        SetItemActivator(activator);
            
        BuildMenu(activator);

        SetAggregate(activator,_collection.Graph);
        LoadGraphAsync();
    }

    public IPersistableObjectCollection GetCollection()
    {
        return _collection;
    }

    public override string GetTabName()
    {
        if(_collection.CohortIfAny != null)
            return $"Cohort Graph {_collection.CohortIfAny}({_collection.Adjustment})";

        if(_collection.CohortContainerIfAny != null)
            return $"Cohort Container Graph {_collection.CohortContainerIfAny}";

        return "Loading...";
    }

    protected override string GetDescription()
    {
        var orig = base.GetDescription();

        string restriction;
        switch (_collection.Adjustment)
        {
            case CohortSummaryAdjustment.WhereExtractionIdentifiersIn:
                restriction =
                    $"Only showing records for people in cohort set '{_collection.CohortIfAny ?? (object)_collection.CohortContainerIfAny}')";
                break;
            case CohortSummaryAdjustment.WhereRecordsIn:
                restriction =
                    $"Only showing records returned by the query defining cohort set '{_collection.CohortIfAny ?? (object)_collection.CohortContainerIfAny}')";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_collection.SingleFilterOnly != null)
            restriction += $". Only showing Filter {_collection.SingleFilterOnly}.";

        var toReturn = $"{orig} (RESULTS RESTRICTED:{restriction}";
        return toReturn.Trim();
    }

    protected override object[] GetRibbonObjects()
    {
        return new object[]{ GetAdjustmentDescription(_collection.Adjustment)};
    }

    private static string GetAdjustmentDescription(CohortSummaryAdjustment adjustment)
    {
        switch (adjustment)
        {
            case CohortSummaryAdjustment.WhereExtractionIdentifiersIn:
                return "Graphing All Records For Patients";
            case CohortSummaryAdjustment.WhereRecordsIn:
                return "Graphing Cohort Query Result";
            default:
                throw new ArgumentOutOfRangeException(nameof(adjustment));
        }
    }

    protected override AggregateBuilder GetQueryBuilder(AggregateConfiguration summary)
    {
        CohortSummaryQueryBuilder builder;

        if (_collection.CohortIfAny != null)
            builder = new CohortSummaryQueryBuilder(summary, _collection.CohortIfAny,Activator.CoreChildProvider);
        else
            builder = new CohortSummaryQueryBuilder(summary, _collection.CohortContainerIfAny);

        return builder.GetAdjustedAggregateBuilder(_collection.Adjustment,_collection.SingleFilterOnly);
    }

}