using System;
using System.Collections.Generic;
using System.Drawing;

namespace ReusableUIComponents
{
    public class IconFactory
    {
        private readonly Dictionary<Bitmap, Icon> _iconDictionary = new Dictionary<Bitmap, Icon>();

        public Icon GetIcon(Bitmap bmp)
        {
            if (_iconDictionary.ContainsKey(bmp))
                return _iconDictionary[bmp];

            return CreateIcon(bmp);
        }

        private Icon CreateIcon(Bitmap bmp)
        {
            // Get an Hicon for myBitmap.
            IntPtr Hicon = bmp.GetHicon();

            // Create a new icon from the handle. 
            Icon newIcon = Icon.FromHandle(Hicon);

            //now that we have created the icon don't create it again for the same bitmap (to avoid memory leaks)
            _iconDictionary.Add(bmp, newIcon);

            return newIcon;
        }

    }
}
