// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
///     Interface for plugins that want to perform custom tasks when part of a cohort builder query is run
///     e.g. call out to an external API and store the resulting identifier list in the query cache
/// </summary>
public interface IPluginCohortCompiler
{
    /// <summary>
    ///     Return true if the <paramref name="ac" /> is of a type that should be handled by your class.
    ///     All aggregates will regularly be passed to this when run so ensure that your response is fast
    /// </summary>
    /// <param name="ac"></param>
    /// <returns></returns>
    bool ShouldRun(AggregateConfiguration ac);

    /// <summary>
    ///     Return true if the <paramref name="catalogue" /> is of a type that should be handled by your class
    ///     by calling your API.
    /// </summary>
    /// <param name="catalogue"></param>
    /// <returns></returns>
    bool ShouldRun(ICatalogue catalogue);


    /// <summary>
    ///     Must be implemented such that by the time the method completes the <paramref name="cache" />
    ///     is populated with an identifier list that matches the expectations of <paramref name="ac" />
    /// </summary>
    /// <param name="ac"></param>
    /// <param name="cache"></param>
    /// <param name="cancellationToken"></param>
    void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Return true if the <paramref name="oldDescription" /> does not match the logic currently
    ///     stored in <paramref name="aggregate" />
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="oldDescription"></param>
    /// <returns></returns>
    bool IsStale(AggregateConfiguration aggregate, string oldDescription);

    /// <summary>
    ///     When the API is used as described in <paramref name="joinedTo" /> as a patient index table
    ///     and its results are cached, which column should be pulled from the results for joining to
    ///     other datasets
    /// </summary>
    /// <param name="joinedTo"></param>
    /// <returns></returns>
    IHasRuntimeName GetJoinColumnForPatientIndexTable(AggregateConfiguration joinedTo);
}