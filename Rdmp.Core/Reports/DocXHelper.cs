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
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using ReusableLibraryCode;

namespace Rdmp.Core.Reports
{
    /// <summary>
    /// Base class for all reports which generate Microsoft DocX files.  Note that the DocX library is used to create the .docx file so it doesn't actually require Microsoft
    /// Office to be installed on the machine using the class but in order to open the resulting files the user will need something compatible with .docx.
    /// 
    /// <para>Also contains all the helper methods for simplifying (even further) the awesome DocX API for adding paragraphs/pictures/tables.</para>
    /// </summary>
    public class DocXHelper
    {        
        
        const int H1Size = 16;
        const int H2Size = 13;
        const int H3Size = 12;  
        const int H4Size = 11;

        protected void InsertParagraph(XWPFDocument document, string ptext, int textFontSize = -1)
        {
            var h = document.CreateParagraph();
            XWPFRun r0 = h.CreateRun();
            //file data
            r0.SetText(ptext??"");

            r0.FontSize = textFontSize != -1 ? textFontSize : 10;
        }

        protected void InsertHeader(XWPFDocument document, string htext, int headSize = 1)
        {
            var h = document.CreateParagraph();
            XWPFRun r0 = h.CreateRun();
            r0.FontSize = GetSize(headSize);

            //file data
            r0.SetText(htext??"");
        }

        private int GetSize(int headSize)
        {
            switch(headSize)
            {
                case 1: return H1Size;
                case 2: return H2Size;
                case 3: return H3Size;
                case 4: return H4Size;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected void SetTableCell(XWPFTable table, int row, int col, string value, int fontSize = -1)
        {
            var cell = table.GetRow(row).GetCell(col);

            var para = cell.Paragraphs[0];
            var run = para.CreateRun();
            
            run.SetText(value??"");

            if (fontSize != -1)
                run.FontSize = fontSize;
        }
         public const int PICTURE_TYPE_PNG =	6;

        protected XWPFPicture GetPicture(XWPFDocument document, Bitmap bmp)
        {
            var para = document.CreateParagraph();
            var run = para.CreateRun();
            
            return GetPicture(run,bmp);
        }

        protected XWPFPicture GetPicture(XWPFRun run, Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms,ImageFormat.Png);
                
                ms.Seek(0, 0);

                // Add an image into the document.    
                return run.AddPicture(ms,PICTURE_TYPE_PNG,"",Units.ToEMU(bmp.Width), Units.ToEMU(bmp.Height));
            }
        }

        protected XWPFTable InsertTable(XWPFDocument document, int rowCount, int colCount,bool autoFit = true)
        {
            return document.CreateTable(rowCount, colCount);
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

        /// <summary>
        /// Opens windows explorer to show the file
        /// </summary>
        /// <param name="document"></param>
        protected void ShowFile(FileInfo fileInfo)
        {
            UsefulStuff.GetInstance().ShowFileInWindowsExplorer(fileInfo);
        }
        /// <summary>
        /// Opens windows explorer to show the document
        /// </summary>
        /// <param name="document"></param>
        protected void ShowFile(XWPFDocumentFile document)
        {
            ShowFile(document.FileInfo);
        }
        /// <summary>
        /// Creates a new document in Work Area (temp) - see <see cref="GetUniqueFilenameInWorkArea(string, string)"/>
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected XWPFDocumentFile GetNewDocFile(string filename)
        {
            FileInfo fi = GetUniqueFilenameInWorkArea(filename);
            return new XWPFDocumentFile(fi,new FileStream(fi.FullName,FileMode.Create));
        }
        
        /// <summary>
        /// Creates a new document in the location of <paramref name="fileInfo"/>
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        protected XWPFDocumentFile GetNewDocFile(FileInfo fileInfo)
        {
            return new XWPFDocumentFile(fileInfo,new FileStream(fileInfo.FullName,FileMode.Create));
        }
        
        protected void InsertSectionPageBreak(XWPFDocument document)
        {
            var pageBreak = document.CreateParagraph();
            var pageBreakRun = pageBreak.CreateRun();
            pageBreakRun.AddBreak(BreakType.PAGE);
        }
        
        protected void SetLandscape(XWPFDocumentFile document)
        {
            document.Document.body.sectPr = document.Document.body.sectPr??new CT_SectPr();
            document.Document.body.sectPr.pgSz = document.Document.body.sectPr.pgSz ?? new CT_PageSz();
            
            document.Document.body.sectPr.pgSz.orient = ST_PageOrientation.landscape;
            document.Document.body.sectPr.pgSz.w = (842 * 20);
            document.Document.body.sectPr.pgSz.h = (595 * 20);
        

            //document.PageLayout.Orientation = Orientation.Landscape;
        }
        protected void InsertTableOfContents(XWPFDocumentFile document)
        {
            //todo
            //document.InsertTableOfContents("Contents", new TableOfContentsSwitches());
        }
        
        protected void AutoFit(XWPFTable table)
        {
            //tables auto fit already with NPOI
            //table.AutoFit = AutoFit.Contents;
        }

        /// <summary>
        /// Sets the page margins to <paramref name="marginSize"/> in hundredths of an inch e.g. 20 = 0.20"
        /// </summary>
        /// <param name="document"></param>
        /// <param name="marginSize"></param>
        protected void SetMargins(XWPFDocumentFile document, int marginSize)
        {
            document.Document.body.sectPr = document.Document.body.sectPr??new CT_SectPr();
            document.Document.body.sectPr.pgMar.right = (ulong) (marginSize * 14.60);
            document.Document.body.sectPr.pgMar.left = (ulong) (marginSize * 14.60);

            /*document.MarginLeft = marginSize;
            document.MarginRight= marginSize;
            document.MarginTop = marginSize;
            document.MarginBottom = marginSize;*/
        }

        protected float GetPageWidth(XWPFDocumentFile document)
        {
            return 500;
            //return document.PageWidth;
        }
        /// <summary>
        /// An <see cref="XWPFDocument"/> pointed at a <see cref="FileStream"/> that implements <see cref="IDisposable"/> and
        /// disposes of underlying stream when that happens.
        /// </summary>
        protected class XWPFDocumentFile : XWPFDocument, IDisposable
        {
            public FileInfo FileInfo { get; }
            private readonly FileStream _stream;
            
            public XWPFDocumentFile(FileInfo fileInfo,FileStream stream)
            {
                FileInfo = fileInfo;
                this._stream = stream;
            }


            public void Dispose()
            {
                //saves?
                Write(_stream);
                _stream.Close();
                _stream.Dispose();

            }
        }
    }
}
