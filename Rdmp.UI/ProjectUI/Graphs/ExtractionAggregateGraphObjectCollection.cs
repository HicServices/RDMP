// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.UI.ProjectUI.Graphs;

/// <summary>
///     Persistence/instantiation collection for <see cref="ExtractionAggregateGraphUI" />.  Records which
///     <see cref="Graph" /> is being
///     visualized (e.g. healthboards over time) with which extractable dataset in which extraction (
///     <see cref="SelectedDataSets" />)
/// </summary>
public class ExtractionAggregateGraphObjectCollection : PersistableObjectCollection
{
    /// <summary>
    ///     Constructor used for persistence
    /// </summary>
    public ExtractionAggregateGraphObjectCollection()
    {
    }

    /// <summary>
    ///     Use this constructor at runtime
    /// </summary>
    /// <param name="selectedDataSet"></param>
    /// <param name="graph"></param>
    public ExtractionAggregateGraphObjectCollection(SelectedDataSets selectedDataSet, AggregateConfiguration graph) :
        this()
    {
        DatabaseObjects.Add(selectedDataSet);
        DatabaseObjects.Add(graph);
    }

    /// <summary>
    ///     The extraction dataset (in a given <see cref="ExtractionConfiguration" />) to which the <see cref="Graph" />
    ///     results
    ///     should be limited.  The graph should only depict records appearing in this extract.
    /// </summary>
    public SelectedDataSets SelectedDataSets => (SelectedDataSets)DatabaseObjects[0];

    /// <summary>
    ///     The graph to be shown
    /// </summary>
    public AggregateConfiguration Graph => (AggregateConfiguration)DatabaseObjects[1];

    /// <summary>
    ///     Returns true if the collection is not in a fit state to generate the graph.  Note that
    ///     the graph may still fail later in the query generation/execution phase.  This is just
    ///     fast checks that can be quickly performed e.g. is there a cohort on the ExtractionConfiguration
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public bool IsImpossible(out string reason)
    {
        if (SelectedDataSets.ExtractionConfiguration.Cohort_ID == null)
        {
            reason = "ExtractionConfiguration does not have a cohort";
            return true;
        }

        reason = null;
        return false;
    }
}