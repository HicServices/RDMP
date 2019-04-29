// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.Curation.Data;

namespace Rdmp.UI.Icons.IconProvision.StateBasedIconProviders
{
    public class TableInfoStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _tableInfo;
        private Bitmap _tableInfoTableValuedFunction;

        public TableInfoStateBasedIconProvider()
        {
            _tableInfo = CatalogueIcons.TableInfo;
            _tableInfoTableValuedFunction = CatalogueIcons.TableInfoTableValuedFunction;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var tableInfo = o as TableInfo;

            if (tableInfo == null)
                return null;

            return tableInfo.IsTableValuedFunction ? _tableInfoTableValuedFunction : _tableInfo;
        }
    }
}