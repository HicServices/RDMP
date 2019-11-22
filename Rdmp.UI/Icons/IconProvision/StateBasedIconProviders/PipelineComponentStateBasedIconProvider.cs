using System.Drawing;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Icons.IconProvision;

namespace Rdmp.UI.Icons.IconProvision.StateBasedIconProviders
{
    public class PipelineComponentStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _component;
        private Bitmap _soure;
        private Bitmap _destnition;

        public PipelineComponentStateBasedIconProvider()
        {
            _component = CatalogueIcons.PipelineComponent;
            _soure = CatalogueIcons.PipelineComponentSource;
            _destnition = CatalogueIcons.PipelineComponentDestination;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is PipelineComponent pc)
            {
                if (pc.Class != null && pc.Class.EndsWith("Source"))
                    return _soure;
                if (pc.Class != null && pc.Class.EndsWith("Destination"))
                    return _destnition;

                return _component;
            }

            return null;

        }
    }
}