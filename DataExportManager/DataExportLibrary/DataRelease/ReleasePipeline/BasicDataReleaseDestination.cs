using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Default release pipeline destination implementation wraps Release Engine for the supplied ReleaseData.
    /// </summary>
    public class BasicDataReleaseDestination : IPluginDataFlowComponent<ReleaseAudit>, IDataFlowDestination<ReleaseAudit>, IPipelineRequirement<Project>, IPipelineRequirement<ReleaseData>
    {
        [DemandsNestedInitialization()]
        public ReleaseEngineSettings ReleaseSettings { get; set; }

        private ReleaseData _releaseData;
        private Project _project;
        private DirectoryInfo _destinationFolder;
        private ReleaseEngine _engine;
        private List<IExtractionConfiguration> _configurationReleased;

        public ReleaseAudit ProcessPipelineData(ReleaseAudit releaseAudit, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (releaseAudit == null)
                return null;

            if (releaseAudit.ReleaseFolder == null)
                throw new ArgumentException("This component needs a destination folder! Did you forget to introduce and initialize the ReleaseFolderProvider in the pipeline?");

            if (_releaseData.ReleaseState == ReleaseState.DoingPatch)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "CumulativeExtractionResults for datasets not included in the Patch will now be erased."));
                    
                int recordsDeleted = 0;

                foreach (var configuration in this._releaseData.ConfigurationsForRelease.Keys)
                {
                    IExtractionConfiguration current = configuration;
                    var currentResults = configuration.CumulativeExtractionResults;
                
                    //foreach existing CumulativeExtractionResults if it is not included in the patch then it should be deleted
                    foreach (var redundantResult in currentResults.Where(r => _releaseData.ConfigurationsForRelease[current].All(rp => rp.DataSet.ID != r.ExtractableDataSet_ID)))
                    {
                        redundantResult.DeleteInDatabase();
                        recordsDeleted++;
                    }
                }
                
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleted " + recordsDeleted + " old CumulativeExtractionResults (That were not included in the final Patch you are preparing)"));
            }

            _engine = new ReleaseEngine(_project, ReleaseSettings, listener, releaseAudit);

            _engine.DoRelease(_releaseData.ConfigurationsForRelease, _releaseData.EnvironmentPotential, isPatch: _releaseData.ReleaseState == ReleaseState.DoingPatch);

            _destinationFolder = _engine.ReleaseAudit.ReleaseFolder;
            _configurationReleased = _engine.ConfigurationsReleased;

            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if (pipelineFailureExceptionIfAny != null && _releaseData != null)
            {
                try
                {
                    int remnantsDeleted = 0;

                    foreach (ExtractionConfiguration configuration in _releaseData.ConfigurationsForRelease.Keys)
                        foreach (ReleaseLogEntry remnant in configuration.ReleaseLogEntries)
                        {
                            remnant.DeleteInDatabase();
                            remnantsDeleted++;
                        }

                    if (remnantsDeleted > 0)
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Because release failed we are deleting ReleaseLogEntries, this resulted in " + remnantsDeleted + " deleted records, you will likely need to re-extract these datasets or retrieve them from the Release directory"));
                }
                catch (Exception e1)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error occurred when trying to clean up remnant ReleaseLogEntries", e1));
                }
            }

            if(pipelineFailureExceptionIfAny == null && _destinationFolder != null)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Data release succeded into:" + _destinationFolder));
                //mark configuration as released
                if (ReleaseSettings.FreezeReleasedConfigurations)
                {
                    foreach (var config in _configurationReleased)
                    {
                        config.IsReleased = true;
                        config.SaveToDatabase();
                    }
                } 
                if (ReleaseSettings.DeleteFilesOnSuccess)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Cleaning up..."));
                    ExtractionDirectory.CleanupExtractionDirectory(this, _project.ExtractionDirectory, _configurationReleased, listener);
                }

                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "All done!"));
            }
        }

        public void Abort(IDataLoadEventListener listener)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "This component cannot Abort!"));
        }

        public void Check(ICheckNotifier notifier)
        {
            ((ICheckable)ReleaseSettings).Check(notifier);
        }

        public void PreInitialize(Project value, IDataLoadEventListener listener)
        {
            _project = value;
        }

        public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
        {
            this._releaseData = value;
        }
    }
}