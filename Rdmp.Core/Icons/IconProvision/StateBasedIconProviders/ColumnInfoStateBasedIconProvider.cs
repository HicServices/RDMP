// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class ColumnInfoStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly IconOverlayProvider _overlayProvider;
    private readonly Image<Rgba32> _columnInfo;
    private readonly Image<Rgba32> _columnInfoWithANO;

    public ColumnInfoStateBasedIconProvider(IconOverlayProvider overlayProvider)
    {
        _overlayProvider = overlayProvider;
        _columnInfo = Image.Load<Rgba32>(CatalogueIcons.ColumnInfo);
        _columnInfoWithANO = Image.Load<Rgba32>(CatalogueIcons.ANOColumnInfo);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not ColumnInfo columnInfo)
            return null;

        var basicIcon = columnInfo.ANOTable_ID != null ? _columnInfoWithANO : _columnInfo;

        return columnInfo.IsPrimaryKey ? _overlayProvider.GetOverlay(basicIcon, OverlayKind.Key) : basicIcon;
    }
}