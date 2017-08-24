using System.Collections.Specialized;
using System.Drawing;

namespace ReusableUIComponents.Dependencies.Models
{
    public interface IObjectVisualisation
    {
        Bitmap GetImage(object toRender);
        OrderedDictionary EntityInformation(object toRender);
        ColorResponse GetColor(object toRender, ColorRequest request);
        string[] GetNameAndType(object toRender);
    }
}