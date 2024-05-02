// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;

namespace Rdmp.Core.Repositories;

/// <summary>
///     A place to store DQE results (typically a database).  See <see cref="DQERepository" />
/// </summary>
public interface IDQERepository
{
    /// <summary>
    ///     The Catalogue database to which the IDs in <see cref="Evaluation" /> refer to.  Each DQE repo can serve
    ///     only a single RDMP Catalogue database
    /// </summary>
    ICatalogueRepository CatalogueRepository { get; }

    /// <summary>
    ///     Returns the most recently run DQE results for <paramref name="c" /> or null
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    Evaluation GetMostRecentEvaluationFor(ICatalogue c);

    /// <summary>
    ///     Returns all DQE results ever run on <paramref name="catalogue" />
    /// </summary>
    /// <param name="catalogue"></param>
    /// <returns></returns>
    IEnumerable<Evaluation> GetAllEvaluationsFor(ICatalogue catalogue);

    /// <summary>
    ///     Returns true if there are DQE results available for <paramref name="catalogue" />
    /// </summary>
    /// <param name="catalogue"></param>
    /// <returns></returns>
    bool HasEvaluations(ICatalogue catalogue);
}