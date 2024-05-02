// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Clone and import one or more <see cref="CohortIdentificationConfiguration" /> into a root or subcontainer of
///     another
/// </summary>
public class ExecuteCommandImportCohortIdentificationConfiguration : BasicCommandExecution
{
    public CohortAggregateContainer IntoContainer { get; }
    public CohortIdentificationConfiguration[] ToImport { get; }

    public ExecuteCommandImportCohortIdentificationConfiguration(IBasicActivateItems activator,
        CohortIdentificationConfiguration[] toImport, CohortAggregateContainer intoContainer) : base(activator)
    {
        Weight = 0.15f;

        ToImport = toImport;
        IntoContainer = intoContainer;

        if (IntoContainer == null)
        {
            SetImpossible("You must specify a container");
            return;
        }

        if (intoContainer.ShouldBeReadOnly(out var reason)) SetImpossible(reason);

        // if we don't know what to import yet then we should have the
        // 'more choices to come' suffix
        UseTripleDotSuffix = ToImport == null;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Add);
    }

    public override void Execute()
    {
        base.Execute();

        var import = ToImport;

        if (import == null)
            if (!BasicActivator.SelectObjects(new DialogArgs
                    {
                        WindowTitle = "Add CohortIdentificationConfiguration(s) to Container",
                        TaskDescription =
                            $"Choose which CohortIdentificationConfiguration(s) to add to the cohort container '{IntoContainer.Name}'.  For each one selected, the entire query tree will be imported."
                    },
                    BasicActivator.RepositoryLocator.CatalogueRepository
                        .GetAllObjects<CohortIdentificationConfiguration>(),
                    out import))
                return;

        if (import == null || !import.Any())
            return;


        var merger =
            new CohortIdentificationConfigurationMerger(
                (CatalogueRepository)BasicActivator.RepositoryLocator.CatalogueRepository);
        merger.Import(import, IntoContainer);

        Publish(IntoContainer);
        Emphasise(IntoContainer, int.MaxValue);
    }
}