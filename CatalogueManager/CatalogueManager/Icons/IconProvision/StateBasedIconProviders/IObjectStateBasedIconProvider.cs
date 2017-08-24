using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data.PerformanceImprovement;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public interface IObjectStateBasedIconProvider
    {
        Bitmap GetImageIfSupportedObject(object o);
    }
}
