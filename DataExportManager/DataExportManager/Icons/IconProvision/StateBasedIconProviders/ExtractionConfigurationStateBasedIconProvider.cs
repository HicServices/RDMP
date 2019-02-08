// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractionConfigurationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _normal;
        private Bitmap _frozen;
        
        public ExtractionConfigurationStateBasedIconProvider(DataExportIconProvider iconProvider)
        {
            _normal = CatalogueIcons.ExtractionConfiguration;
            _frozen = CatalogueIcons.FrozenExtractionConfiguration;

        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var ec = o as ExtractionConfiguration;

            if (ec == null)
                return null;

            Bitmap basicImage = ec.IsReleased ? _frozen : _normal;

            return basicImage;//its all fine and green
        }
    }
}