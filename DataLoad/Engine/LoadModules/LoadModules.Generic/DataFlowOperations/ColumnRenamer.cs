using System;
using System.ComponentModel;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowOperations
{
    /// <summary>
    /// Pipeline component for renaming a single column in DataTables passing through the component.
    /// <para>Renames a column with a given name to have a new name e.g. 'mCHI' to 'CHI'</para>
    /// </summary>
    public class ColumnRenamer : IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Looks for a column with exactly this name", Mandatory = true)]
        public string ColumnNameToFind { get; set; }

        [DemandsInitialization("Renames the column to this name", Mandatory = true)]
        public string ReplacementName { get; set; }


        [DemandsInitialization("In relaxed mode the pipeline will not be crashed if the column does not appear.  Default is false i.e. the column MUST appear.", Mandatory = true, DefaultValue = false)]
        public bool RelaxedMode { get; set; }



        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (!toProcess.Columns.Contains(ColumnNameToFind))
                if (RelaxedMode)
                    return toProcess;
                else
                    throw new InvalidOperationException("The column to be renamed (" + ColumnNameToFind + ") does not exist in the supplied data table and RelaxedMode is off. Check that this component is configured correctly, or if any upstream components are removing this column unexpectedly.");

            toProcess.Columns[ColumnNameToFind].ColumnName = ReplacementName;

            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(ColumnNameToFind))
                notifier.OnCheckPerformed(new CheckEventArgs("No value specified for argument ColumnNameToFind",CheckResult.Fail));

            if (string.IsNullOrWhiteSpace(ReplacementName))
                notifier.OnCheckPerformed(new CheckEventArgs("No value specified for argument ReplacementName", CheckResult.Fail));
        }
    }
}
