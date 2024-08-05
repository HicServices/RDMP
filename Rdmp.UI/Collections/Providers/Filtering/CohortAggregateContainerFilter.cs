// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.Collections.Providers.Filtering;
/// <summary>
/// Filters objects in a <see cref="NewfindUI"/> based on whether the <see cref="Catalogue"/> is marked
/// with various flags (e.g. <see cref="Catalogue.IsDeprecated"/>
/// </summary>
public class CohortAggregateContainerFilter : IModelFilter
{
    private readonly bool _filter = false;

    public CohortAggregateContainerFilter()
    {
        _filter = UserSettings.ScoreZeroForCohortAggregateContainers;
    }
    public bool Filter(object modelObject)
    {
        if (_filter)
        {
            return modelObject is CohortAggregateContainer;
        }
        return true;
    }
}

