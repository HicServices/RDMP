// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.QueryCaching.Aggregation;

/// <summary>
///     Describes the role of an <see cref="Rdmp.Core.Curation.Data.Aggregation.AggregateConfiguration" /> being cached by
///     a
///     <see cref="Rdmp.Core.QueryCaching.Aggregation.CachedAggregateConfigurationResultsManager" />.  This indicates what
///     values
///     are cached and its role in the RDMP (e.g. to build cohorts, to visualize data etc).
/// </summary>
public enum AggregateOperation
{
    /// <summary>
    ///     The <see cref="Rdmp.Core.Curation.Data.Aggregation.AggregateConfiguration" /> is a set of identifiers used for
    ///     building a cohort
    ///     (single column containing identifiers).  This list can be intersected / unioned etc with other similar lists to
    ///     build a final cohort
    /// </summary>
    IndexedExtractionIdentifierList,

    /// <summary>
    ///     The <see cref="Rdmp.Core.Curation.Data.Aggregation.AggregateConfiguration" /> is a graph / summary table useful for
    ///     visualizing the
    ///     data referenced by a <see cref="Rdmp.Core.Curation.Data.Catalogue" />.  The cached table may be displayed on a web
    ///     page or used for
    ///     high speed viewing of the last known state of the data.  The data should not contain identifiable row level data.
    /// </summary>
    ExtractableAggregateResults,

    /// <summary>
    ///     The <see cref="Rdmp.Core.Curation.Data.Aggregation.AggregateConfiguration" /> is a patient index table that
    ///     contains a list of
    ///     patient identifiers along with relevant other columns e.g. drug prescription dates.  This table can be used to join
    ///     against in
    ///     the <see cref="Rdmp.Core.CohortCreation.Execution.CohortCompiler" /> (see
    ///     <see cref="Rdmp.Core.Curation.Data.Cohort.Joinables.JoinableCohortAggregateConfiguration" />)
    /// </summary>
    JoinableInceptionQuery
}