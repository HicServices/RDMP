// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
///     Manages information about what set containers / subcontainers exist under a
///     <see cref="CohortIdentificationConfiguration" />
/// </summary>
public interface ICohortContainerManager
{
    /// <summary>
    ///     If the AggregateConfiguration is set up as a cohort identification set in a
    ///     <see cref="CohortIdentificationConfiguration" /> then this method will return the set container
    ///     (e.g. UNION / INTERSECT / EXCEPT) that it is in.  Returns null if it is not in a
    ///     <see cref="CohortAggregateContainer" />.
    /// </summary>
    /// <returns></returns>
    CohortAggregateContainer GetParent(AggregateConfiguration child);

    /// <summary>
    ///     Gets the parent container of the current container (if it is not a root / orphan container)
    /// </summary>
    /// <returns></returns>
    CohortAggregateContainer GetParent(CohortAggregateContainer child);

    /// <summary>
    ///     Makes the configuration a member of the given container with the given <paramref name="order" /> relative to other
    ///     things (if any) in the container.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <param name="order"></param>
    void Add(CohortAggregateContainer parent, AggregateConfiguration child, int order);

    /// <summary>
    ///     Removes the <paramref name="child" /> configuration from the given container (to which it must belong already)
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    void Remove(CohortAggregateContainer parent, AggregateConfiguration child);

    /// <summary>
    ///     If the configuration is part of any aggregate container anywhere this method will return the order within that
    ///     container
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    int? GetOrderIfExistsFor(AggregateConfiguration configuration);

    /// <summary>
    ///     Gets all the subcontainers of the current container (if any)
    /// </summary>
    /// <returns></returns>
    IOrderable[] GetChildren(CohortAggregateContainer parent);

    /// <summary>
    ///     Removes the given <paramref name="child" /> container from its host parent container
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    void Remove(CohortAggregateContainer parent, CohortAggregateContainer child);

    /// <summary>
    ///     Reorders the <paramref name="child" /> to appear in the new location within its parent container (relative to other
    ///     things in the container).
    /// </summary>
    /// <param name="child"></param>
    /// <param name="newOrder"></param>
    void SetOrder(AggregateConfiguration child, int newOrder);

    /// <summary>
    ///     Adds the given <paramref name="child" /> to the <paramref name="parent" /> container.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    void Add(CohortAggregateContainer parent, CohortAggregateContainer child);
}