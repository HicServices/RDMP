//specially for MSword file
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using Xceed.Words.NET;

namespace CatalogueLibrary.Reports
{
    public class DocumentationReportMapsDirectlyToDatabaseOfficeBit : RequiresMicrosoftOffice
    {
        private DocumentationReportMapsDirectlyToDatabase _report;

        public void GenerateReport(ICheckNotifier notifier, Dictionary<string, Bitmap> imagesDictionary)
        {
            try
            {
                _report = new DocumentationReportMapsDirectlyToDatabase(typeof (Catalogue).Assembly);
                _report.Check(notifier);

                //word = new word.ApplicationClass();
                using (DocX document = DocX.Create("RDMPDocumentation"))
                {

                    var t = InsertTable(document,_report.Summaries.Count + 1, 2);
                    
                    //Listing Cell header
                    SetTableCell(t, 0, 0, "Table");
                    SetTableCell(t, 1, 1, "Definition");

                    Type[] keys = _report.Summaries.Keys.ToArray();

                    for (int i = 0; i < _report.Summaries.Count; i++)
                    {
                        SetTableCell(t, i + 1, 0, keys[i].Name);


                        var bmp = GetImage(keys[i],imagesDictionary);

                        if (bmp != null)
                            t.Rows[i + 1].Cells[0].Paragraphs.First().InsertPicture(GetPicture(document, bmp));

                        SetTableCell(t,i + 1, 1, _report.Summaries[keys[i]]);
                    }
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Report generation failed", CheckResult.Fail, e));
            }
        }


        private Bitmap GetImage(Type type, Dictionary<string, Bitmap> imagesDictionary)
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
                return imagesDictionary[key];

            return null;
        }
    }
}
