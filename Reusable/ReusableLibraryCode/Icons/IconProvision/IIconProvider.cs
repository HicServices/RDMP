using System.Drawing;

namespace ReusableLibraryCode.Icons.IconProvision
{
    /// <summary>
    /// Provides 19x19 pixel images for the given object which could be an RDMPConcept, class instance or Type.
    /// </summary>
    public interface IIconProvider
    {
        Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None);
    }
}