// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Governance;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
/// Subcomponent of <see cref="ICatalogueRepository"/> which manages persisting / editing which
/// <see cref="ICatalogue"/> datasets are governed by which <see cref="GovernancePeriod"/> (many to many relationship)
/// </summary>
public interface IGovernanceManager
{
    void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue);
    void Link(GovernancePeriod governancePeriod, ICatalogue catalogue);

    /// <summary>
    /// Returns the IDs of all <see cref="GovernancePeriod"/> with the corresponding set of <see cref="Catalogue"/> IDs which are covered by the governance.
    /// </summary>
    /// <returns></returns>
    Dictionary<int, HashSet<int>> GetAllGovernedCataloguesForAllGovernancePeriods();

    IEnumerable<ICatalogue> GetAllGovernedCatalogues(GovernancePeriod governancePeriod);
}