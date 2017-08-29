using System.Drawing;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Collections.Providers;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractionConfigurationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly DataExportIconProvider _iconProvider;
        private DataExportProblemProvider _problemProvider;
        private DataExportChildProvider _childProvider;

        private Bitmap _normal;
        private Bitmap _frozen;
        
        public ExtractionConfigurationStateBasedIconProvider(DataExportIconProvider iconProvider)
        {
            _iconProvider = iconProvider;

            _normal = CatalogueIcons.ExtractionConfiguration;
            _frozen = CatalogueIcons.FrozenExtractionConfiguration;

        }

        public void SetProviders(DataExportChildProvider childProvider, DataExportProblemProvider problemProvider)
        {
            _problemProvider = problemProvider;
            _childProvider = childProvider;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var ec = o as ExtractionConfiguration;

            if (ec == null)
                return null;

            Bitmap basicImage = ec.IsReleased ? _frozen : _normal;

            //does it have problems?
            if (_problemProvider != null && _problemProvider.HasProblems(ec))
                return _iconProvider.OverlayProvider.GetOverlay(basicImage,OverlayKind.Problem);

            return basicImage;//its all fine and green
        }
    }
}