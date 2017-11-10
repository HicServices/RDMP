using System.Drawing;
using System.Linq;
using System.Windows.Media;
using ReusableLibraryCode;
using Xceed.Words.NET;

namespace CatalogueLibrary.Reports
{
    public class RequiresMicrosoftOffice
    {
        protected void InsertTitle(DocX document, string tText)
        {
            var p = document.InsertParagraph();
            p.StyleName = "Title";
            p.InsertText(tText);

        }
        protected void InsertParagraph(DocX document, string ptext, int textFontSize = -1)
        {
            var h = document.InsertParagraph();

            //file data
            h.InsertText(ptext);

            if (textFontSize != -1)
                h.FontSize(textFontSize);
        }

        protected void InsertHeader(DocX document, string htext, int headSize = 1)
        {
            var h = document.InsertParagraph();
            h.StyleName = "Heading" + headSize;

            //file data
            h.InsertText(htext);
        }

        protected void SetTableCell(Table table, int row, int col, string value, int fontSize = -1)
        {
            table.Rows[row].Cells[col].Paragraphs.First().Append(value);
            if (fontSize != -1)
                table.Rows[row].Cells[col].Paragraphs.First().FontSize(fontSize);
        }
        protected Picture GetPicture(DocX document, Bitmap bmp)
        {
            var path = System.IO.Path.GetTempFileName();
            bmp.Save(path);

            // Add an image into the document.    
            var image = document.AddImage(path);

            // Create a picture (A custom view of an Image).
            return image.CreatePicture();
        }

        protected void InsertPicture(DocX document, Bitmap img)
        {
            Paragraph p = document.InsertParagraph();
            p.InsertPicture(GetPicture(document,img));
        }
    }
}