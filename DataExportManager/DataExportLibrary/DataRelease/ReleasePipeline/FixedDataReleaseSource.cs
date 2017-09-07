using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class FixedDataReleaseSource : IPluginDataFlowSource<FileInfo[]>
    {
        private bool firstTime = true;

        public HashSet<FileInfo> FilesToRelease { get; set; }

        public FixedDataReleaseSource()
        {
            FilesToRelease = new HashSet<FileInfo>();
        }

        public FileInfo[] GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if(firstTime)
            {
                firstTime = false;
                return FilesToRelease.ToArray();
            }

            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            firstTime = true;
        }

        public void Abort(IDataLoadEventListener listener)
        {   
        }

        public FileInfo[] TryGetPreview()
        {
            return null;
        }

        public void Check(ICheckNotifier notifier)
        {
        }
    }
}
