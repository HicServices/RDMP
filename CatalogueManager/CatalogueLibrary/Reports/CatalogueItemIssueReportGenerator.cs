using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Reports
{
    public class CatalogueItemIssueReportGenerator: RequiresMicrosoftOffice
    {
        /*List<CatalogueItemIssue> Issues;
        private Microsoft.Office.Interop.Excel.Application xlApp;
        private object _missing = false;

        public CatalogueItemIssueReportGenerator(IRepository repository)
        {

            Issues = new List<CatalogueItemIssue>(repository.GetAllObjects<CatalogueItemIssue>());
        }

        private int numberOfReferencedSheetsCreated = 0;

        public void GenerateReport()
        {
            numberOfReferencedSheetsCreated= 0;
            _filesToCleanup = new List<string>();

            xlApp = new Microsoft.Office.Interop.Excel.Application();

            xlApp.Visible = false;
            xlApp.UserControl = false;

            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = (Worksheet)wb.Worksheets[1];


            if (ws == null)
            {
                Console.WriteLine("Worksheet could not be created. Check that your office installation and project references are correct.");
            }

            //excel indexes cells from 1
            int currentRow = 1;

            ws.Cells[currentRow, 1] = "ID";
            ws.Cells[currentRow, 2] = "Ref";
            ws.Cells[currentRow, 3] = "CatalogueItem";
            ws.Cells[currentRow, 4] = "Ticket";
            ws.Cells[currentRow, 5] = "RAG";
            ws.Cells[currentRow, 6] = "Dataset";
            ws.Cells[currentRow, 7] = "Reported";
            ws.Cells[currentRow, 8] = "Reported Date";
            ws.Cells[currentRow, 9] = "Issue";
            ws.Cells[currentRow, 10] = "Owner";
            ws.Cells[currentRow, 11] = "Action";
            ws.Cells[currentRow, 12] = "Status";
            ws.Cells[currentRow, 13] = "NotesToResearcher";

            foreach (CatalogueItemIssue issue in Issues.Where(i=>i.Status != IssueStatus.Resolved))
            {
                currentRow++;
                AddIssueToSpreadsheet(wb, ws, issue, currentRow);
                
            }
            foreach (CatalogueItemIssue issue in Issues.Where(i => i.Status == IssueStatus.Resolved))
            {
                currentRow++;
                AddIssueToSpreadsheet(wb, ws, issue, currentRow);
               
            }


            //reverse worksheet order - each step put the end one on the before the array idx i
            for (int i = 0; i < wb.Sheets.Count; i++)
                wb.Sheets[wb.Sheets.Count].Move(wb.Sheets[i+1]);

            ws.Move(wb.Sheets[1]);
            ws.Columns.AutoFit();
            ws.Rows.AutoFit();

            xlApp.Visible = true;
            xlApp.UserControl = true;

            foreach (string f in _filesToCleanup)
            {
                File.Delete(f);
            }
        }

        private void AddIssueToSpreadsheet(Workbook wb, Worksheet ws, CatalogueItemIssue issue, int currentRow)
        {
            var cataItem = issue.CatalogueItem;
            var cata = cataItem.Catalogue;

            ws.Cells[currentRow, 1] = issue.ID;
            ws.Cells[currentRow, 2] = AddReferenceSheet(wb,ws,issue.PathToExcelSheetWithAdditionalInformation,issue.Name);
            ws.Cells[currentRow, 3] = cataItem.Name;
            ws.Cells[currentRow, 4] = issue.Ticket;
            ws.Cells[currentRow, 5] = issue.Severity.ToString();

            switch (issue.Severity)
            {
                case IssueSeverity.Red:
                    ws.Range["E" + currentRow].Font.Color = ColorTranslator.ToOle(Color.Red);
                    break;
                case IssueSeverity.Amber:
                    ws.Range["E" + currentRow].Font.Color = ColorTranslator.ToOle(Color.Orange);
                    break;
                case IssueSeverity.Green:
                    ws.Range["E" + currentRow].Font.Color = ColorTranslator.ToOle(Color.Green);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ws.Cells[currentRow, 6] = cata.Name; //catalogue name 
            ws.Cells[currentRow, 7] = issue.GetReportedByName();
            ws.Cells[currentRow, 8] = issue.ReportedOnDate;
            ws.Cells[currentRow, 9] = issue.Name;
            ws.Cells[currentRow, 10] = issue.GetOwnerByName();
            ws.Cells[currentRow, 11] = issue.Action;
            ws.Cells[currentRow, 12] = issue.Status.ToString();
            ws.Cells[currentRow, 13] = issue.NotesToResearcher;
        }

        List<string> _filesToCleanup = new List<string>();

        private string AddReferenceSheet(Workbook wb, Worksheet ws, string path, string issueName)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            if (!File.Exists(path))
                return "MISSING FILE";

            Workbook workBookToImportFrom;

            //if we need to change it into an xlsx
            if (path.EndsWith(".xls"))
            {
                Workbook wbAsXls = xlApp.Workbooks.Open(path,false,true);

                if (!Directory.Exists(@"c:\temp"))
                    Directory.CreateDirectory(@"C:\temp");
                
                string saveName = @"C:\temp\" + wbAsXls.Name.Replace(".xls",".xlsx");

                if (File.Exists(saveName))
                    File.Delete(saveName);

                //should now be in xlsx format
                wbAsXls.SaveAs(saveName, wb.FileFormat);
                wbAsXls.Close(false,false,false);

                _filesToCleanup.Add(saveName);

                workBookToImportFrom = xlApp.Workbooks.Open(saveName, false, true);
            }
            else
                 workBookToImportFrom = xlApp.Workbooks.Open(path,false,true);

            //will be something like "Ref P3, Ref P4"
            string referenceString = "";

            for (int i = 0; i < workBookToImportFrom.Sheets.Count; i++)
            {
                numberOfReferencedSheetsCreated++;
                workBookToImportFrom.Worksheets[i+1].Copy(wb.Worksheets[1]);

                //import just before end
                Worksheet imported = wb.Worksheets[1];
                imported.Name = "Ref P" + numberOfReferencedSheetsCreated;
                referenceString += imported.Name + ",";

            }

            referenceString = referenceString.TrimEnd(',');
            workBookToImportFrom.Close(false);

            return referenceString;
        }
        */
    }
}