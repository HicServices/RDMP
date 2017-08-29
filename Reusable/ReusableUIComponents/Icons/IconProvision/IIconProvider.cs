using System.Drawing;

namespace ReusableUIComponents.Icons.IconProvision
{
    public interface IIconProvider
    {
        Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None);
    }
}