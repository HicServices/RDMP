using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Ticketing;

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