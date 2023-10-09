using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
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

    [DemandsInitialization("Write the holdout data to disk.")]
    public string holdoutStorageLocation { get; set; }


    public IExtractDatasetCommand Request { get; private set; }

    private int getHoldoutRowCount(DataTable toProcess, IDataLoadEventListener listener)
    {
        int rowCount = holdoutCount;
        if (rowCount >= toProcess.Rows.Count && !isPercentage)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "More holdout data was requested than there is available data. All data will be held back"));
            rowCount = toProcess.Rows.Count;
        }
        if (isPercentage)
        {
            if (holdoutCount > 100)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Holdout percentage was >100%. Will use 100%"));
                holdoutCount = 100;
            }
            rowCount = toProcess.Rows.Count / 100 * holdoutCount;
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
        String filename = Request.ToString();
        File.WriteAllText($"{holdoutStorageLocation}/holdout_{filename}.csv", sb.ToString());
    }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (toProcess.Rows.Count == 0)
        {
            return toProcess;
        }
        DataTable holdoutData = toProcess.Clone();
        int holdoutCount = getHoldoutRowCount(toProcess, listener);
        var rand = new Random();
        holdoutData.BeginLoadData();
        toProcess.BeginLoadData();
        var rowsToMove = toProcess.AsEnumerable().OrderBy(r => rand.Next()).Take(holdoutCount);
        foreach (DataRow row in rowsToMove)
        {
            holdoutData.ImportRow(row);
            row.Delete();
        }
        holdoutData.EndLoadData();
        toProcess.EndLoadData();
        //todo need to sdo something with the holdoutCount
        //maybe reimport it as a catalog?
        if (holdoutStorageLocation is not null && holdoutStorageLocation.Length > 0)
        {
            //write this data somewhere
            writeDataTabletoCSV(holdoutData);
        }
        {

        }
        Catalogue x = new Catalogue();
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
