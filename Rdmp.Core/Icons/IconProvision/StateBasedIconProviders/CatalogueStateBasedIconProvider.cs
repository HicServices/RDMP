// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class CatalogueStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Image _basic;
        private Image _projectSpecific;
        private readonly IDataExportRepository _dataExportRepository;
        private IconOverlayProvider _overlayProvider;


        public CatalogueStateBasedIconProvider(IDataExportRepository dataExportRepository,
            IconOverlayProvider overlayProvider)
        {
            _basic = CatalogueIcons.Catalogue;
            _projectSpecific = CatalogueIcons.ProjectCatalogue;

            _dataExportRepository = dataExportRepository;
            _overlayProvider = overlayProvider;

        }

        public Image GetImageIfSupportedObject(object o)
        {
            var c = o as Catalogue;
            
            if (c == null)
                return null;

            var status = c.GetExtractabilityStatus(_dataExportRepository);

            Image img;
            if (status != null && status.IsExtractable && status.IsProjectSpecific)
                img = _projectSpecific;
            else
                img = _basic;

            if (c.IsApiCall())
                img = _overlayProvider.GetOverlay(img, OverlayKind.Cloud);

            if (c.IsDeprecated)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Deprecated);
            
            if (c.IsInternalDataset)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Internal);
            
            if (status != null && status.IsExtractable)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Extractable);

            return img;
        }
    }
}