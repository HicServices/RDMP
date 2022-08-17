using System;
using ImageFormat=System.Drawing.Imaging.ImageFormat;
using Bitmap = System.Drawing.Bitmap;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI;

public static class ImageTools
{
    public static Bitmap ImageToBitmap(this Image<Rgba32> img)
    {
        using MemoryStream stream=new();
        img.SaveAsPng(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(stream);
    }

    public static Bitmap ImageToBitmap(this byte [] img)
    {
        using MemoryStream ms=new(img); 
        return new Bitmap(ms);
    }

    public static Image<Rgba32> LegacyToImage(this System.Drawing.Image img)
    {
        using MemoryStream stream = new();
        img.Save(stream,ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);
        return Image.Load<Rgba32>(stream);
    }
}