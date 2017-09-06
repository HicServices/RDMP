using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents.SingleControlForms;

namespace DataExportManager.DataRelease.PipelineSource
{
    public class InteractiveDataReleaseSource : IPluginDataFlowSource<FileInfo[]>, IPipelineRequirement<IProject>, IPipelineRequirement<IActivateItems>
    {
        private IActivateItems _activator;
        private IProject _project;
        private bool isFirstTime = true;

        public FileInfo[] GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (!isFirstTime)
                return null;

            isFirstTime = false;

            var ui = new DataReleaseUI(_activator,(Project)_project);
            if (ui.ShowDialog() == DialogResult.OK)
                return ui.FinalFiles;
            
            throw new Exception("User cancelled executing the Release");
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
            
        }

        public void PreInitialize(IProject value, IDataLoadEventListener listener)
        {
            _project = value;
        }

        public void PreInitialize(IActivateItems value, IDataLoadEventListener listener)
        {
            _activator = value;
        }
    }
}
