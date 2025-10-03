// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Adds a new cohort set to <see cref="CohortAggregateContainer"/> (A UNION/INETERSECT/EXCEPT container).
/// The cohort set will return all identifiers in the <see cref="ConcreteColumn.IsExtractionIdentifier"/>
/// column of the dataset constrained by any <see cref="ConcreteFilter.IsMandatory"/> filters.  More filters
/// can be added later to further restrict the identifiers matched.
/// </summary>
public class ExecuteCommandAddCatalogueToCohortIdentificationSetContainer : BasicCommandExecution
{
    private readonly CatalogueCombineable _catalogueCombineable;
    private readonly CohortAggregateContainer _targetCohortAggregateContainer;

    private ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer _postImportCommand;

    public bool SkipMandatoryFilterCreation { get; set; }

    public AggregateConfiguration AggregateCreatedIfAny => _postImportCommand?.AggregateCreatedIfAny;

    [UseWithObjectConstructor]
    public ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(IBasicActivateItems activator,
        [DemandsInitialization("The container you want to add the set into")]
        CohortAggregateContainer targetCohortAggregateContainer,
        [DemandsInitialization("The dataset to add, must have an extraction identifier declared on it")]
        Catalogue catalogue,
        [DemandsInitialization(
            "Typically optional.  But if Catalogue has multiple columns marked IsExtractionIdentifier then you must indicate which to use here.")]
        ExtractionInformation identifierColumn = null
    ) : base(activator)
    {
        Weight = 0.11f;

        _targetCohortAggregateContainer = targetCohortAggregateContainer;

        if (targetCohortAggregateContainer.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);

        if (catalogue != null)
        {
            _catalogueCombineable = new CatalogueCombineable(catalogue);

            if (identifierColumn != null)
            {
                if (!identifierColumn.IsExtractionIdentifier)
                {
                    SetImpossible(
                        $"Column '{identifierColumn}' is not marked {nameof(ConcreteColumn.IsExtractionIdentifier)}");
                    return;
                }

                if (identifierColumn.CatalogueItem_ID != catalogue.ID)
                {
                    SetImpossible(
                        $"Column '{identifierColumn}'(ID={identifierColumn.ID}) is not from the same Catalogue as '{catalogue}'(ID={catalogue.ID})");
                    return;
                }

                // when/if it comes time to pick which extraction identifier to add pick the one the user said
                _catalogueCombineable.ResolveMultipleExtractionIdentifiers = (c, e) => identifierColumn;
            }

            UpdateIsImpossibleFor(_catalogueCombineable);
        }

        UseTripleDotSuffix = true;
    }

    public ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(IBasicActivateItems activator,
        CatalogueCombineable catalogueCombineable, CohortAggregateContainer targetCohortAggregateContainer) : this(
        activator, targetCohortAggregateContainer, null)
    {
        _catalogueCombineable = catalogueCombineable;
        _targetCohortAggregateContainer = targetCohortAggregateContainer;

        UpdateIsImpossibleFor(catalogueCombineable);
    }

    private void UpdateIsImpossibleFor(CatalogueCombineable catalogueCombineable)
    {
        if (catalogueCombineable.Catalogue.IsApiCall()) return;

        if (!catalogueCombineable.ContainsAtLeastOneExtractionIdentifier)
            SetImpossible(
                $"Catalogue {catalogueCombineable.Catalogue} does not contain any IsExtractionIdentifier columns");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);

    public override void Execute()
    {
        base.Execute();

        // if user hasn't picked a Catalogue yet
        if (_catalogueCombineable == null)
        {
            var cic = _targetCohortAggregateContainer.GetCohortIdentificationConfiguration();
            List<int> associatedProjectCataloguesIDs= new();
            var pcica = BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>().Where(pcica => pcica.CohortIdentificationConfiguration_ID == cic.ID).FirstOrDefault();
            if(pcica is not null)
            {
                associatedProjectCataloguesIDs = pcica.Project.GetAllProjectCatalogues().Select(c => c.ID).ToList();
            }
            if (!BasicActivator.SelectObjects(new DialogArgs
            {
                WindowTitle = "Add Catalogue(s) to Container",
                TaskDescription =
                        $"Choose which Catalogues to add to the cohort container '{_targetCohortAggregateContainer.Name}'.  Catalogues must have at least one IsExtractionIdentifier column."
            }, BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Where(c => !c.IsInternalDataset &&(!c.IsProjectSpecific(BasicActivator.RepositoryLocator.DataExportRepository) || associatedProjectCataloguesIDs.Contains(c.ID))).ToArray(), out var selected))
                // user didn't pick one
                return;

            // for each catalogue they picked
            foreach (var catalogue in selected)
            {
                if(BasicActivator.IsInteractive && catalogue.IsDeprecated)
                {
                    var confirmDeprecatedUser = BasicActivator.YesNo($"{catalogue.Name} is marked as deprecated. Are you sure you wish to use it?", "Confirm use of Deprecated Catalogue");
                    if (!confirmDeprecatedUser)
                    {
                        continue;
                    }
                }
                var combineable = new CatalogueCombineable(catalogue);

                UpdateIsImpossibleFor(combineable);

                if (IsImpossible)
                    throw new ImpossibleCommandException(this, ReasonCommandImpossible);

                // add it to the cic container
                Execute(combineable, catalogue == selected.Last());
            }
        }
        else
        {
            Execute(_catalogueCombineable, true);
        }
    }

    private void Execute(CatalogueCombineable catalogueCombineable, bool publish)
    {
        var cmd = catalogueCombineable.GenerateAggregateConfigurationFor(BasicActivator,
            _targetCohortAggregateContainer, !SkipMandatoryFilterCreation);
        if (cmd != null)
        {
            _postImportCommand =
                new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(BasicActivator, cmd,
                    _targetCohortAggregateContainer)
                {
                    DoNotClone = true,
                    NoPublish = !publish
                };
            _postImportCommand.Execute();
        }
    }
}