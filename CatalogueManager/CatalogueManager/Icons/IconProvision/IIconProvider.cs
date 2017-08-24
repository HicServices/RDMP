using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.Icons.IconOverlays;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Icons.IconProvision
{
    public interface IIconProvider
    {
        Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None);
    }
}