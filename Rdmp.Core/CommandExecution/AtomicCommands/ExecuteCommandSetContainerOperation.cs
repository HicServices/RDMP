// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Changes the set operation on a <see cref="CohortAggregateContainer" />
/// </summary>
public class ExecuteCommandSetContainerOperation : BasicCommandExecution
{
    private readonly CohortAggregateContainer _container;
    private readonly SetOperation _operation;

    public ExecuteCommandSetContainerOperation(IBasicActivateItems activator, CohortAggregateContainer container,
        SetOperation operation) : base(activator)
    {
        if (container.Operation == operation)
            SetImpossible($"Container already uses {operation}");

        _container = container;
        _operation = operation;

        if (container.ShouldBeReadOnly(out var reason)) SetImpossible(reason);

        Weight = _operation switch
        {
            SetOperation.UNION => 0.21f,
            SetOperation.EXCEPT => 0.22f,
            SetOperation.INTERSECT => 0.23f,
            _ => Weight
        };
    }

    public override string GetCommandName()
    {
        return !string.IsNullOrWhiteSpace(OverrideCommandName)
            ? OverrideCommandName
            : $"Set operation {_operation}";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return _operation switch
        {
            SetOperation.EXCEPT => iconProvider.GetImage(Image.Load<Rgba32>(CatalogueIcons.EXCEPT)),
            SetOperation.INTERSECT => iconProvider.GetImage(Image.Load<Rgba32>(CatalogueIcons.INTERSECT)),
            SetOperation.UNION => iconProvider.GetImage(Image.Load<Rgba32>(CatalogueIcons.UNION)),
            _ => base.GetImage(iconProvider)
        };
    }

    public override void Execute()
    {
        base.Execute();

        var oldOperation = _container.Operation;

        //if the old name was UNION and we are changing to INTERSECT Operation then we should probably change the Name too! even if they have something like 'INTERSECT the people who are big and small' and they change to UNION we want it to be changed to 'UNION the people who are big and small'
        if (_container.Name.StartsWith(oldOperation.ToString()))
        {
            _container.Name = _operation + _container.Name[oldOperation.ToString().Length..];
        }
        else
        {
            if (BasicActivator.TypeText("New name for container?",
                    "You have changed the operation, do you want to give it a new description?", 1000, _container.Name,
                    out var newName, false))
            {
                _container.Name = newName;
            }
            else
            {
                Show("Cancelled changing operation");
                // user cancelled the operation
                return;
            }
        }

        _container.Operation = _operation;
        _container.SaveToDatabase();
        Publish(_container);
    }
}