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
using Rdmp.Core.Repositories;

namespace Rdmp.UI.PieCharts;

/// <summary>
/// Input object for <see cref="GoodBadCataloguePieChart"/> Records whether it is showing all/single <see cref="Catalogue"/> and which is being shown.
/// </summary>
public class GoodBadCataloguePieChartObjectCollection : PersistableObjectCollection
{
    public bool ShowLabels { get; set; }

    //Catalogue filters
    public bool IncludeNonExtractableCatalogues { get; set; }
    public bool IncludeDeprecatedCatalogues { get; set; }
    public bool IncludeInternalCatalogues { get; set; }
    public bool IncludeProjectSpecificCatalogues { get; set; }

    //Catalogue item filters

    public bool IncludeNonExtractableCatalogueItems { get; set; }
    public bool IncludeInternalCatalogueItems { get; set; }

    public bool IncludeDeprecatedCatalogueItems { get; set; }


    public bool IsSingleCatalogueMode => DatabaseObjects.Any();


    /// <summary>
    /// Returns true if the Catalogue <paramref name="c"/> should be included in the good/bad counts
    /// based on the flags e.g. <see cref="IncludeDeprecatedCatalogues"/>
    /// </summary>
    /// <param name="c"></param>
    /// <param name="repo"></param>
    /// <returns></returns>
    public bool Include(Catalogue c, IDataExportRepository repo)
    {
        var returnValue = true;

        var status = c.GetExtractabilityStatus(repo);

        if (!status.IsExtractable)
            returnValue &= IncludeNonExtractableCatalogues;

        if (status.IsProjectSpecific)
            returnValue &= IncludeProjectSpecificCatalogues;

        if (c.IsDeprecated)
            returnValue &= IncludeDeprecatedCatalogues;

        if (c.IsInternalDataset)
            returnValue &= IncludeInternalCatalogueItems;


        return returnValue;
    }

    /// <summary>
    /// Returns true if the <see cref="CatalogueItem"/> <paramref name="ci"/> should be included in the good/bad
    /// counts based on the flags e.g. <see cref="IncludeDeprecatedCatalogueItems"/>
    /// </summary>
    /// <param name="ci"></param>
    /// <returns></returns>
    public bool Include(CatalogueItem ci)
    {
        var ei = ci.ExtractionInformation;

        return ei == null
            ? IncludeNonExtractableCatalogueItems
            : ei.ExtractionCategory switch
            {
                ExtractionCategory.Internal => IncludeInternalCatalogueItems,
                ExtractionCategory.Deprecated => IncludeDeprecatedCatalogueItems,
                _ => true
            };
    }

    public Catalogue GetSingleCatalogueModeCatalogue() => (Catalogue)DatabaseObjects.SingleOrDefault();

    public override string SaveExtraText() =>
        PersistStringHelper.SaveDictionaryToString(new Dictionary<string, string>
        {
            { nameof(ShowLabels), ShowLabels.ToString() },

            { nameof(IncludeNonExtractableCatalogues), IncludeNonExtractableCatalogues.ToString() },
            { nameof(IncludeDeprecatedCatalogues), IncludeDeprecatedCatalogues.ToString() },
            { nameof(IncludeInternalCatalogues), IncludeInternalCatalogues.ToString() },
            { nameof(IncludeProjectSpecificCatalogues), IncludeProjectSpecificCatalogues.ToString() },

            { nameof(IncludeNonExtractableCatalogueItems), IncludeNonExtractableCatalogueItems.ToString() },
            { nameof(IncludeInternalCatalogueItems), IncludeInternalCatalogueItems.ToString() },
            { nameof(IncludeDeprecatedCatalogueItems), IncludeDeprecatedCatalogueItems.ToString() }
        });

    public override void LoadExtraText(string s)
    {
        var dict = PersistStringHelper.LoadDictionaryFromString(s);

        //if it's empty we just use the default values we are set up for
        if (dict == null || !dict.Any())
            return;

        ShowLabels = PersistStringHelper.GetBool(dict, nameof(ShowLabels), true);

        IncludeNonExtractableCatalogues =
            PersistStringHelper.GetBool(dict, nameof(IncludeNonExtractableCatalogues), true);
        IncludeDeprecatedCatalogues = PersistStringHelper.GetBool(dict, nameof(IncludeDeprecatedCatalogues), true);
        IncludeInternalCatalogues = PersistStringHelper.GetBool(dict, nameof(IncludeInternalCatalogues), true);
        IncludeProjectSpecificCatalogues =
            PersistStringHelper.GetBool(dict, nameof(IncludeProjectSpecificCatalogues), true);

        IncludeNonExtractableCatalogueItems =
            PersistStringHelper.GetBool(dict, nameof(IncludeNonExtractableCatalogueItems), true);
        IncludeInternalCatalogueItems = PersistStringHelper.GetBool(dict, nameof(IncludeInternalCatalogueItems), true);
        IncludeDeprecatedCatalogueItems =
            PersistStringHelper.GetBool(dict, nameof(IncludeDeprecatedCatalogueItems), true);
    }

    public void SetAllCataloguesMode()
    {
        DatabaseObjects.Clear();
    }

    public void SetSingleCatalogueMode(Catalogue catalogue)
    {
        if (catalogue == null)
            throw new ArgumentException("Catalogue must not be null to turn on SingleCatalogue mode",
                nameof(catalogue));

        DatabaseObjects.Clear();
        DatabaseObjects.Add(catalogue);
    }
}