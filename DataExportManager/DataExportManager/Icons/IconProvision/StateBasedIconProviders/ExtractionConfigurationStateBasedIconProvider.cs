using System.Drawing;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Collections.Providers;
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