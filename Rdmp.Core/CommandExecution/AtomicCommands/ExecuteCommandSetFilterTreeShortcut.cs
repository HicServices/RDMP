// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates a reference in one <see cref="AggregateConfiguration" /> pointing to another informing it to use the WHERE
///     filter logic of the other.  This allows you to maintain a single master copy of a given configuration and reference
///     it from several places without creating duplicates.
/// </summary>
internal class ExecuteCommandSetFilterTreeShortcut : BasicCommandExecution
{
    private AggregateConfiguration _setOn { get; }

    private readonly bool _promptChoice;

    private AggregateConfiguration _pointTo { get; }

    /// <summary>
    ///     Constructor for interactive mode (ask what they want to set it to when the command is run)
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="setOn"></param>
    public ExecuteCommandSetFilterTreeShortcut(IBasicActivateItems activator, AggregateConfiguration setOn) :
        base(activator)
    {
        _setOn = setOn;
        _promptChoice = true;

        if (_setOn.RootFilterContainer_ID != null)
            SetImpossible("Aggregate already has a root filter container");

        if (_setOn.Catalogue.IsApiCall())
            SetImpossible(ExecuteCommandAddNewFilterContainer.FiltersCannotBeAddedToApiCalls);
    }

    /// <summary>
    ///     Constructor for setting a specific shortcut
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="setOn"></param>
    /// <param name="pointTo"></param>
    [UseWithObjectConstructor]
    public ExecuteCommandSetFilterTreeShortcut(IBasicActivateItems activator,
        [DemandsInitialization(
            "An aggregate for whom you want to set the WHERE logic on (must not have any current filters/containers)")]
        AggregateConfiguration setOn,
        [DemandsInitialization(
            "The destination aggregate which contains a WHERE logic tree (containers and filters) that you want to copy.  Pass Null to clear")]
        AggregateConfiguration pointTo) : base(activator)
    {
        _setOn = setOn;
        _pointTo = pointTo;

        if (_setOn.RootFilterContainer_ID != null)
            SetImpossible($"{_setOn} already has a root filter container");

        if (_setOn.Catalogue.IsApiCall())
            SetImpossible(ExecuteCommandAddNewFilterContainer.FiltersCannotBeAddedToApiCalls);

        if (_pointTo is { RootFilterContainer_ID: null })
            SetImpossible($"{_pointTo} does not have a filter container tree to link to");

        if (_pointTo == null && setOn.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null)
            SetImpossible($"{_pointTo} does not have a shortcut to clear");
    }

    public override void Execute()
    {
        base.Execute();

        var pointTo = _pointTo;

        if (_promptChoice && pointTo == null)
        {
            var available = _setOn.Repository.GetAllObjects<AggregateConfiguration>().Where(a =>
                    //which are not themselves already shortcuts!
                    a.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null
                    &&
                    //and which have a filter set!
                    a.RootFilterContainer_ID != null)
                //and are not ourself!
                .Except(new[] { _setOn }).ToArray();


            if (!available.Any())
            {
                BasicActivator.Show("There are no other AggregateConfigurations with filter trees you could reference");
                return;
            }

            pointTo = (AggregateConfiguration)BasicActivator.SelectOne("Target", available);

            // Looks like they didn't make a choice
            if (pointTo == null)
                return;
        }

        _setOn.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = pointTo?.ID;
        _setOn.SaveToDatabase();
        Publish(_setOn);
    }
}