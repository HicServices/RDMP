// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractableCohortStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private Image _basicIcon;

        public ExtractableCohortStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _basicIcon = CatalogueIcons.ExtractableCohort;
        }

        public Image GetImageIfSupportedObject(object o)
        {
            var cohort = o as ExtractableCohort;

            if (cohort != null)
                return cohort.IsDeprecated
                    ? _overlayProvider.GetOverlay(_basicIcon, OverlayKind.Deprecated)
                    : _basicIcon;

            return null;
        }
    }
}