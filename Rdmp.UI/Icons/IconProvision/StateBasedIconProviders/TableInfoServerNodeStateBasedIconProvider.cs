// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Icons.IconProvision.StateBasedIconProviders
{
    public class TableInfoServerNodeStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private DatabaseTypeIconProvider _databaseTypeIconProvider;
        private Bitmap _serverNode;

        public TableInfoServerNodeStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _databaseTypeIconProvider = new DatabaseTypeIconProvider();

            _serverNode = CatalogueIcons.TableInfoServerNode;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var node = o as TableInfoServerNode;

            if (node == null)
                return null;

            return _overlayProvider.GetOverlay(_serverNode, _databaseTypeIconProvider.GetOverlay(node.DatabaseType));
        }
    }
}