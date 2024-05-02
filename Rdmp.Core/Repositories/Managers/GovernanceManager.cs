// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Governance;

namespace Rdmp.Core.Repositories.Managers;

internal class GovernanceManager : IGovernanceManager
{
    private readonly CatalogueRepository _catalogueRepository;

    public GovernanceManager(CatalogueRepository catalogueRepository)
    {
        _catalogueRepository = catalogueRepository;
    }

    public void Unlink(GovernancePeriod governancePeriod, ICatalogue catalogue)
    {
        _catalogueRepository.Delete(
            $@"DELETE FROM GovernancePeriod_Catalogue WHERE Catalogue_ID={catalogue.ID} AND GovernancePeriod_ID={governancePeriod.ID}");
    }

    public void Link(GovernancePeriod governancePeriod, ICatalogue catalogue)
    {
        // do not insert the same link twice
        if (governancePeriod.GovernedCatalogues.Contains(catalogue))
            return;

        _catalogueRepository.Insert(
            $@"INSERT INTO GovernancePeriod_Catalogue (Catalogue_ID,GovernancePeriod_ID) VALUES ({catalogue.ID},{governancePeriod.ID})",
            null);
    }

    /// <inheritdoc />
    public Dictionary<int, HashSet<int>> GetAllGovernedCataloguesForAllGovernancePeriods()
    {
        var toReturn = new Dictionary<int, HashSet<int>>();

        var server = _catalogueRepository.DiscoveredServer;
        using var con = server.GetConnection();
        con.Open();
        using var cmd =
            server.GetCommand(@"SELECT GovernancePeriod_ID,Catalogue_ID FROM GovernancePeriod_Catalogue", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var gp = (int)r["GovernancePeriod_ID"];
            var cata = (int)r["Catalogue_ID"];

            if (!toReturn.ContainsKey(gp))
                toReturn.Add(gp, new HashSet<int>());

            toReturn[gp].Add(cata);
        }

        return toReturn;
    }

    public IEnumerable<ICatalogue> GetAllGovernedCatalogues(GovernancePeriod governancePeriod)
    {
        return _catalogueRepository.SelectAll<Catalogue>(
            $@"SELECT Catalogue_ID FROM GovernancePeriod_Catalogue where GovernancePeriod_ID={governancePeriod.ID}",
            "Catalogue_ID");
    }
}