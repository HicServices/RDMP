using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Ticketing;
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
    public abstract class FixedReleaseSource<T> : ICheckable, IPipelineRequirement<ReleaseData>, IDataFlowSource<T> where T : class, new()
    {
        private readonly Action<ICheckNotifier> checkAction;
        protected readonly T flowData;
        protected ReleaseData _releaseData;

        public FixedReleaseSource(Action<ICheckNotifier> checkAction = null, T flowData = null)
        {
            this.checkAction = checkAction ?? (cn => { });
            this.flowData = flowData;
        }

        public abstract T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);

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

        public void Check(ICheckNotifier notifier)
        {
            if (_releaseData.IsDesignTime)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Stale datasets will be checked at runtime...", CheckResult.Success));
                return;
            }

            var staleDatasets = _releaseData.ConfigurationsForRelease.SelectMany(c => c.Value).Where(
                   p => p.ExtractionResults.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted).ToArray();

            if (staleDatasets.Any())
                throw new Exception(
                    "The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were (" +
                    string.Join(",", staleDatasets.Select(ds => ds.ToString())) + ").  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");

            //make sure everything is releasable
            var dodgyStates = _releaseData.ConfigurationsForRelease.Where(
                kvp =>
                    kvp.Value.Any(
                        p =>
                            //these are the only permissable release states
                            p.Assesment != Releaseability.Releaseable &&
                            p.Assesment != Releaseability.ColumnDifferencesVsCatalogue)).ToArray();

            if (dodgyStates.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp in dodgyStates)
                {
                    sb.AppendLine(kvp.Key + ":");
                    foreach (var releasePotential in kvp.Value)
                        sb.AppendLine("\t" + releasePotential.Configuration.Name + " : " + releasePotential.Assesment);

                }

                throw new Exception("Attempted to release a dataset that was not evaluated as being releaseable.  The following Release Potentials were at a dodgy state:" + sb);
            }

            var projects = _releaseData.ConfigurationsForRelease.Keys.Select(cfr => cfr.Project_ID).Distinct().ToList();
            if (projects.Count() != 1)
                throw new Exception("How is it possible that you are doing a release for multiple different projects?");

            if (_releaseData.ConfigurationsForRelease.Any(kvp => kvp.Key.Project_ID != projects.First()))
                throw new Exception("Mismatch between project passed into constructor and DoRelease projects");

            if (_releaseData.EnvironmentPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable &&
                _releaseData.EnvironmentPotential.Assesment != TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
                throw new Exception("Ticketing system decided that the Environment is not ready for release. Reason: " + _releaseData.EnvironmentPotential.Reason);

            RunSpecificChecks(notifier);
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
            const string folderName = ExtractionDirectory.GlobalsDataFolderName;

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
