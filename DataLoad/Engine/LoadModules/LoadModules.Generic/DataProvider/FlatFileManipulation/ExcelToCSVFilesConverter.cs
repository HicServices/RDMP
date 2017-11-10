using System;
using System.Collections.Generic;
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
using LoadModules.Generic.Checks;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataProvider.FlatFileManipulation
{
    public class ExcelToCSVFilesConverter: IPluginDataProvider
    {

        [DemandsInitialization("Pattern to match Excel files in forLoading directory", Mandatory = true)]
        public string ExcelFilePattern { get; set; }

        [DemandsInitialization("Optional,if populated will only extract sheets that match the pattern e.g. '.*data$' will only extract worksheets whose names end with data")]
        public Regex WorksheetPattern { get; set; }

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

            foreach (FileInfo f in job.HICProjectDirectory.ForLoading.GetFiles(ExcelFilePattern))
            {
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to process file " + f.Name));
                ProcessFile(f,job,excelApp);
            }

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
                    
                    var newName = SqlSyntaxHelper.GetSensibleTableNameFromString(w.Name) + ".csv";

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

        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        public IDataProvider Clone()
        {
            throw new NotImplementedException();
        }

        public bool Validate(IHICProjectDirectory destination)
        {
            return true;
        }
    }
}
