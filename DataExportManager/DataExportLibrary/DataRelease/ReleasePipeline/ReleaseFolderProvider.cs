using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Middle component for preparing the Release Folders for the Release Pipeline.
    /// Some destination components will complain if this is not present!
    /// </summary>
    public class ReleaseFolderProvider : IPluginDataFlowComponent<ReleaseAudit>, IPipelineRequirement<Project>, IPipelineRequirement<ReleaseData>
    {
        private Project _project;
        private ReleaseData _releaseData;
        private DirectoryInfo _releaseFolder;

        [DemandsNestedInitialization()]
        public ReleaseFolderSettings FolderSettings { get; set; }

        public ReleaseAudit ProcessPipelineData(ReleaseAudit releaseAudit, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (releaseAudit == null)
                return null;

            releaseAudit.ReleaseFolder = _releaseFolder;
            return releaseAudit;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Abort(IDataLoadEventListener listener)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "This component cannot Abort!"));
        }

        public void Check(ICheckNotifier notifier)
        {
            ((ICheckable)FolderSettings).Check(notifier);
            PrepareAndCheckReleaseFolder(notifier);
        }

        public void PreInitialize(Project value, IDataLoadEventListener listener)
        {
            _project = value;
        }

        public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
        {
            _releaseData = value;
        }

        private void PrepareAndCheckReleaseFolder(ICheckNotifier notifier)
        {
            if (_releaseData.IsDesignTime)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Release folder will be checked at runtime...", CheckResult.Success));
                return;
            }

            if (FolderSettings.CustomReleaseFolder != null && !String.IsNullOrWhiteSpace(FolderSettings.CustomReleaseFolder.FullName))
            {
                _releaseFolder = FolderSettings.CustomReleaseFolder;
            }
            else
            {
                _releaseFolder = GetFromProjectFolder();// new DirectoryInfo(_project.ExtractionDirectory);
            }

            if (_releaseFolder.Exists)
            {
                if (notifier.OnCheckPerformed(new CheckEventArgs(String.Format("Release folder {0} already exists!", _releaseFolder.FullName), CheckResult.Warning, null, "Do you want to delete it? You should check the contents first.")))
                {
                    _releaseFolder.Delete(true);
                    notifier.OnCheckPerformed(new CheckEventArgs("Cleaned non-empty existing release folder: " + _releaseFolder.FullName,
                                                                 CheckResult.Success));
                }
                else
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Intended release folder was existing and I was forbidden to delete it: " + _releaseFolder.FullName,
                                                                 CheckResult.Fail));
                    return;
                }
            }

            if (FolderSettings.CreateReleaseDirectoryIfNotFound)
                _releaseFolder.Create();
            else
                throw new Exception("Intended release directory was not found and I was forbidden to create it: " +
                                    _releaseFolder.FullName);
        }

        private DirectoryInfo GetFromProjectFolder()
        {
            if (string.IsNullOrWhiteSpace(_project.ExtractionDirectory))
                return null;

            var prefix = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string suffix = String.Empty;
            if (_releaseData.ConfigurationsForRelease.Keys.Any())
            {
                var releaseTicket = _releaseData.ConfigurationsForRelease.Keys.First().ReleaseTicket;
                if (_releaseData.ConfigurationsForRelease.Keys.All(x => x.ReleaseTicket == releaseTicket))
                    suffix = releaseTicket;
                else
                    throw new Exception("Multiple release tickets seen, this is not allowed!");
            }

            if (String.IsNullOrWhiteSpace(suffix))
            {
                if (String.IsNullOrWhiteSpace(_project.MasterTicket))
                    suffix = _project.ID + "_" + _project.Name;
                else
                    suffix = _project.MasterTicket;
            }

            return new DirectoryInfo(Path.Combine(_project.ExtractionDirectory, prefix + "_" + suffix));
        }
    }
}