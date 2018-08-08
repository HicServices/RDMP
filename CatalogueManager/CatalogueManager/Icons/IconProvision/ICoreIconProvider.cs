using System.Windows.Forms;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision
{
    public interface ICoreIconProvider:IIconProvider
    {
        ImageList GetImageList(bool addFavouritesOverlayKeysToo);

        /// <summary>
        /// Returns true if there is a valid icon associated with the object (i.e. not a NoIconAvailable icon).
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool HasIcon(object o);
    }
}