//specially for MSword file
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Triggers;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Reports
{
    public class DocumentationReportMapsDirectlyToDatabaseOfficeBit : RequiresMicrosoftOffice
    {
        Microsoft.Office.Interop.Word.Application wrdApp;
        private DocumentationReportMapsDirectlyToDatabase _report;

        public void GenerateReport(ICheckNotifier notifier, Dictionary<string, Bitmap> imagesDictionary)
        {
            try
            {
                _report = new DocumentationReportMapsDirectlyToDatabase(typeof(Catalogue).Assembly);
                _report.Check(notifier);
            
                object oMissing = Missing.Value;
                object oEndOfDoc = "\\endofdoc"; /* \endofdoc is a predefined bookmark */

                //word = new word.ApplicationClass();
                wrdApp = new Application();

                wrdApp.Visible = true;
                var doc = wrdApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                Range wrdRng = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;

                Table newTable;
                newTable = doc.Tables.Add(wrdRng, _report.Summaries.Count+1, 2, ref oMissing, ref oMissing);
                newTable.Borders.InsideLineStyle = WdLineStyle.wdLineStyleSingle;
                newTable.Borders.OutsideLineStyle = WdLineStyle.wdLineStyleSingle;
                newTable.AllowAutoFit = true;

                var wordHelper = new WordHelper(wrdApp);
                
                //Listing Cell header
                newTable.Cell(1, 1).Range.Text = "Table";
                newTable.Cell(1, 2).Range.Text = "Definition";

                Type[] keys = _report.Summaries.Keys.ToArray();

                for(int i=0;i<_report.Summaries.Count;i++)
                {
                    newTable.Cell(i + 2, 1).Range.Text = keys[i].Name;
                    
                    //select the start of the cell
                    var r = newTable.Cell(i + 2, 1).Range;
                    r.Collapse(WdCollapseDirection.wdCollapseStart);
                    r.Select();

                    WriteImageIfExistsForType(keys[i], imagesDictionary,wordHelper,doc);

                    newTable.Cell(i + 2, 2).Range.Text = _report.Summaries[keys[i]];
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Report generation failed",CheckResult.Fail ,e));
            }
        }
        
        private void WriteImageIfExistsForType(Type type, Dictionary<string, Bitmap> imagesDictionary, WordHelper wordHelper, Document doc)
        {
            string key = type.Name;

            if (typeof (IFilter).IsAssignableFrom(type))
                key = "Filter";

            if (typeof (IContainer).IsAssignableFrom(type))
                key = "FilterContainer";

            if (typeof(ISqlParameter).IsAssignableFrom(type))
                key = "ParametersNode";
            
            //if it has an image associated with it add it
            if (imagesDictionary.ContainsKey(key))
                wordHelper.WriteImage(imagesDictionary[key], doc, goToEndOfDocumentAfterwards: false);
        }
    }
}
