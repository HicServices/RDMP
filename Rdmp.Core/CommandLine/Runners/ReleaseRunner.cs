// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataRelease;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
/// Runs the release process for one or more <see cref="ExtractionConfiguration"/> in the same <see cref="Project"/>.  This is the proces by which we gather all the artifacts
/// produced by the Extraction Engine (anonymised project extracts, bundled lookups and documents etc) and transmit them somewhere as a final released package.
/// </summary>
public class ReleaseRunner : ManyRunner
{
    private readonly ReleaseOptions _options;
    private readonly IBasicActivateItems _activator;
    private Pipeline _pipeline;
    private IProject _project;
    private IExtractionConfiguration[] _configurations;
    private ISelectedDataSets[] _selectedDatasets;

    public ReleaseRunner(IBasicActivateItems activator, ReleaseOptions options) : base(options)
    {
        _activator = activator;
        _options = options;
    }

    protected override void Initialize()
    {
        _pipeline = GetObjectFromCommandLineString<Pipeline>(RepositoryLocator, _options.Pipeline);

        //get all configurations user has picked
        _configurations =
            GetObjectsFromCommandLineString<ExtractionConfiguration>(RepositoryLocator, _options.Configurations)
                .ToArray();

        if (_options.SkipReleased) _configurations = _configurations.Where(c => !c.IsReleased).ToArray();

        //some datasets only
        if (_options.SelectedDataSets != null && _options.SelectedDataSets.Any())
        {
            _selectedDatasets =
                GetObjectsFromCommandLineString<SelectedDataSets>(RepositoryLocator, _options.SelectedDataSets)
                    .ToArray();

            var configurationIds = _configurations.Select(c => c.ID).ToArray();

            //if user has specified some selected datasets that do not belong to configurations they specified then we will need to include
            //those configurations as well
            foreach (var s in _selectedDatasets)
                if (!configurationIds.Contains(s.ExtractionConfiguration_ID))
                    //add the config since it's not included in _options.Configurations
                    _configurations = _configurations.ToList().Union(new[] { s.ExtractionConfiguration }).ToArray();
        }
        else
        {
            _selectedDatasets = _configurations.SelectMany(c => c.SelectedDataSets).ToArray();
        }

        if (!_configurations.Any())
            throw new Exception("No Configurations have been selected for release");

        _project = _configurations.Select(c => c.Project).Distinct().Single();
    }


    protected override void AfterRun()
    {
    }

    protected override ICheckable[] GetCheckables(ICheckNotifier checkNotifier)
    {
        foreach (var configuration in _configurations)
            IdentifyAndRemoveOldExtractionResults(RepositoryLocator, checkNotifier, configuration);

        var toReturn = new List<ICheckable>();

        if (_options.ReleaseGlobals ?? true)
        {
            toReturn.Add(new GlobalsReleaseChecker(RepositoryLocator, _configurations));
            toReturn.AddRange(_configurations.First()
                .GetGlobals()
                .Select(availableGlobal =>
                    new GlobalsReleaseChecker(RepositoryLocator, _configurations, availableGlobal).GetEvaluator()));
        }

        foreach (var configuration in _configurations)
        {
            toReturn.AddRange(GetReleasePotentials(checkNotifier, configuration));
            toReturn.Add(new ReleaseEnvironmentPotential(configuration));
        }

        if (_pipeline == null)
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs("No Pipeline has been picked", CheckResult.Fail));
            return Array.Empty<ICheckable>();
        }

        var useCase = GetReleaseUseCase(checkNotifier);
        if (useCase != null)
        {
            var engine = useCase.GetEngine(_pipeline, ThrowImmediatelyDataLoadEventListener.Quiet);
            if (((IDataFlowPipelineEngine)engine).DestinationObject is IInteractiveCheckable)
            {
                ((IInteractiveCheckable)(((IDataFlowPipelineEngine)engine).DestinationObject)).SetActivator(_activator);
            }
            toReturn.Add(engine);
        }
        return toReturn.ToArray();
    }

    public static void IdentifyAndRemoveOldExtractionResults(IRDMPPlatformRepositoryServiceLocator repo,
        ICheckNotifier checkNotifier, IExtractionConfiguration configuration)
    {
        var oldResults = configuration.CumulativeExtractionResults
            .Where(cer => !configuration.GetAllExtractableDataSets().Contains(cer.ExtractableDataSet))
            .ToArray();

        if (oldResults.Any())
        {
            var message =
                $"In Configuration {configuration}:{Environment.NewLine}{Environment.NewLine}The following CumulativeExtractionResults reflect datasets that were previously extracted under the existing Configuration but are no longer in the CURRENT configuration:";

            message = oldResults.Aggregate(message, (s, n) => s + Environment.NewLine + n);

            if (
                checkNotifier.OnCheckPerformed(new CheckEventArgs(message, CheckResult.Fail, null,
                    $"Delete expired CumulativeExtractionResults for configuration.{Environment.NewLine}Not doing so may result in failures at Release time.")))
                foreach (var result in oldResults)
                    result.DeleteInDatabase();
        }

        var oldLostSupplemental = configuration.CumulativeExtractionResults
            .SelectMany(c => c.SupplementalExtractionResults)
            .Union(configuration.SupplementalExtractionResults)
            .Where(s => !repo.ArbitraryDatabaseObjectExists(s.ReferencedObjectRepositoryType, s.ReferencedObjectType,
                s.ReferencedObjectID))
            .ToArray();

        if (oldLostSupplemental.Any())
        {
            var message =
                $"In Configuration {configuration}:{Environment.NewLine}{Environment.NewLine}The following list reflect objects (supporting sql, lookups or documents) that were previously extracted but have since been deleted:";

            message = oldLostSupplemental.Aggregate(message,
                (s, n) => s + Environment.NewLine + n.DestinationDescription);

            if (
                checkNotifier.OnCheckPerformed(new CheckEventArgs(message, CheckResult.Fail, null,
                    $"Delete expired Extraction Results for configuration.{Environment.NewLine}Not doing so may result in failures at Release time.")))
                foreach (var result in oldLostSupplemental)
                    result.DeleteInDatabase();
        }
    }

    private List<ReleasePotential> GetReleasePotentials(ICheckNotifier checkNotifier,
        IExtractionConfiguration configuration)
    {
        var toReturn = new List<ReleasePotential>();

        //create new ReleaseAssesments
        foreach (var selectedDataSet in GetSelectedDataSets(configuration)) //todo only the ones user ticked
        {
            var extractionResults = selectedDataSet.GetCumulativeExtractionResultsIfAny();

            var progress = selectedDataSet.ExtractionProgressIfAny;
            if (progress != null)
                if (progress.MoreToFetch())
                    checkNotifier.OnCheckPerformed(new CheckEventArgs(
                        ErrorCodes.AttemptToReleaseUnfinishedExtractionProgress,
                        selectedDataSet, progress.ProgressDate, progress.EndDate));

            //if it has never been extracted
            if (extractionResults?.DestinationDescription == null)
            {
                toReturn.Add(new NoReleasePotential(RepositoryLocator,
                    selectedDataSet)); //the potential is ZERO to release this dataset
            }
            else
            {
                //it's been extracted!, who extracted it?
                var destinationThatExtractedIt =
                    (IExecuteDatasetExtractionDestination)ObjectConstructor.Construct(extractionResults
                        .GetDestinationType());

                //destination tell us how releasable it is
                var releasePotential =
                    destinationThatExtractedIt.GetReleasePotential(RepositoryLocator, selectedDataSet);

                //it is THIS much releasability!
                toReturn.Add(releasePotential);
            }
        }

        return toReturn;
    }

    protected override object[] GetRunnables()
    {
        return new[] { GetReleaseUseCase(ThrowImmediatelyCheckNotifier.Quiet) };
    }

    private ReleaseUseCase GetReleaseUseCase(ICheckNotifier checkNotifier)
    {
        var data = new ReleaseData(RepositoryLocator);

        foreach (var configuration in _configurations)
        {
            data.ConfigurationsForRelease.Add(configuration, GetReleasePotentials(checkNotifier, configuration));
            data.EnvironmentPotentials.Add(configuration, new ReleaseEnvironmentPotential(configuration));
            data.SelectedDatasets.Add(configuration, GetSelectedDataSets(configuration));
        }

        data.ReleaseGlobals = _options.ReleaseGlobals ?? true;

        var allDdatasets = _configurations.SelectMany(ec => ec.GetAllExtractableDataSets()).ToList();
        var selectedDatasets = data.SelectedDatasets.Values.SelectMany(sd => sd.ToList()).ToList();

        data.ReleaseState = allDdatasets.Count != selectedDatasets.Count
            ? ReleaseState.DoingPatch
            : ReleaseState.DoingProperRelease;

        try
        {
            return new ReleaseUseCase(_project, data, RepositoryLocator.CatalogueRepository);
        }
        catch (Exception ex)
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs($"FAIL: {ex.Message}", CheckResult.Fail, ex));
        }

        return null;
    }

    protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
    {
        var useCase = (ReleaseUseCase)runnable;
        var engine = useCase.GetEngine(_pipeline, listener);
        engine.ExecutePipeline(Token);
    }

    private IEnumerable<ISelectedDataSets> GetSelectedDataSets(IExtractionConfiguration configuration)
    {
        //are we only releasing some of the datasets?
        var onlySomeDatasets = _selectedDatasets.Where(sds => sds.ExtractionConfiguration_ID == configuration.ID)
            .ToArray();

        if (onlySomeDatasets.Any())
            return onlySomeDatasets;

        //no, we are releasing all of them
        return configuration.SelectedDataSets;
    }

    public object GetState(IExtractionConfiguration configuration)
    {
        var matches = GetCheckerResults<ReleaseEnvironmentPotential>(rp => rp.Configuration.Equals(configuration));

        return matches.Length == 0 ? null : ((ReleaseEnvironmentPotential)matches.Single().Key).Assesment;
    }

    public object GetState(ISelectedDataSets selectedDataSets)
    {
        var matches = GetCheckerResults<ReleasePotential>(rp => rp.SelectedDataSet.ID == selectedDataSets.ID);

        if (matches.Length == 0)
            return null;

        var releasePotential = (ReleasePotential)matches.Single().Key;
        var results = matches.Single().Value;

        //not been released ever
        if (releasePotential is NoReleasePotential)
            return Releaseability.NeverBeenSuccessfullyExecuted;

        //do we know the release state of the assesments
        if (releasePotential.Assessments != null && releasePotential.Assessments.Any())
        {
            var releasability = releasePotential.Assessments.Values.Min();

            if (releasability != Releaseability.Undefined)
                return releasability;
        }

        //otherwise use the checks of it
        return results.GetWorst();
    }

    public object GetState(SupportingSQLTable global) => GetState((IMapsDirectlyToDatabaseTable)global);

    public object GetState(SupportingDocument global) => GetState((IMapsDirectlyToDatabaseTable)global);

    private object GetState(IMapsDirectlyToDatabaseTable global)
    {
        var matches = GetCheckerResults<GlobalReleasePotential>(rp => rp.RelatedGlobal.Equals(global));

        return matches.Length == 0 ? null : ((GlobalReleasePotential)matches.Single().Key).Releasability;
    }

    public CheckResult? GetGlobalReleaseState()
    {
        var result = GetSingleCheckerResults<GlobalsReleaseChecker>();

        return result?.GetWorst();
    }
}