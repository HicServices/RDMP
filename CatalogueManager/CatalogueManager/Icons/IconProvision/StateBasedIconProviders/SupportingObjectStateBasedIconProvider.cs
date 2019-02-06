// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueLibrary.Data;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class SupportingObjectStateBasedIconProvider : IObjectStateBasedIconProvider
    {

        private Bitmap _supportingDocument;
        private Bitmap _supportingDocumentGlobal;
        private Bitmap _supportingDocumentExtractable;
        private Bitmap _supportingDocumentExtractableGlobal;

        private Bitmap _supportingSql;
        private Bitmap _supportingSqlGlobal;
        private Bitmap _supportingSqlExtractable;
        private Bitmap _supportingSqlExtractableGlobal;

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
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var doc = o as SupportingDocument;
            if (doc != null)
            {
                if (doc.Extractable)
                    return doc.IsGlobal ? _supportingDocumentExtractableGlobal : _supportingDocumentExtractable;

                return doc.IsGlobal ? _supportingDocumentGlobal : _supportingDocument;
            }

            var sql = o as SupportingSQLTable;
            if (sql != null)
            {
                if (sql.Extractable)
                    return sql.IsGlobal ? _supportingSqlExtractableGlobal : _supportingSqlExtractable;

                return sql.IsGlobal ? _supportingSqlGlobal : _supportingSql;
            }

            return null;
        }
    }
}