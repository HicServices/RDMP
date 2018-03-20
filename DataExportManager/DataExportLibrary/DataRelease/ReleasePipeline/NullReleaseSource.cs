using System;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class NullReleaseSource<T> : FixedReleaseSource<ReleaseAudit>
    {
        public override ReleaseAudit GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        protected override void RunSpecificChecks(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Fixed Release Source";
        }
    }
}