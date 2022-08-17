// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class SupportingObjectStateBasedIconProvider : IObjectStateBasedIconProvider
    {

        private readonly Image<Rgba32> _supportingDocument;
        private readonly Image<Rgba32> _supportingDocumentGlobal;
        private readonly Image<Rgba32> _supportingDocumentExtractable;
        private readonly Image<Rgba32> _supportingDocumentExtractableGlobal;

        private readonly Image<Rgba32> _supportingSql;
        private readonly Image<Rgba32> _supportingSqlGlobal;
        private readonly Image<Rgba32> _supportingSqlExtractable;
        private readonly Image<Rgba32> _supportingSqlExtractableGlobal;

        public SupportingObjectStateBasedIconProvider()
        {
            _supportingDocument = CatalogueIcons.SupportingDocument;
            _supportingDocumentGlobal = CatalogueIcons.SupportingDocumentGlobal;
            _supportingDocumentExtractable = CatalogueIcons.SupportingDocumentExtractable;
            _supportingDocumentExtractableGlobal = CatalogueIcons.SupportingDocumentExtractableGlobal;

            _supportingSql = CatalogueIcons.SupportingSQLTable;
            _supportingSqlGlobal = CatalogueIcons.SupportingSqlGlobal;
            _supportingSqlExtractable = CatalogueIcons.SupportingSqlExtractable;
            _supportingSqlExtractableGlobal = CatalogueIcons.SupportingSqlExtractableGlobal;

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
}