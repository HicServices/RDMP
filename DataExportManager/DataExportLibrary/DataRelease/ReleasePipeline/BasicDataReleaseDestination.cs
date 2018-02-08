using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
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

        public ReleaseData CurrentRelease { get; set; }
        private Project _project;
        private DirectoryInfo _destinationFolder;
        private ReleaseEngine _engine;

        public DirectoryInfo ReleaseFolder { get; set; }

        public ReleaseAudit ProcessPipelineData(ReleaseAudit currentRelease, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (CurrentRelease.ReleaseState == ReleaseState.DoingPatch)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "CumulativeExtractionResults for datasets not included in the Patch will now be erased."));
                    
                int recordsDeleted = 0;

                foreach (var configuration in this.CurrentRelease.ConfigurationsForRelease.Keys)
                {
                    IExtractionConfiguration current = configuration;
                    var currentResults = configuration.CumulativeExtractionResults;
                
                    //foreach existing CumulativeExtractionResults if it is not included in the patch then it should be deleted
                    foreach (var redundantResult in currentResults.Where(r => CurrentRelease.ConfigurationsForRelease[current].All(rp => rp.DataSet.ID != r.ExtractableDataSet_ID)))
                    {
                        redundantResult.DeleteInDatabase();
                        recordsDeleted++;
                    }
                }
                
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleted " + recordsDeleted + " old CumulativeExtractionResults (That were not included in the final Patch you are preparing)"));
            }

            _engine = new ReleaseEngine(_project, ReleaseSettings, listener, ReleaseFolder);

            _engine.DoRelease(CurrentRelease.ConfigurationsForRelease, CurrentRelease.EnvironmentPotential, isPatch: CurrentRelease.ReleaseState == ReleaseState.DoingPatch);

            _destinationFolder = _engine.ReleaseFolder;

            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if (pipelineFailureExceptionIfAny != null && CurrentRelease != null)
            {
                try
                {
                    int remnantsDeleted = 0;

                    foreach (ExtractionConfiguration configuration in CurrentRelease.ConfigurationsForRelease.Keys)
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
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Data release succeded into:" + _destinationFolder));
        }

        public void Abort(IDataLoadEventListener listener)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "This component cannot Abort!"));
        }

        public void Check(ICheckNotifier notifier)
        {
            ((ICheckable)ReleaseSettings).Check(notifier);
            PrepareAndCheckReleaseFolder(notifier);
        }

        public void PreInitialize(Project value, IDataLoadEventListener listener)
        {
            _project = value;
        }

        public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
        {
            this.CurrentRelease = value;
        }

        private void PrepareAndCheckReleaseFolder(ICheckNotifier notifier)
        {
            if (CurrentRelease.IsDesignTime)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Release folder will be checked at runtime...", CheckResult.Success));
                return;
            }

            if (ReleaseSettings.CustomReleaseFolder != null && !String.IsNullOrWhiteSpace(ReleaseSettings.CustomReleaseFolder.FullName))
            {
                ReleaseFolder = ReleaseSettings.CustomReleaseFolder;
            }
            else
            {
                ReleaseFolder = GetFromProjectFolder();// new DirectoryInfo(_project.ExtractionDirectory);
            }

            if (ReleaseFolder.Exists)
            {
                if (notifier.OnCheckPerformed(new CheckEventArgs("Release folder exists", CheckResult.Warning, null, "You should delete it or the release will fail.")))
                {
                    ReleaseFolder.Delete(true);
                    notifier.OnCheckPerformed(new CheckEventArgs("Cleaned non-empty existing release folder: " + ReleaseFolder.FullName,
                                                                 CheckResult.Success));
                }
                else
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Intended release directory was existing and I was forbidden to delete it: " + ReleaseFolder.FullName,
                                                                 CheckResult.Fail));
                    return;
                }
            }

            if (ReleaseSettings.CreateReleaseDirectoryIfNotFound)
                ReleaseFolder.Create();
            else
                throw new Exception("Intended release directory was not found and I was forbidden to create it: " +
                                    ReleaseFolder.FullName);
        }

        private DirectoryInfo GetFromProjectFolder()
        {
            if (string.IsNullOrWhiteSpace(_project.ExtractionDirectory))
                return null;

            var prefix = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string suffix = String.Empty;
            if (CurrentRelease.ConfigurationsForRelease.Keys.Any())
            {
                var releaseTicket = CurrentRelease.ConfigurationsForRelease.Keys.First().ReleaseTicket;
                if (CurrentRelease.ConfigurationsForRelease.Keys.All(x => x.ReleaseTicket == releaseTicket))
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