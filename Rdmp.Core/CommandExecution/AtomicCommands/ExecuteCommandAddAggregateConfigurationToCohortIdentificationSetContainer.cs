// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer : BasicCommandExecution
{
    private readonly AggregateConfigurationCombineable _aggregateConfigurationCombineable;
    private readonly CohortAggregateContainer _targetCohortAggregateContainer;
    private readonly bool _offerCohortAggregates;
    private readonly AggregateConfiguration[] _available;

    public AggregateConfiguration AggregateCreatedIfAny { get; private set; }

    /// <summary>
    ///     True if the <see cref="AggregateConfigurationCombineable" /> passed to the constructor was a newly created one and
    ///     does
    ///     not need cloning.
    /// </summary>
    public bool DoNotClone { get; set; }

    private void SetCommandWeight()
    {
        Weight = _offerCohortAggregates ? 0.14f : 0.13f;
    }


    private ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems activator,
        CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
    {
        _targetCohortAggregateContainer = targetCohortAggregateContainer;

        if (targetCohortAggregateContainer.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);

        UseTripleDotSuffix = true;
        SetCommandWeight();
    }

    public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems activator,
        AggregateConfigurationCombineable aggregateConfigurationCommand,
        CohortAggregateContainer targetCohortAggregateContainer) : this(activator, targetCohortAggregateContainer)
    {
        _aggregateConfigurationCombineable = aggregateConfigurationCommand;

        SetCommandWeight();
    }

    [UseWithObjectConstructor]
    public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems activator,
        AggregateConfiguration aggregateConfiguration, CohortAggregateContainer targetCohortAggregateContainer)
        : this(activator, new AggregateConfigurationCombineable(aggregateConfiguration), targetCohortAggregateContainer)
    {
    }

    /// <summary>
    ///     Constructor for selecting one or more aggregates at execute time
    /// </summary>
    /// <param name="basicActivator"></param>
    /// <param name="targetCohortAggregateContainer"></param>
    /// <param name="offerCohortAggregates"></param>
    public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IBasicActivateItems basicActivator,
        CohortAggregateContainer targetCohortAggregateContainer, bool offerCohortAggregates) : this(basicActivator,
        targetCohortAggregateContainer)
    {
        if (offerCohortAggregates)
        {
            _available = BasicActivator.CoreChildProvider.AllAggregateConfigurations
                .Where(c => c.IsCohortIdentificationAggregate && !c.IsJoinablePatientIndexTable()).ToArray();

            if (_available.Length == 0) SetImpossible("You do not currently have any cohort sets");
        }
        else
        {
            _available = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<AggregateConfiguration>()
                .Where(c => !c.IsCohortIdentificationAggregate).ToArray();

            if (_available.Length == 0)
                SetImpossible("You do not currently have any non-cohort AggregateConfigurations");
        }

        _offerCohortAggregates = offerCohortAggregates;

        SetCommandWeight();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return _offerCohortAggregates
            ? iconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Add)
            : iconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add);
    }

    public override string GetCommandName()
    {
        // If we're explicity overriding the command name, then use that
        if (!string.IsNullOrWhiteSpace(OverrideCommandName))
            return base.GetCommandName();

        // if an execute time decision is expected then command name should reflect the kind of available objects the user can add
        return _available?.Any() ?? false
            ? _offerCohortAggregates ? "Import (Copy of) Cohort Set into container" : "Add Aggregate(s) into container"
            : base.GetCommandName();
    }

    public override void Execute()
    {
        base.Execute();

        var available = _available;

        if (_aggregateConfigurationCombineable == null)
        {
            // runtime decision is required

            if (available == null || !available.Any()) throw new Exception("There are no available objects to add");

            // Are there templates that we can use instead of showing all?
            var cataRepo = BasicActivator.RepositoryLocator.CatalogueRepository;
            var templates = cataRepo.GetExtendedProperties(ExtendedProperty.IsTemplate)
                .Select(p => p.GetReferencedObject(BasicActivator.RepositoryLocator))
                .OfType<AggregateConfiguration>()
                .ToArray();

            // yes
            if (templates.Any())
            {
                // ask user if they want to use a template
                if (BasicActivator.YesNo(new DialogArgs
                    {
                        WindowTitle = "Use Template?",
                        TaskDescription =
                            $"You have {templates.Length} AggregateConfiguration templates, do you want to use one of these?"
                    }, out var useTemplate))
                    available = useTemplate ? templates : available.Except(templates).ToArray();
                else
                    // cancel clicked?
                    return;
            }

            if (!BasicActivator.SelectObjects(new DialogArgs
                {
                    WindowTitle = "Add Aggregate Configuration(s) to Container",
                    TaskDescription =
                        $"Choose which AggregateConfiguration(s) to add to the cohort container '{_targetCohortAggregateContainer.Name}'."
                }, available, out var selected))
                // user cancelled
                return;

            foreach (var aggregateConfiguration in selected)
            {
                var combineable = new AggregateConfigurationCombineable(aggregateConfiguration);
                Execute(combineable, aggregateConfiguration == selected.Last());
            }
        }
        else
        {
            Execute(_aggregateConfigurationCombineable, true);
        }

        if (AggregateCreatedIfAny != null)
            Emphasise(AggregateCreatedIfAny);
    }

    private void Execute(AggregateConfigurationCombineable toAdd, bool publish)
    {
        var cic = _targetCohortAggregateContainer.GetCohortIdentificationConfiguration();

        var child = DoNotClone
            ? toAdd.Aggregate
            : cic.ImportAggregateConfigurationAsIdentifierList(toAdd.Aggregate,
                (a, b) => CohortCombineToCreateCommandHelper.PickOneExtractionIdentifier(BasicActivator, a, b));

        //current contents
        var contents = _targetCohortAggregateContainer.GetOrderedContents().ToArray();

        //insert it at the begining of the contents
        var minimumOrder = 0;
        if (contents.Any())
            minimumOrder = contents.Min(o => o.Order);

        //bump everyone down to make room
        _targetCohortAggregateContainer.CreateInsertionPointAtOrder(child, minimumOrder, true);
        _targetCohortAggregateContainer.AddChild(child, minimumOrder);

        if (publish)
            Publish(_targetCohortAggregateContainer);

        AggregateCreatedIfAny = child;
    }
}