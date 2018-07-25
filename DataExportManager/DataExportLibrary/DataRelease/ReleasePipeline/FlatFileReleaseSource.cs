using System;
using System.IO;
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
        protected override ReleaseAudit GetChunkImpl(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            return flowData ?? new ReleaseAudit()
            {
                SourceGlobalFolder = PrepareSourceGlobalFolder()
            };
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

        protected override DirectoryInfo PrepareSourceGlobalFolder()
        {
            if (_releaseData.ReleaseGlobals)
                return base.PrepareSourceGlobalFolder();

            return null;
        }
    }
}