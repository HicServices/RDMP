// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class SelectedDataSetsStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private ExtractableDataSetStateBasedIconProvider _edsIconProvider;

        public SelectedDataSetsStateBasedIconProvider(IconOverlayProvider overlayProvider, ExtractableDataSetStateBasedIconProvider edsIconProvider)
        {
            _edsIconProvider = edsIconProvider;
            this._overlayProvider = overlayProvider;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is not SelectedDataSets sds)
                return null;

            var edsIcon = _edsIconProvider.GetImageIfSupportedObject(sds.ExtractableDataSet);

            if (edsIcon == null)
                return null;

            var withLink = _overlayProvider.GetOverlay(edsIcon, OverlayKind.Link);

            return withLink;
        }
    }
}