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
    public class BasicDataReleaseSource : IPluginDataFlowSource<FileInfo[]>,IPipelineRequirement<IProject>
    {
        private IProject _project;


        private bool firstTime = true;

        public FileInfo[] GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if(firstTime)
            {
                firstTime = false;

                return new FileInfo[0];
            }
            
            

            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
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
            notifier.OnCheckPerformed(new CheckEventArgs("About to check releasability of Project '" + _project + "'",
                CheckResult.Success));
        }

        public void PreInitialize(IProject value, IDataLoadEventListener listener)
        {
            _project = value;
        }
    }
}
