// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Providers.Nodes;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    internal class CatalogueItemsNodeStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _basic;
        private Bitmap _core;
        private Bitmap _internal;
        private Bitmap _supplemental;
        private Bitmap _special;
        private Bitmap _deprecated;

        public CatalogueItemsNodeStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _basic = CatalogueIcons.CatalogueItemsNode;
            _core = overlayProvider.GetOverlay(_basic, OverlayKind.Extractable);
            _internal = overlayProvider.GetOverlay(_basic, OverlayKind.Extractable_Internal);
            _supplemental = overlayProvider.GetOverlay(_basic, OverlayKind.Extractable_Supplemental);
            _special = overlayProvider.GetOverlay(_basic, OverlayKind.Extractable_SpecialApproval);
            _deprecated = overlayProvider.GetOverlay(_basic, OverlayKind.Deprecated);
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is not CatalogueItemsNode cin)
                return null;

            if (cin.Category == null)
                return _basic;

            switch (cin.Category.Value)
            {
                case Curation.Data.ExtractionCategory.Core:
                    return _core;
                case Curation.Data.ExtractionCategory.Supplemental:
                    return _supplemental;
                case Curation.Data.ExtractionCategory.SpecialApprovalRequired:
                    return _special;
                case Curation.Data.ExtractionCategory.Internal:
                    return _internal;
                case Curation.Data.ExtractionCategory.Deprecated:
                    return _deprecated;
            }

            return _basic;
        }
    }
}