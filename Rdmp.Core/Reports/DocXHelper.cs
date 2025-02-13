// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using Rdmp.Core.ReusableLibraryCode;
using SixLabors.ImageSharp;

namespace Rdmp.Core.Reports;

/// <summary>
/// Base class for all reports which generate Microsoft DocX files.  Note that the DocX library is used to create the .docx file so it doesn't actually require Microsoft
/// Office to be installed on the machine using the class but in order to open the resulting files the user will need something compatible with .docx.
/// 
/// <para>Also contains all the helper methods for simplifying (even further) the awesome DocX API for adding paragraphs/pictures/tables.</para>
/// </summary>
public class DocXHelper
{
    private const int H1Size = 16;
    private const int H2Size = 13;
    private const int H3Size = 12;
    private const int H4Size = 11;

    /// <summary>
    /// <see cref="Units.ToEMU(double)"/> seems to result in word showing images at 133% size.  This constant fixes that
    /// problem when using the <see cref="GetPicture(XWPFDocument, Image)"/> methods.
    /// 
    /// </summary>
    private const float PICTURE_SCALING = 0.75f;

    protected static void InsertParagraph(XWPFDocument document, string ptext, int textFontSize = -1)
    {
        if (string.IsNullOrWhiteSpace(ptext)) return;

        foreach (var para in ptext.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            var h = document.CreateParagraph();
            var r0 = h.CreateRun();
            //file data
            r0.SetText(para);

            r0.FontSize = textFontSize != -1 ? textFontSize : 10;
        }
    }

    protected static void InsertHeader(XWPFDocument document, string htext, int headSize = 1)
    {
        var h = document.CreateParagraph();
        var r0 = h.CreateRun();
        r0.FontSize = GetSize(headSize);

        //file data
        r0.SetText(htext ?? "");
    }

    private static int GetSize(int headSize) =>
        headSize switch
        {
            1 => H1Size,
            2 => H2Size,
            3 => H3Size,
            4 => H4Size,
            _ => throw new ArgumentOutOfRangeException(nameof(headSize))
        };

    protected static void SetTableCell(XWPFTable table, int row, int col, string value, int fontSize = -1)
    {
        if (string.IsNullOrEmpty(value)) return;

        var cell = table.GetRow(row).GetCell(col);

        var first = true;

        foreach (var bit in value.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            var para = first ? cell.Paragraphs[0] : cell.AddParagraph();
            var run = para.CreateRun();

            run.SetText(bit);
            run.AddBreak();

            if (fontSize != -1)
                run.FontSize = fontSize;

            first = false;
        }
    }

    public const int PICTURE_TYPE_PNG = 6;

    protected static XWPFPicture GetPicture(XWPFDocument document, Image bmp)
    {
        var para = document.CreateParagraph();
        var run = para.CreateRun();

        return GetPicture(run, bmp);
    }

    protected static XWPFPicture GetPicture(XWPFRun run, Image bmp)
    {
        using var ms = new MemoryStream();
        bmp.SaveAsPng(ms);

        ms.Seek(0, 0);

        // Add an image into the document.
        var picture = run.AddPicture(ms, PICTURE_TYPE_PNG, "", Units.ToEMU(bmp.Width * PICTURE_SCALING),
            Units.ToEMU(bmp.Height * PICTURE_SCALING));

        return picture;
    }

    protected static XWPFTable InsertTable(XWPFDocument document, int rowCount, int colCount)
    {
        var table1 = document.CreateTable(rowCount, colCount);
        var tblLayout1 = table1.GetCTTbl().tblPr.AddNewTblLayout();
        tblLayout1.type = ST_TblLayoutType.@fixed;

        const int width = 10000;

        for (var i = 0; i < colCount; i++) table1.SetColumnWidth(i, (ulong)(width / colCount));

        return table1;
    }

    protected static FileInfo GetUniqueFilenameInWorkArea(string desiredName, string extension = ".docx")
    {
        var root = GetTempPath();


        var f = new FileInfo(Path.Combine(root.FullName,
            UsefulStuff.RemoveIllegalFilenameCharacters(desiredName) + extension));
        var i = 1;

        //file name is taken
        while (f.Exists)
        {
            f = new FileInfo(Path.Combine(root.FullName, $"{desiredName}_{i}{extension}"));
            i++;

            //give up it has clearly gone horribly wrong
            if (i > 100)
                return new FileInfo(Path.Combine(root.FullName, $"F{Guid.NewGuid()}{extension}"));
        }

        return f;
    }

    protected static DirectoryInfo GetTempPath() => new(Path.GetTempPath());

    /// <summary>
    /// Creates a new document in Work Area (temp) - see <see cref="GetUniqueFilenameInWorkArea(string, string)"/>
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    protected static XWPFDocumentFile GetNewDocFile(string filename)
    {
        var fi = GetUniqueFilenameInWorkArea(filename);
        return new XWPFDocumentFile(fi, new FileStream(fi.FullName, FileMode.Create));
    }

    /// <summary>
    /// Opens windows explorer to show the file
    /// </summary>
    /// <param name="fileInfo"></param>
    protected static void ShowFile(FileInfo fileInfo)
    {
        UsefulStuff.ShowPathInWindowsExplorer(fileInfo);
    }

    /// <summary>
    /// Opens windows explorer to show the document
    /// </summary>
    /// <param name="document"></param>
    protected static void ShowFile(XWPFDocumentFile document)
    {
        ShowFile(document.FileInfo);
    }

    protected static void AddFooter(XWPFDocument document, string text, int textFontSize)
    {
        var secPr = document.Document.body.sectPr;
        var footer = new CT_Ftr();
        var run = footer.AddNewP().AddNewR();
        run.AddNewT().Value = text;
        var relation2 = XWPFRelation.FOOTER;
        var myFooter =
            (XWPFFooter)document.CreateRelationship(relation2, XWPFFactory.GetInstance(),
                document.FooterList.Count + 1);

        myFooter.SetHeaderFooter(footer);
        var myFooterRef = secPr.AddNewFooterReference();
        myFooterRef.type = ST_HdrFtr.@default;
#pragma warning disable CS0618 // Type or member is obsolete
        myFooterRef.id = myFooter.GetPackageRelationship().Id;
#pragma warning restore CS0618 // Type or member is obsolete
        myFooter.Paragraphs[0].Runs[0].FontSize = textFontSize != -1 ? textFontSize : 10;
    }

    /// <summary>
    /// Creates a new document in the location of <paramref name="fileInfo"/>
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    protected static XWPFDocumentFile GetNewDocFile(FileInfo fileInfo) =>
        new(fileInfo, new FileStream(fileInfo.FullName, FileMode.Create));

    protected static void InsertSectionPageBreak(XWPFDocument document)
    {
        var pageBreak = document.CreateParagraph();
        var pageBreakRun = pageBreak.CreateRun();
        pageBreakRun.AddBreak(BreakType.PAGE);
    }

    protected static void SetLandscape(XWPFDocumentFile document)
    {
        document.Document.body.sectPr ??= new CT_SectPr();
        document.Document.body.sectPr.pgSz ??= new CT_PageSz();

        document.Document.body.sectPr.pgSz.orient = ST_PageOrientation.landscape;
        document.Document.body.sectPr.pgSz.w = 842 * 20;
        document.Document.body.sectPr.pgSz.h = 595 * 20;


        //document.PageLayout.Orientation = Orientation.Landscape;
    }

    protected static void InsertTableOfContents(XWPFDocumentFile document)
    {
        //todo
        //document.InsertTableOfContents("Contents", new TableOfContentsSwitches());
    }

    protected static void AutoFit(XWPFTable table)
    {
        //tables auto fit already with NPOI
        //table.AutoFit = AutoFit.Contents;
    }

    /// <summary>
    /// Sets the page margins to <paramref name="marginSize"/> in hundredths of an inch e.g. 20 = 0.20"
    /// </summary>
    /// <param name="document"></param>
    /// <param name="marginSize"></param>
    protected static void SetMargins(XWPFDocumentFile document, int marginSize)
    {
        document.Document.body.sectPr ??= new CT_SectPr();
        document.Document.body.sectPr.pgMar.right = (ulong)(marginSize * 14.60);
        document.Document.body.sectPr.pgMar.left = (ulong)(marginSize * 14.60);

        /*document.MarginLeft = marginSize;
        document.MarginRight= marginSize;
        document.MarginTop = marginSize;
        document.MarginBottom = marginSize;*/
    }

    protected static float GetPageWidth() => 500;

    //return document.PageWidth;
    /// <summary>
    /// An <see cref="XWPFDocument"/> pointed at a <see cref="FileStream"/> that implements <see cref="IDisposable"/> and
    /// disposes of underlying stream when that happens.
    /// </summary>
    protected class XWPFDocumentFile : XWPFDocument, IDisposable
    {
        public FileInfo FileInfo { get; }
        private readonly FileStream _stream;

        public XWPFDocumentFile(FileInfo fileInfo, FileStream stream)
        {
            FileInfo = fileInfo;
            _stream = stream;
        }


        public new void Dispose()
        {
            //saves?
            Write(_stream);
            _stream.Close();
            _stream.Dispose();
        }
    }
}