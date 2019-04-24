// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ReusableLibraryCode;
using Xceed.Words.NET;

namespace Rdmp.Core.CatalogueLibrary.Reports
{
    /// <summary>
    /// Base class for all reports which generate Microsoft DocX files.  Note that the DocX library is used to create the .docx file so it doesn't actually require Microsoft
    /// Office to be installed on the machine using the class but in order to open the resulting files the user will need something compatible with .docx.
    /// 
    /// <para>Also contains all the helper methods for simplifying (even further) the awesome DocX API for adding paragraphs/pictures/tables.</para>
    /// </summary>
    public class DocXHelper
    {
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
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms,ImageFormat.Png);
                
                ms.Seek(0, 0);

                // Add an image into the document.    
                var image = document.AddImage(ms);

                // Create a picture (A custom view of an Image).
                return image.CreatePicture();
            }
        }

        protected void InsertPicture(DocX document, Bitmap img)
        {
            Paragraph p = document.InsertParagraph();
            p.InsertPicture(GetPicture(document,img));
        }

        protected Table InsertTable(DocX document, int rowCount, int colCount, TableDesign design = TableDesign.LightList,bool autoFit = true)
        {
            var t = document.InsertTable(rowCount, colCount);

            if (autoFit)
                t.AutoFit = AutoFit.Contents;

            t.Design = design;
            
            return t;
        }

        protected FileInfo GetUniqueFilenameInWorkArea(string desiredName, string extension = ".docx")
        {
            var root = GetTempPath();


            var f = new FileInfo(Path.Combine(root.FullName,UsefulStuff.RemoveIllegalFilenameCharacters(desiredName) + extension));
            int i = 1;

            //file name is taken
            while (f.Exists)
            {
                f = new FileInfo(Path.Combine(root.FullName, desiredName + "_" + i + extension));
                i++;

                //give up it has clearly gone horribly wrong
                if (i > 100)
                    return new FileInfo(Path.Combine(root.FullName, "F" + Guid.NewGuid() + extension));
            }

            return f;
        }

        protected DirectoryInfo GetTempPath()
        {
            return new DirectoryInfo(Path.GetTempPath());
        }

        protected void ShowFile(FileInfo fileInfo)
        {
            UsefulStuff.GetInstance().ShowFileInWindowsExplorer(fileInfo);
        }
    }
}
