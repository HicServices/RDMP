// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;

namespace Rdmp.UI.Raceway;

/// <summary>
///     Input/Persistence collection for <see cref="DatasetRaceway" />
/// </summary>
public class DatasetRacewayObjectCollection : PersistableObjectCollection
{
    public DatasetRacewayObjectCollection()
    {
        //default
        ShowPeriod = DatasetRaceway.RacewayShowPeriod.AllTime;
        IgnoreRows = false;
    }

    public DatasetRaceway.RacewayShowPeriod ShowPeriod { get; set; }
    public bool IgnoreRows { get; set; }

    public Catalogue[] GetCatalogues()
    {
        return DatabaseObjects.Cast<Catalogue>().ToArray();
    }

    public override string SaveExtraText()
    {
        return PersistStringHelper.SaveDictionaryToString(new Dictionary<string, string>
        {
            { "ShowPeriod", ShowPeriod.ToString() },
            { "IgnoreRows", IgnoreRows.ToString() }
        });
    }

    public override void LoadExtraText(string s)
    {
        var dict = PersistStringHelper.LoadDictionaryFromString(s);

        //if it's empty we just use the default values we are set up for
        if (dict == null || !dict.Any())
            return;

        ShowPeriod =
            (DatasetRaceway.RacewayShowPeriod)Enum.Parse(typeof(DatasetRaceway.RacewayShowPeriod), dict["ShowPeriod"],
                true);
        IgnoreRows = Convert.ToBoolean(dict["IgnoreRows"]);
    }

    public void AddCatalogue(Catalogue catalogue)
    {
        if (catalogue == null)
            throw new ArgumentException("Catalogue must not be null", nameof(catalogue));

        DatabaseObjects.Add(catalogue);
    }

    public void RemoveCatalogue(Catalogue catalogue)
    {
        if (catalogue == null)
            throw new ArgumentException("Catalogue must not be null", nameof(catalogue));

        DatabaseObjects.Remove(catalogue);
    }

    public void ClearDatabaseObjects()
    {
        DatabaseObjects.Clear();
    }
}