using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Ticketing;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Release pipeline must start with a Fixed Source that will run checks and prepare the source folder.
    /// Extraction Destinations will return an implementation of this class based on the extraction method used.
    /// </summary>
    /// <typeparam name="T">The type which is passed around in the pipeline</typeparam>
    public abstract class FixedReleaseSource<T> : ICheckable, IPipelineRequirement<ReleaseData>, IDataFlowSource<T> where T : ReleaseAudit
    {
        protected readonly T flowData;
        protected ReleaseData _releaseData;
        protected bool firstTime = true;

        public FixedReleaseSource(T flowData = null)
        {
            this.flowData = flowData;
        }

        public T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (firstTime)
            {
                firstTime = false;
                Check(new FromDataLoadEventListenerToCheckNotifier(listener), true);
                return GetChunkImpl(listener, cancellationToken);
            }

            return null;
        }

        protected abstract T GetChunkImpl(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);

        public abstract void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);

        public abstract void Abort(IDataLoadEventListener listener);
        
        public T TryGetPreview()
        {
            return null;
        }

        public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
        {
            _releaseData = value;
        }

        private void Check(ICheckNotifier notifier, bool isRunTime)
        {
            if (_releaseData.IsDesignTime)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Stale datasets will be checked at runtime...", CheckResult.Success));
                return;
            }

            if (isRunTime)
            {
                var allPotentials = _releaseData.ConfigurationsForRelease.SelectMany(c => c.Value).ToList();
                var staleDatasets = allPotentials.Where(
                       p => p.DatasetExtractionResult.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted).ToArray();

                if (staleDatasets.Any())
                    throw new Exception(
                        "The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were (" +
                        string.Join(",", staleDatasets.Select(ds => ds.ToString())) + ").  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");

                if (_releaseData.ConfigurationsForRelease.Any(kvp => kvp.Value.OfType<NoReleasePotential>().Any()))
                    throw new Exception("There are DataSets with NoReleasePotential in the ReleaseData");

                foreach (var releasePotentials in allPotentials)
                    releasePotentials.Check(notifier);

                //make sure everything is releasable
                var dodgyStates = _releaseData.ConfigurationsForRelease.Where(
                    kvp =>
                        kvp.Value.Any(p =>
                        {
                            var dsReleasability = p.Assessments[p.DatasetExtractionResult];
                            return dsReleasability != Releaseability.Releaseable &&
                                   dsReleasability != Releaseability.ColumnDifferencesVsCatalogue;
                        })).ToArray();

                if (dodgyStates.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp in dodgyStates)
                    {
                        sb.AppendLine(kvp.Key + ":");
                        foreach (var releasePotential in kvp.Value)
                            sb.AppendLine("\t" + releasePotential.Configuration.Name + " : " + releasePotential.DatasetExtractionResult);
                    }

                    throw new Exception("Attempted to release a dataset that was not evaluated as being releaseable. The following Release Potentials were at a dodgy state:" + sb);
                }

                foreach (var environmentPotential in _releaseData.EnvironmentPotentials.Values)
                {
                    environmentPotential.Check(notifier);
                    if (environmentPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable && environmentPotential.Assesment != TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
                        throw new Exception("Ticketing system decided that the Environment is not ready for release. Reason: " + environmentPotential.Reason);
                }
            }

            var projects = _releaseData.ConfigurationsForRelease.Keys.Select(cfr => cfr.Project_ID).Distinct().ToList();
            if (projects.Count() != 1)
                throw new Exception("How is it possible that you are doing a release for multiple different projects?");

            if (_releaseData.ConfigurationsForRelease.Any(kvp => kvp.Key.Project_ID != projects.First()))
                throw new Exception("Mismatch between project passed into constructor and DoRelease projects");

            RunSpecificChecks(notifier);
        }

        public void Check(ICheckNotifier notifier)
        {
            Check(notifier, false);
        }

        protected abstract void RunSpecificChecks(ICheckNotifier notifier);

        protected virtual DirectoryInfo PrepareSourceGlobalFolder()
        {
            var globalDirectoriesFound = new List<DirectoryInfo>();

            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> releasePotentials in _releaseData.ConfigurationsForRelease)
                globalDirectoriesFound.AddRange(GetAllGlobalFolders(releasePotentials));

            if (globalDirectoriesFound.Any())
            {
                var firstGlobal = globalDirectoriesFound.First();

                foreach (var directoryInfo in globalDirectoriesFound.Distinct(new DirectoryInfoComparer()))
                {
                    UsefulStuff.GetInstance().ConfirmContentsOfDirectoryAreTheSame(firstGlobal, directoryInfo);
                }

                return firstGlobal;
            }

            return null;
        }

        protected IEnumerable<DirectoryInfo> GetAllGlobalFolders(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            const string folderName = ExtractionDirectory.GLOBALS_DATA_NAME;

            foreach (ReleasePotential releasePotential in toRelease.Value)
            {
                Debug.Assert(releasePotential.ExtractDirectory.Parent != null, "releasePotential.ExtractDirectory.Parent != null");
                DirectoryInfo globalFolderForThisExtract = releasePotential.ExtractDirectory.Parent.EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly).SingleOrDefault();

                if (globalFolderForThisExtract == null) //this particualar release didn't include globals/custom data at all
                    continue;

                yield return (globalFolderForThisExtract);
            }
        }
    }
}
