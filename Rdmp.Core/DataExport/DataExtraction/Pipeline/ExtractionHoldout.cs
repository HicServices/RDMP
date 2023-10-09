using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
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

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (toProcess.Rows.Count == 0)
        {
            return toProcess;
        }
        DataTable holdoutData = toProcess.Clone();
        int holdoutCount = getHoldoutRowCount(toProcess, listener);
        var rand = new Random();
        Console.WriteLine("Hello world!");
        var rowsToMove = toProcess.AsEnumerable().OrderBy(r => rand.Next()).Take(holdoutCount);
        foreach (DataRow row in rowsToMove)
        {
            holdoutData.ImportRow(row);
            row.Delete();
        }
        holdoutData.AcceptChanges();
        toProcess.AcceptChanges();

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
