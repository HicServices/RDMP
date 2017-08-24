using System.Drawing;

namespace ReusableUIComponents.Dependencies.Models
{
    public class ColorResponse
    {
        public ColorResponse(KnownColor gradientStartColor, KnownColor gradientEndColor)
        {
            GradientStartColor = gradientStartColor;
            GradientEndColor = gradientEndColor;
        }

        public KnownColor GradientStartColor{ get;set; }
        public KnownColor GradientEndColor { get; set; }
    }


}
