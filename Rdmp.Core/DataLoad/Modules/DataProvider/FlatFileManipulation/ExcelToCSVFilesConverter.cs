// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Implementations.MicrosoftSQL;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Extensions;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataProvider.FlatFileManipulation;

/// <summary>
///     DLE component which converts Microsoft Excel Workbooks into CSV files.  Workbooks can have multiple worksheets in
///     which case 1 csv will be created for
///     each worksheet.  Supports both .xls and .xlsx by using NPOI (i.e. not Interop).
/// </summary>
public class ExcelToCSVFilesConverter : IPluginDataProvider
{
    [DemandsInitialization("Pattern to match Excel files in forLoading directory", Mandatory = true)]
    public string ExcelFilePattern { get; set; }

    [DemandsInitialization(
        "Optional,if populated will only extract sheets that match the pattern e.g. '.*data$' will only extract worksheets whose names end with data")]
    public Regex WorksheetPattern { get; set; }

    [DemandsInitialization(
        "Normally a workbook called 'mywb.xlsx' with 2 worksheets 'sheet1' and 'sheet2' will produce csv files called 'sheet1.csv' and 'sheet2.csv'.  Setting this to true will add the workbook name as a prefix 'mywb_sheet1.csv' and 'mywb_sheet2.csv'",
        defaultValue: false)]
    public bool PrefixWithWorkbookName { get; set; }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(ExcelFilePattern))
            notifier.OnCheckPerformed(new CheckEventArgs("Argument ExcelFilePattern has not been specified",
                CheckResult.Fail));
    }

    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var foundAtLeastOne = false;

        foreach (var f in job.LoadDirectory.ForLoading.GetFiles(ExcelFilePattern))
        {
            foundAtLeastOne = true;
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"About to process file {f.Name}"));
            ProcessFile(f, job);
        }

        if (!foundAtLeastOne)
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Did not find any files matching Pattern '{ExcelFilePattern}' in directory '{job.LoadDirectory.ForLoading.FullName}'"));

        return ExitCodeType.Success;
    }

    private void ProcessFile(FileInfo fileInfo, IDataLoadJob job)
    {
        using var fs = new FileStream(fileInfo.FullName, FileMode.Open);
        IWorkbook wb;
        if (fileInfo.Extension == ".xls")
            wb = new HSSFWorkbook(fs);
        else
            wb = new XSSFWorkbook(fs);

        try
        {
            var source = new ExcelDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(fileInfo), job);

            for (var i = 0; i < wb.NumberOfSheets; i++)
            {
                var sheet = wb.GetSheetAt(i);

                if (IsWorksheetNameMatch(sheet.SheetName))
                {
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information,
                            $"Started processing worksheet:{sheet.SheetName}"));

                    var newName = PrefixWithWorkbookName
                        ? $"{Path.GetFileNameWithoutExtension(fileInfo.FullName)}_{sheet.SheetName}"
                        : sheet.SheetName;

                    //make it sensible
                    newName =
                        $"{MicrosoftQuerySyntaxHelper.Instance.GetSensibleEntityNameFromString(newName)}.csv";

                    var savePath = Path.Combine(job.LoadDirectory.ForLoading.FullName, newName);
                    var dt = source.GetAllData(sheet, job);
                    dt.EndLoadData();
                    using var saveStream = new StreamWriter(savePath, false, Encoding.UTF8, 1 << 20);
                    dt.SaveAsCsv(saveStream);

                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information, $"Saved worksheet as {newName}"));
                }
                else
                {
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information,
                            $"Ignoring worksheet:{sheet.SheetName}"));
                }
            }
        }
        finally
        {
            wb.Close();
        }
    }

    private bool IsWorksheetNameMatch(string name)
    {
        return WorksheetPattern?.IsMatch(name) != false;
    }
}