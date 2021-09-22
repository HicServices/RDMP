// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractableDataSetStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private CatalogueStateBasedIconProvider _catalogueIconProvider;
        private Bitmap _disabled;

        public ExtractableDataSetStateBasedIconProvider(IconOverlayProvider overlayProvider, CatalogueStateBasedIconProvider catalogueIconProvider)
        {
            _catalogueIconProvider = catalogueIconProvider;
            _disabled = CatalogueIcons.ExtractableDataSetDisabled;
            this._overlayProvider = overlayProvider;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var ds = o as ExtractableDataSet ;
            if (ds == null)
                return null;

            var cataOne = _catalogueIconProvider.GetImageIfSupportedObject(ds.Catalogue);

            if (cataOne == null)
                return null;

            var withE = _overlayProvider.GetOverlay(cataOne, OverlayKind.BigE);

            return ds.IsCatalogueDeprecated || ds.DisableExtraction ? _disabled : withE;
        }
    }
}