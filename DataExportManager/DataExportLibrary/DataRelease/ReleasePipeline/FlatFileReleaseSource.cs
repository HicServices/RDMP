using System;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Prepares the Source Global Folder for the ReleaseEngine.
    /// </summary>
    /// <typeparam name="T">The ReleaseAudit object passed around in the pipeline</typeparam>
    public class FlatFileReleaseSource<T> : FixedReleaseSource<ReleaseAudit>
    {
        private bool firstTime = true;

        public override ReleaseAudit GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (firstTime)
            {
                firstTime = false;
                return flowData ?? new ReleaseAudit()
                {
                    SourceGlobalFolder = PrepareSourceGlobalFolder()
                };
            }

            return null;
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            firstTime = true;
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            firstTime = true;
        }

        protected override void RunSpecificChecks(ICheckNotifier notifier)
        {
            
        }
    }
}