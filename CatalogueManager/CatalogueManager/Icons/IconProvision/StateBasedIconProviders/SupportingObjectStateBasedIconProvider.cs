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