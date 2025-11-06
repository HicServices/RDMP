// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.UI.Collections.Providers.Filtering;

/// <summary>
/// Filters objects in a <see cref="CatalogueCollectionUI"/> based on whether the <see cref="Catalogue"/> is marked
/// with various flags (e.g. <see cref="Catalogue.IsDeprecated"/>
/// </summary>
public class CatalogueCollectionFilter : IModelFilter
{
    public ICoreChildProvider ChildProvider { get; set; }
    private readonly bool _isInternal;
    private readonly bool _isDeprecated;
    private readonly bool _isProjectSpecific;

    public CatalogueCollectionFilter(ICoreChildProvider childProvider)
    {
        ChildProvider = childProvider;
        _isInternal = UserSettings.ShowInternalCatalogues;
        _isDeprecated = UserSettings.ShowDeprecatedCatalogues;
        _isProjectSpecific = UserSettings.ShowProjectSpecificCatalogues;
    }

    public bool Filter(object modelObject) => SearchablesMatchScorer.Filter(modelObject,
        ChildProvider.GetDescendancyListIfAnyFor(modelObject), _isInternal, _isDeprecated,
        _isProjectSpecific);
}