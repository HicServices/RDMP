using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ReusableUIComponents.Dependencies.Models
{
    public class WPFBitmapConverter : IValueConverter
    {
        #region IValueConverter Members

        private readonly Dictionary<Byte[], BitmapImage> _cachedBitmapImages = new Dictionary<Byte[], BitmapImage>(new ByteComparer());
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ImageConverter converter = new ImageConverter();
            byte[] rawImageData = converter.ConvertTo(value, typeof(byte[])) as byte[];
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(rawImageData);

            if (_cachedBitmapImages.ContainsKey(hash))
                return _cachedBitmapImages[hash];

            MemoryStream ms = new MemoryStream();
            ((Bitmap)value).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            _cachedBitmapImages.Add(hash, image);
            return image;


        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}


