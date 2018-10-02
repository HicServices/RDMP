using System.Drawing;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Describes an Aggregate Graph or Heatmap etc with optional description/headers
    /// </summary>
    public class BitmapWithDescription
    {
        public Bitmap Bitmap { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }

        public BitmapWithDescription(Bitmap bitmap, string header, string description)
        {
            Bitmap = bitmap;
            Header = header;
            Description = description;
        }
    }
}