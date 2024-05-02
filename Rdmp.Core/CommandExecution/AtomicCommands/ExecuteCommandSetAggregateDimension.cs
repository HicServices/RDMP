// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Removes all <see cref="AggregateDimension" /> on an <see cref="AggregateConfiguration" /> and creates
///     a single new dimension for linkage in a <see cref="CohortIdentificationConfiguration" /> based on
///     a chosen <see cref="ExtractionInformation" /> which must be marked
///     <see cref="ConcreteColumn.IsExtractionIdentifier" />
/// </summary>
public class ExecuteCommandSetAggregateDimension : BasicCommandExecution, IAtomicCommand
{
    private readonly AggregateConfiguration _aggregate;
    private readonly ExtractionInformation[] _available;
    private readonly ExtractionInformation _extractionInformation;

    public ExecuteCommandSetAggregateDimension(IBasicActivateItems activator,
        [DemandsInitialization("The AggregateConfiguration which you want to change the extraction identifier on")]
        AggregateConfiguration ac,
        [DemandsInitialization(
            "The extractable column in the Catalogue which should be created as a new AggregateDimension on the aggregate.  Or null to prompt at runtime",
            DefaultValue = true)]
        ExtractionInformation ei = null) : base(activator)
    {
        if (!ac.IsCohortIdentificationAggregate)
        {
            SetImpossible("AggregateConfiguration is not a cohort aggregate");
            return;
        }

        _extractionInformation = ei;

        if (_extractionInformation is { IsExtractionIdentifier: false })
        {
            SetImpossible($"'{_extractionInformation}' is not marked IsExtractionIdentifier");
            return;
        }

        try
        {
            var cata = ac.GetCatalogue();
            _available = cata.GetAllExtractionInformation().Where(ci => ci.IsExtractionIdentifier).ToArray();

            if (_extractionInformation != null)
                if (cata.ID != _extractionInformation.CatalogueItem.Catalogue_ID)
                {
                    SetImpossible($"'{_extractionInformation}' does not belong to the same Catalogue as '{ac}'");
                    return;
                }

            if (_available.Length == 0)
            {
                SetImpossible($"There are no columns in Catalogue '{cata}' that are marked IsExtractionIdentifier");
                return;
            }

            if (_available.Length == 1 && ac.AggregateDimensions.Length == 1 &&
                _available[0].ID == ac.AggregateDimensions[0].ExtractionInformation_ID)
            {
                SetImpossible(
                    $"AggregateConfiguration already uses the only IsExtractionIdentifier column in '{cata}'");
                return;
            }
        }
        catch (Exception ex)
        {
            SetImpossible($"Could not determine compatible columns:{ex.Message}");
        }

        _aggregate = ac;
    }

    public override void Execute()
    {
        base.Execute();

        var chosen = _extractionInformation;

        if (chosen == null && BasicActivator.IsInteractive)
            chosen = (ExtractionInformation)BasicActivator.SelectOne(new DialogArgs
            {
                WindowTitle = "Select AggregateDimension",
                TaskDescription =
                    "Choose which column to query and link with other datasets in the CohortIdentificationConfiguration.  All datasets in the configuration must have the same identifier type to be linkable."
            }, _available);

        if (chosen == null)
            return;

        foreach (var d in _aggregate.AggregateDimensions) d.DeleteInDatabase();

        var added = _aggregate.AddDimension(chosen);
        Publish(_aggregate);
        Emphasise(added);
    }
}