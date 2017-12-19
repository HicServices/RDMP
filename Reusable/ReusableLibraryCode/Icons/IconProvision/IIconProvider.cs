using System.Drawing;

namespace ReusableLibraryCode.Icons.IconProvision
{
    public interface IIconProvider
    {
        Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None);
    }
}