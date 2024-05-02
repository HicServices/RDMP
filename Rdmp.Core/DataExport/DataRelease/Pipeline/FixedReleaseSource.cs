// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
///     Release pipeline must start with a Fixed Source that will run checks and prepare the source folder.
///     Extraction Destinations will return an implementation of this class based on the extraction method used.
/// </summary>
/// <typeparam name="T">The type which is passed around in the pipeline</typeparam>
public abstract class FixedReleaseSource<T> : ICheckable, IPipelineRequirement<ReleaseData>, IDataFlowSource<T>
    where T : ReleaseAudit
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
        if (isRunTime)
        {
            var allPotentials = _releaseData.ConfigurationsForRelease.SelectMany(c => c.Value).ToList();
            var staleDatasets = allPotentials.Where(
                    p => p.DatasetExtractionResult.HasLocalChanges().Evaluation ==
                         ChangeDescription.DatabaseCopyWasDeleted)
                .ToArray();

            if (staleDatasets.Any())
                throw new Exception(
                    $"The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were ({string.Join(",", staleDatasets.Select(ds => ds.ToString()))}).  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");

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
                var sb = new StringBuilder();
                foreach (var kvp in dodgyStates)
                {
                    sb.AppendLine($"{kvp.Key}:");
                    foreach (var releasePotential in kvp.Value)
                        sb.AppendLine(
                            $"\t{releasePotential.Configuration.Name} : {releasePotential.DatasetExtractionResult}");
                }

                throw new Exception(
                    $"Attempted to release a dataset that was not evaluated as being releaseable. The following Release Potentials were at a dodgy state:{sb}");
            }

            foreach (var environmentPotential in _releaseData.EnvironmentPotentials.Values)
            {
                environmentPotential.Check(notifier);
                if (environmentPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable &&
                    environmentPotential.Assesment != TicketingReleaseabilityEvaluation
                        .TicketingLibraryMissingOrNotConfiguredCorrectly)
                    throw new Exception(
                        $"Ticketing system decided that the Environment is not ready for release. Reason: {environmentPotential.Reason}");
            }
        }

        var projects = _releaseData.ConfigurationsForRelease.Keys.Select(cfr => cfr.Project_ID).Distinct().ToList();
        if (projects.Count != 1)
            notifier.OnCheckPerformed(new CheckEventArgs(
                "How is it possible that you are doing a release for multiple different projects?", CheckResult.Fail));

        if (_releaseData.ConfigurationsForRelease.Any(kvp => kvp.Key.Project_ID != projects.First()))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Mismatch between project passed into constructor and DoRelease projects", CheckResult.Fail));

        RunSpecificChecks(notifier, isRunTime);
    }

    public void Check(ICheckNotifier notifier)
    {
        Check(notifier, false);
    }

    protected abstract void RunSpecificChecks(ICheckNotifier notifier, bool isRunTime);

    protected virtual DirectoryInfo PrepareSourceGlobalFolder()
    {
        var globalDirectoriesFound = new List<DirectoryInfo>();

        foreach (var releasePotentials in _releaseData.ConfigurationsForRelease)
            globalDirectoriesFound.AddRange(GetAllGlobalFolders(releasePotentials));

        if (globalDirectoriesFound.Any())
        {
            var firstGlobal = globalDirectoriesFound.First();

            foreach (var directoryInfo in globalDirectoriesFound.Distinct(new DirectoryInfoComparer()))
                UsefulStuff.ConfirmContentsOfDirectoryAreTheSame(firstGlobal, directoryInfo);

            return firstGlobal;
        }

        return null;
    }

    protected IEnumerable<DirectoryInfo> GetAllGlobalFolders(
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
    {
        const string folderName = ExtractionDirectory.GLOBALS_DATA_NAME;

        foreach (var releasePotential in toRelease.Value)
        {
            Debug.Assert(releasePotential.ExtractDirectory.Parent != null,
                "releasePotential.ExtractDirectory.Parent != null");
            var globalFolderForThisExtract = releasePotential.ExtractDirectory.Parent
                .EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly).SingleOrDefault();

            if (globalFolderForThisExtract == null) //this particular release didn't include globals/custom data at all
                continue;

            yield return globalFolderForThisExtract;
        }
    }
}