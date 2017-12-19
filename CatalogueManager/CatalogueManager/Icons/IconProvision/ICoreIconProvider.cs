using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueManager.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision
{
    public interface ICoreIconProvider:IIconProvider
    {
        void SetClassifications(Dictionary<int, CatalogueItemClassification> classifications);
        ImageList GetImageList(bool addFavouritesOverlayKeysToo);
    }
}