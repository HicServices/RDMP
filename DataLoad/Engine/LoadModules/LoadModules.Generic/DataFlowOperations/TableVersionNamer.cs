using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowOperations
{
    /// <summary>
    /// Renames DataTables in the pipeline so that they do not collide with any tables at the destination database.  This is done by appending V1,V2,V3 etc to the table
    /// </summary>
    public class TableVersionNamer : IPluginDataFlowComponent<DataTable>,IPipelineRequirement<DiscoveredDatabase>
    {
        private string[] _tableNamesAtDestination;

        [DemandsInitialization("Suffix pattern, $v is the version, default is _V$v as in MyTable_V1, MyTable_V2 etc",DemandType.Unspecified,"_V$v")]
        public string Suffix { get; set; }

        [DemandsInitialization("The maximum number of tables to allow, defaults to 1000 so MyTable_V1 up to MyTable_V1000", DemandType.Unspecified, 1000)]
        public int MaximumNumberOfVersionsAllowed { get; set; }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            toProcess.TableName = GetVersionedTableName(toProcess.TableName);
            return toProcess;
        }

        private string GetVersionedTableName(string tableName)
        {
            for (int i = 1; i <= MaximumNumberOfVersionsAllowed; i++)
            {
                string candidate = tableName + Suffix.Replace("$v", i.ToString());
                if (!_tableNamesAtDestination.Any(t => t.Equals(candidate, StringComparison.CurrentCultureIgnoreCase)))
                    return candidate;
            }

            throw new Exception("Unable to find unique table name after " + MaximumNumberOfVersionsAllowed + " TableNames were tried");
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            if (MaximumNumberOfVersionsAllowed == 0)
                notifier.OnCheckPerformed(new CheckEventArgs("MaximumNumberOfVersionsAllowed cannot be 0",CheckResult.Fail));
        }

        public void PreInitialize(DiscoveredDatabase value, IDataLoadEventListener listener)
        {
            _tableNamesAtDestination = value.DiscoverTables(true).Select(t => t.GetRuntimeName()).ToArray();
        }
    }
}
