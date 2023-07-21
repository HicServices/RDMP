// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class SupportingObjectStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _supportingDocument;
    private readonly Image<Rgba32> _supportingDocumentExtractable;
    private readonly Image<Rgba32> _supportingDocumentExtractableGlobal;
    private readonly Image<Rgba32> _supportingDocumentGlobal;

    private readonly Image<Rgba32> _supportingSql;
    private readonly Image<Rgba32> _supportingSqlExtractable;
    private readonly Image<Rgba32> _supportingSqlExtractableGlobal;
    private readonly Image<Rgba32> _supportingSqlGlobal;

    public SupportingObjectStateBasedIconProvider()
    {
        _supportingDocument = Image.Load<Rgba32>(CatalogueIcons.SupportingDocument);
        _supportingDocumentGlobal = Image.Load<Rgba32>(CatalogueIcons.SupportingDocumentGlobal);
        _supportingDocumentExtractable = Image.Load<Rgba32>(CatalogueIcons.SupportingDocumentExtractable);
        _supportingDocumentExtractableGlobal = Image.Load<Rgba32>(CatalogueIcons.SupportingDocumentExtractableGlobal);

        _supportingSql = Image.Load<Rgba32>(CatalogueIcons.SupportingSQLTable);
        _supportingSqlGlobal = Image.Load<Rgba32>(CatalogueIcons.SupportingSqlGlobal);
        _supportingSqlExtractable = Image.Load<Rgba32>(CatalogueIcons.SupportingSqlExtractable);
        _supportingSqlExtractableGlobal = Image.Load<Rgba32>(CatalogueIcons.SupportingSqlExtractableGlobal);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        return o switch
        {
            SupportingDocument { Extractable: true } doc => doc.IsGlobal
                ? _supportingDocumentExtractableGlobal
                : _supportingDocumentExtractable,
            SupportingDocument doc => doc.IsGlobal ? _supportingDocumentGlobal : _supportingDocument,
            SupportingSQLTable { Extractable: true } sql => sql.IsGlobal
                ? _supportingSqlExtractableGlobal
                : _supportingSqlExtractable,
            SupportingSQLTable sql => sql.IsGlobal ? _supportingSqlGlobal : _supportingSql,
            _ => null
        };
    }
}