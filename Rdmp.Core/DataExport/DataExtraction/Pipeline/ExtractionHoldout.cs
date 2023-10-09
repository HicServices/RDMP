using NPOI.SS.Formula.Functions;
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

    [DemandsInitialization("The column that the befor and after date options use to filter holdout data")]
    public String dateColumn { get; set; }

    // We may want to automatically reimport into RDMP, but this is quite complicated. It may be worth having users reimport the catalogue themself until it is proven that this is worth building.
    //Currently only support writting holdback data to a CSV

    //TODO - force date range
    //TODO - force specific holdback criteria

    public IExtractDatasetCommand Request { get; private set; }


    private DataTable FilterableData { get; set; } //Rows that are valid as holdout data based on the user filters

    private bool validateIfRowShouldBeFiltered(DataRow row)
    {
        if (!string.IsNullOrEmpty(dateColumn))
        {
            //had a data column
            DateTime dateCell = DateTime.Parse(row.Field<string>(dateColumn), CultureInfo.InvariantCulture);
            if (afterDate != DateTime.MinValue)
            {
                //has date
                if (dateCell <= afterDate)
                {
                    return false;
                }
            }
            if (beforeDate != DateTime.MinValue)
            {
                //has date
                if (dateCell >= beforeDate)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private DataTable filterRowsBasedOnHoldoutDates(DataTable toProcess)
    {
        DataTable filteredTable = toProcess.AsEnumerable().Where(row => validateIfRowShouldBeFiltered(row)).CopyToDataTable();
        return filteredTable;
    }

    private int getHoldoutRowCount(DataTable toProcess, IDataLoadEventListener listener)
    {

        int rowCount = holdoutCount;
        if (rowCount >= FilterableData.Rows.Count && !isPercentage)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "More holdout data was requested than there is available data. All valid data will be held back"));
            rowCount = FilterableData.Rows.Count;
        }
        if (isPercentage)
        {
            if (holdoutCount > 100)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Holdout percentage was >100%. Will use 100%"));
                holdoutCount = 100;
            }
            rowCount = FilterableData.Rows.Count / 100 * holdoutCount;
        }
        return rowCount;
    }

    public void PreInitialize(IExtractCommand request, IDataLoadEventListener listener)
    {
        Request = request as IExtractDatasetCommand;

        // We only care about dataset extraction requests
        if (Request == null)
            return;
        //tod need to do a bunch of error handling

    }

    private void writeDataTabletoCSV(DataTable dt)
    {
        StringBuilder sb = new StringBuilder();

        IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                          Select(column => column.ColumnName);
        sb.AppendLine(string.Join(",", columnNames));

        foreach (DataRow row in dt.Rows)
        {
            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
            sb.AppendLine(string.Join(",", fields));
        }
        string filename = Request.ToString();
        holdoutStorageLocation.TrimEnd('/');
        holdoutStorageLocation.TrimEnd('\\');
        //todo this isn't the correct filename
        File.WriteAllText($"{holdoutStorageLocation}/holdout_{filename}.csv", sb.ToString());
    }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (toProcess.Rows.Count == 0)
        {
            return toProcess;
        }
        FilterableData = toProcess;
        if (dateColumn is not null && (afterDate != DateTime.MinValue || beforeDate != DateTime.MinValue))
        {
            FilterableData = filterRowsBasedOnHoldoutDates(toProcess);
        }

        DataTable holdoutData = toProcess.Clone();
        int holdoutCount = getHoldoutRowCount(toProcess, listener);
        var rand = new Random();
        holdoutData.BeginLoadData();
        toProcess.BeginLoadData();

        var rowsToMove = FilterableData.AsEnumerable().OrderBy(r => rand.Next()).Take(holdoutCount);
        foreach (DataRow row in rowsToMove)
        {
            holdoutData.ImportRow(row);
            toProcess.Rows.Remove(row);
            //row.Delete();
        }
        holdoutData.EndLoadData();
        toProcess.EndLoadData();
        if (holdoutStorageLocation is not null && holdoutStorageLocation.Length > 0)
        {
            writeDataTabletoCSV(holdoutData);
        }
        return toProcess;
    }

    public void Check(ICheckNotifier notifier)
    {
        //todo
    }
    public void Abort(IDataLoadEventListener listener)
    {
    }
    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }
}
