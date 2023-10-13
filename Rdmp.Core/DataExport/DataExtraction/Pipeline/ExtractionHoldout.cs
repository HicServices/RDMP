using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline;

public class ExtractionHoldout : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<IExtractCommand>
{
    [DemandsInitialization("The % of the data you want to be kelp as holdout data")]
    public int holdoutCount { get; set; }

    [DemandsInitialization("Use a % as holdout value. If unselected, the actual number will be used.")]
    public bool isPercentage { get; set; }

    [DemandsInitialization("Write the holdout data to disk. Leave blank if you don't want it exported somewhere")]
    public string holdoutStorageLocation { get; set; }

    [DemandsInitialization("Set this value to only select data for holdout that is before this date")]
    public DateTime beforeDate { get; set; }

    [DemandsInitialization("Set this value to only select data for holdout that is after this date")]
    public DateTime afterDate { get; set; }

    [DemandsInitialization("The column that the before and after date options use to filter holdout data")]
    public string dateColumn { get; set; }

    //can only filter on strings, not dates
    [DemandsInitialization("Allows for the filtering of what data can be used as holdout data. The filter only currently supports filtering on string columns, not dates. Filter References https://learn.microsoft.com/en-us/dotnet/api/system.data.dataview.rowfilter?view=net-7.0 and https://learn.microsoft.com/en-us/dotnet/api/system.data.datacolumn.expression?view=net-7.0")]
    public string whereCondition { get; set; }

    [DemandsInitialization("Overrides any data in the holdout file with new data")]
    public bool overrideFile { get; set; }


    // We may want to automatically reimport into RDMP, but this is quite complicated.
    // It may be worth having users reimport the catalogue themself until it is proven that this is worth building.
    //Currently only support writting holdback data to a CSV


    private readonly string holdoutColumnName = "_isValidHoldout";


    public IExtractDatasetCommand Request { get; private set; }


    private bool validateIfRowShouldBeFiltered(DataRow row, DataTable toProcess)
    {
        if (!string.IsNullOrWhiteSpace(dateColumn))
        {
            //had a data column
            DateTime dateCell;
            try
            {
                dateCell = row.Field<DateTime>(dateColumn);
            }
            catch (Exception)
            {
                dateCell = DateTime.Parse(row.Field<string>(dateColumn), CultureInfo.InvariantCulture);
            }

            if (afterDate != DateTime.MinValue && dateCell <= afterDate)
            {
                //has date
                return false;
            }
            if (beforeDate != DateTime.MinValue && dateCell >= beforeDate)
            {
                //has date
                return false;
            }
        }
        if (!string.IsNullOrWhiteSpace(whereCondition))
        {
            DataTable dt = toProcess.Clone();
            dt.ImportRow(row);
            DataView dv = new DataView(dt);
            dv.RowFilter = whereCondition;
            DataTable dt2 = dv.ToTable();
            dv.Dispose();
            if (dt2.Rows.Count < 1)
            {
                return false;
            }
        }
        return true;
    }

    private void filterRowsBasedOnHoldoutDates(DataTable toProcess)
    {
        toProcess.Columns.Add(holdoutColumnName, typeof(bool));
        foreach (DataRow row in toProcess.Rows)
        {
            row[holdoutColumnName] = validateIfRowShouldBeFiltered(row, toProcess);
        }
    }

    private int getHoldoutRowCount(DataTable toProcess, IDataLoadEventListener listener)
    {

        float rowCount = (float)holdoutCount;
        if (rowCount >= toProcess.Rows.Count && !isPercentage)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "More holdout data was requested than there is available data. All valid data will be held back"));
            rowCount = toProcess.Rows.Count;
        }
        if (isPercentage)
        {
            if (holdoutCount > 100)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Holdout percentage was >100%. Will use 100%"));
                holdoutCount = 100;
            }
            rowCount = (float)toProcess.Rows.Count / 100 * holdoutCount;
        }
        return (int)Math.Ceiling(rowCount);
    }

    public void PreInitialize(IExtractCommand request, IDataLoadEventListener listener)
    {
        Request = request as IExtractDatasetCommand;

        if (Request == null)
            return;
    }

    private void writeDataTabletoCSV(DataTable dt)
    {
        StringBuilder sb = new();
        string filename = Request.ToString();
        string path = $"{holdoutStorageLocation}/holdout_{filename}.csv";
        IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        if (overrideFile || !File.Exists(path))
        {
            sb.AppendLine(string.Join(",", columnNames));
        }

        foreach (DataRow row in dt.Rows)
        {
            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
            sb.AppendLine(string.Join(",", fields));
        }
        holdoutStorageLocation.TrimEnd('/');
        holdoutStorageLocation.TrimEnd('\\');

        if (File.Exists(path) && !overrideFile)
        {

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(sb.ToString());
            }
            return;

        }

        File.WriteAllText(path, sb.ToString());
    }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (toProcess.Rows.Count == 0)
        {
            return toProcess;
        }
        bool toProcessDTModified = false;
        if (dateColumn is not null && (afterDate != DateTime.MinValue || beforeDate != DateTime.MinValue))
        {
            //we only want to check for valid rows if dates are set, otherwise all rows are valid
            filterRowsBasedOnHoldoutDates(toProcess);
            toProcessDTModified = true;
        }

        DataTable holdoutData = toProcess.Clone();
        int foundHoldoutCount = getHoldoutRowCount(toProcess, listener);
        var rand = new Random();
        holdoutData.BeginLoadData();
        toProcess.BeginLoadData();

        var rowsToMove = toProcess.AsEnumerable().Where(row => !toProcessDTModified || row[holdoutColumnName] is true).OrderBy(r => rand.Next()).Take(foundHoldoutCount);
        if (rowsToMove.Count() < 1)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "No valid holdout rows were found. Please check your settings."));

        }
        foreach (DataRow row in rowsToMove)
        {
            holdoutData.ImportRow(row);
            toProcess.Rows.Remove(row);
        }
        holdoutData.EndLoadData();
        toProcess.EndLoadData();

        if (holdoutStorageLocation is not null && holdoutStorageLocation.Length > 0)
        {
            if (toProcessDTModified)
            {
                holdoutData.Columns.Remove(holdoutColumnName);
            }
            writeDataTabletoCSV(holdoutData);
        }
        if (toProcessDTModified)
        {
            toProcess.Columns.Remove(holdoutColumnName);

        }

        return toProcess;
    }

    public void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(holdoutStorageLocation))
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"No holdout file location set.", CheckResult.Fail));
        }
        if (holdoutCount is 0)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"No data holdout count set.", CheckResult.Fail));
        }
    }
    public void Abort(IDataLoadEventListener listener)
    {
    }
    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }
}
