using System;
using System.Drawing;
using System.IO;
using Microsoft.Office.Interop.Word;

namespace CatalogueLibrary
{
    public class WordHelper
    {
        private readonly Application _wrdApp;

        public WordHelper(Application wrdApp)
        {
            _wrdApp = wrdApp;
        }

        #region Word helper methods
        public void Write(string toWrite, WdBuiltinStyle style, WdColor color = WdColor.wdColorBlack)
        {
            Object oStyle = style;

            _wrdApp.Selection.Font.Color = color; //set colour then type text

            _wrdApp.Selection.TypeText(toWrite);

            if (color == WdColor.wdColorBlack)
                _wrdApp.Selection.set_Style(ref oStyle); //type text then overwrite with font
        }

        public void WriteLine(string toWrite, WdBuiltinStyle style, WdColor color = WdColor.wdColorBlack)
        {
            Object oStyle = style;

            _wrdApp.Selection.Font.Color = color;

            _wrdApp.Selection.TypeText(toWrite);

            if (color == WdColor.wdColorBlack)
                _wrdApp.Selection.set_Style(ref oStyle);

            _wrdApp.Selection.TypeParagraph();
        }


        public void WriteLine(string toWrite, int fontSize)
        {
            float before = _wrdApp.Selection.Font.Size;

            _wrdApp.Selection.Font.Size = fontSize;
            _wrdApp.Selection.TypeText(toWrite);
            _wrdApp.Selection.TypeParagraph();

            _wrdApp.Selection.Font.Size = before;
        }

        public void WriteLine()
        {
            _wrdApp.Selection.TypeParagraph();
        }
        public void GoToEndOfDocument()
        {
            object what = WdGoToItem.wdGoToPercent;
            object which = WdGoToDirection.wdGoToLast;
            Object oMissing = System.Reflection.Missing.Value;

            _wrdApp.Selection.GoTo(ref what, ref which, ref oMissing, ref oMissing);

        }
        public void StartNewPageInDocument()
        {
            object what = WdGoToItem.wdGoToPercent;
            object which = WdGoToDirection.wdGoToLast;
            Object oMissing = System.Reflection.Missing.Value;

            _wrdApp.Selection.GoTo(ref what, ref which, ref oMissing, ref oMissing);

            _wrdApp.Selection.InsertBreak(WdBreakType.wdPageBreak);
        }
        #endregion

        public Table CreateTable(int linesRequred, int columns, _Document wrdDoc)
        {
            object start = _wrdApp.Selection.End;
            object end = _wrdApp.Selection.End;

            Range tableLocation = wrdDoc.Range(ref start, ref end);
            return wrdDoc.Tables.Add(tableLocation, linesRequred, columns);
        }

        public void GoToStartOfDocument()
        {
            object what = WdGoToItem.wdGoToPercent;
            object which = 0;
            Object oMissing = System.Reflection.Missing.Value;

            _wrdApp.Selection.GoTo(ref what, ref which, ref oMissing, ref oMissing);
        }

        public void WriteImage(Image img, _Document wrdDoc, bool goToEndOfDocumentAfterwards = true, Range range = null)
        {
            string tempFileName = Path.GetTempFileName();

            img.Save(tempFileName);


            if(range != null)
                wrdDoc.InlineShapes.AddPicture(tempFileName,Range:range);
            else
                wrdDoc.InlineShapes.AddPicture(tempFileName);

            File.Delete(tempFileName);

            if (goToEndOfDocumentAfterwards)
            {
                GoToEndOfDocument();
                WriteLine();
            }
        }
    }
}
