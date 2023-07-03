﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Creates or Updates an <see cref="ExtendedProperty"/> declaration on any RDMP object.
/// </summary>
public class ExecuteCommandSetExtendedProperty : BasicCommandExecution, IAtomicCommand
{
    public IMapsDirectlyToDatabaseTable[] SetOn { get; }
    public string PropertyName { get; }
    public string Value { get; }
    public bool Strict { get; }
        
    /// <summary>
    /// Set to true to prompt user for the <see cref="Value"/> at execution time (e.g. for interactive UIs)
    /// </summary>
    public bool PromptForValue { get; set; }

    /// <summary>
    /// If <see cref="PromptForValue"/> is set to true then this is the description to show to the user
    /// that explains what they should be entering as a <see cref="Value"/>
    /// </summary>
    public string PromptForValueTaskDescription { get; set; }

    [UseWithObjectConstructor]
    public ExecuteCommandSetExtendedProperty(IBasicActivateItems activator,
        [DemandsInitialization("The object(s) that you want to set the property on")]
        IMapsDirectlyToDatabaseTable[] setOn,
        [DemandsInitialization("The property you want to set")]
        string propertyName,
        [DemandsInitialization("The value to store")]
        string value,
        [DemandsInitialization("True to validate propertyName against known properties.  False to allow custom named properties.  Defaults to true.",DefaultValue = true)]
        bool strict = true
    )
        : base(activator)
    {
        if (strict && !ExtendedProperty.KnownProperties.Contains(propertyName))
        {
            SetImpossible($"{propertyName} is not a known property.  Known properties are: {Environment.NewLine}{string.Join(Environment.NewLine,ExtendedProperty.KnownProperties)}");
        }
        SetOn = setOn;
        PropertyName = propertyName;
        Value = value;
        Strict = strict;
    }

    public override string GetCommandName()
    {
        return !string.IsNullOrWhiteSpace(OverrideCommandName) ? OverrideCommandName : $"Set {PropertyName}";
    }

    public override void Execute()
    {
        base.Execute();

        var cataRepo = BasicActivator.RepositoryLocator.CatalogueRepository;
        var newValue = Value;
                        
        foreach(var o in SetOn)
        {
            var props = cataRepo.GetExtendedProperties(PropertyName, o).ToArray();
            var oldValue = props.FirstOrDefault()?.Value;

            if (PromptForValue)
            {
                if (!BasicActivator.TypeText(new DialogArgs
                    {
                        WindowTitle = PropertyName,
                        TaskDescription = PromptForValueTaskDescription
                    }, int.MaxValue, oldValue, out newValue, false))
                {
                    // user cancelled entering some text
                    return;
                }
            }

            // delete any old versions
            foreach (var d in props)
            {
                d.DeleteInDatabase();
            }

            // Creates the new property into the db
            // If the Value passed was null just leave it deleted
            if(!string.IsNullOrWhiteSpace(newValue))
            {
                new ExtendedProperty(cataRepo, o, PropertyName, newValue);
            }
                    
        }

        if(SetOn.Any())
            Publish(SetOn.First());
    }
}