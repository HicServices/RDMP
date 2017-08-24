using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using LoadModules.Generic.Checks;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.FileOperations
{
    [Description("Converts between any file format openable by Excel into comma separated format")]
    public class ExcelToCsvConverter : IPluginDataProvider
    {
        [DemandsInitialization("Set this to a pattern (e.g. *.xlsx) for the files you wish to be converted - in the forLoading directory", DemandType.Unspecified, "*.xls*", Mandatory = true)]
        public string FilePatternToConvert { get; set; }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            return Fetch(job.HICProjectDirectory,job, cancellationToken);
        }

        public ExitCodeType Fetch(IHICProjectDirectory directory, IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
            FileInfo[] filesToConvert = directory.ForLoading.GetFiles(FilePatternToConvert);

            if (!filesToConvert.Any())
                throw new Exception("Could not find any files matching extension " + FilePatternToConvert);

            foreach (var fileToConvert in directory.ForLoading.GetFiles(FilePatternToConvert))
                ConvertFile(fileToConvert,job);

            return ExitCodeType.Success;
        }

        private void ConvertFile(FileInfo fileToConvert, IDataLoadEventListener job)
        {
            object oFalse = false;
            object oMissing = null;

            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            try
            {
                Stopwatch s = new Stopwatch();

                Microsoft.Office.Interop.Excel.Workbook workBook = app.Workbooks.Open(fileToConvert.FullName, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                
                for (int i = 0; i < workBook.Worksheets.Count; i++)
                {
                    s.Start();

                    Microsoft.Office.Interop.Excel.Worksheet workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.Sheets[i + 1];

                    string destinationForSheet = fileToConvert.FullName.Replace(fileToConvert.Extension, "_Sheet" + (i+1) + ".csv");

                    workSheet.SaveAs(destinationForSheet, Microsoft.Office.Interop.Excel.XlFileFormat.xlCSV);

                    
                   FileInfo fileCreated = new FileInfo(destinationForSheet);
                   job.OnProgress(this,new ProgressEventArgs(destinationForSheet,new ProgressMeasurement((int)(fileCreated.Length/1000),ProgressType.Kilobytes),s.Elapsed));
                    
                    
                    s.Stop();
                    s.Reset();

                }

                //close without saving
                workBook.Close(oFalse, oMissing,oMissing);


            }
            finally
            {
                app.DisplayAlerts = false;
                app.Quit();
            }
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
        }

        
        public void Check(ICheckNotifier notifier)
        {
            var installedChecker = new ExcelInstalledChecker();
            installedChecker.Check(notifier);
        }
    }
}
