using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using FAnsi.Discovery;
using Fansi.Implementations.MicrosoftSQL;
using LoadModules.Generic.Checks;
using Microsoft.Office.Interop.Excel;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataProvider.FlatFileManipulation
{
    /// <summary>
    /// DLE component which converts Microsoft Excel Workbooks into CSV files.  Workbooks can have multiple worksheets in which case 1 csv will be created for
    /// each worksheet.  Uses Interop to SaveAs csv format so runs faster for large / complex workbooks than using an ExcelAttacher.
    /// </summary>
    public class ExcelToCSVFilesConverter: IPluginDataProvider
    {
        [DemandsInitialization("Pattern to match Excel files in forLoading directory", Mandatory = true)]
        public string ExcelFilePattern { get; set; }

        [DemandsInitialization("Optional,if populated will only extract sheets that match the pattern e.g. '.*data$' will only extract worksheets whose names end with data")]
        public Regex WorksheetPattern { get; set; }

        [DemandsInitialization("Normally a workbook called 'mywb.xlsx' with 2 worksheets 'sheet1' and 'sheet2' will produce csv files called 'sheet1.csv' and 'sheet2.csv'.  Setting this to true will add the workbook name as a prefix 'mywb_sheet1.csv' and 'mywb_sheet2.csv'",defaultValue:false)]
        public bool PrefixWithWorkbookName { get; set; }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            var excelChecks = new ExcelInstalledChecker();
            excelChecks.Check(notifier);

            if (string.IsNullOrWhiteSpace(ExcelFilePattern))
                notifier.OnCheckPerformed(new CheckEventArgs("Argument ExcelFilePattern has not been specified", CheckResult.Fail));
        }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            
            Application excelApp = new Application();
            excelApp.Visible = false;
            excelApp.Interactive = false;
            excelApp.ScreenUpdating = false;
            excelApp.DisplayAlerts = false;

            bool foundAtLeastOne = false;

            foreach (FileInfo f in job.HICProjectDirectory.ForLoading.GetFiles(ExcelFilePattern))
            {
                foundAtLeastOne = true;
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to process file " + f.Name));
                ProcessFile(f,job,excelApp);
            }

            if(!foundAtLeastOne)
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Did not find any files matching Pattern '" + ExcelFilePattern+"' in directory '" + job.HICProjectDirectory.ForLoading.FullName+"'"));

            excelApp.DisplayAlerts = false;
            excelApp.Quit();
            
            return ExitCodeType.Success;
        }

        private void ProcessFile(FileInfo fileInfo, IDataLoadJob job, Application excelApp)
        {
            Workbook wb = excelApp.Workbooks.Open(fileInfo.FullName);
          
            foreach (Worksheet w in wb.Worksheets.Cast<Worksheet>())
            {
                if (IsWorksheetNameMatch(w.Name))
                {
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Started processing worksheet:" + w.Name));


                    string newName = PrefixWithWorkbookName
                        ? Path.GetFileNameWithoutExtension(fileInfo.FullName) + "_" + w.Name
                        : w.Name;

                    //make it sensible
                    newName = new MicrosoftQuerySyntaxHelper().GetSensibleTableNameFromString(newName) + ".csv";

                    string savePath = Path.Combine(job.HICProjectDirectory.ForLoading.FullName, newName);

                    w.SaveAs(savePath, XlFileFormat.xlCSVWindows);
                    
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Saved worksheet as "  + newName));

                }
                else
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Ignoring worksheet:" + w.Name));
            }

            wb.Close(false);
        }

        private bool IsWorksheetNameMatch(string name)
        {
            if (WorksheetPattern == null)
                return true;

            return WorksheetPattern.IsMatch(name);
        }
    }
}
