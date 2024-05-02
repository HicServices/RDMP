// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
///     Describes the use case in which a <see cref="Pipeline" /> takes artefacts produced as part of one or more
///     <see cref="ExtractionConfiguration" /> for a <see cref="Project" />.
///     The artefacts may be CSV files, tables in an extraction database etc.  The artefacts should be gathered and sent to
///     the recipient (e.g. zipped up and moved to FTP
///     server / output folder).
///     <para>The configurations should be marked as released.</para>
/// </summary>
public sealed class ReleaseUseCase : PipelineUseCase
{
    public ReleaseUseCase(IProject project, ReleaseData releaseData, ICatalogueRepository catalogueRepository)
    {
        ExplicitDestination = null;

        var releasePotentials = releaseData.ConfigurationsForRelease.Values.SelectMany(x => x).ToList();
        var releaseTypes = releasePotentials.Select(rp => rp.GetType()).Distinct().ToList();

        if (releaseTypes.Count == 0)
            throw new Exception("How did you manage to have multiple ZERO types in the extraction?");

        if (releaseTypes.Count(t => t != typeof(NoReleasePotential)) > 1)
            throw new Exception(
                $"You cannot release multiple configurations which have been extracted in multiple ways; e.g. one to DB and one to disk.  Your datasets have been extracted in the following ways:{Environment.NewLine}{string.Join($",{Environment.NewLine}", releaseTypes.Select(t => t.Name))}");

        var releasePotentialWithKnownDestination =
            releasePotentials.FirstOrDefault(rp => rp.DatasetExtractionResult != null);

        if (releasePotentialWithKnownDestination == null)
        {
            ExplicitSource = new NullReleaseSource();
        }
        else
        {
            var destinationType = MEF.GetType(
                releasePotentialWithKnownDestination.DatasetExtractionResult.DestinationType,
                typeof(IExecuteDatasetExtractionDestination));
            var destinationUsedAtExtraction =
                (IExecuteDatasetExtractionDestination)ObjectConstructor.Construct(destinationType, catalogueRepository);

            var fixedReleaseSource =
                destinationUsedAtExtraction.GetReleaseSource(catalogueRepository);

            ExplicitSource = fixedReleaseSource;
            // destinationUsedAtExtraction.GetReleaseSource(); // new FixedSource<ReleaseAudit>(notifier => CheckRelease(notifier));
        }

        AddInitializationObject(project);
        AddInitializationObject(releaseData);
        AddInitializationObject(catalogueRepository);

        GenerateContext();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        var contextFactory = new DataFlowPipelineContextFactory<ReleaseAudit>();
        var context = contextFactory.Create(PipelineUsage.FixedSource);

        context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseAudit>);

        return context;
    }

    /// <summary>
    ///     Design time constructor
    /// </summary>
    public ReleaseUseCase() : base(new[]
    {
        typeof(Project),
        typeof(ReleaseData),
        typeof(CatalogueRepository)
    })
    {
        ExplicitSource = new NullReleaseSource();
        GenerateContext();
    }

    public static ReleaseUseCase DesignTime()
    {
        return new ReleaseUseCase();
    }
}