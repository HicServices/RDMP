// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Changes a single property of an object and saves the new value to the database.  New value must be valid for the
///     object and respect
///     any Type / Database constraints.
/// </summary>
public class ExecuteCommandSet : BasicCommandExecution
{
    private readonly IMapsDirectlyToDatabaseTable _setOn;
    private readonly Func<IMapsDirectlyToDatabaseTable> _setOnFunc;
    private readonly PropertyInfo _property;
    private readonly bool _getValueAtExecuteTime;

    /// <summary>
    ///     Optional dialog arguments for UI prompts when running the command
    /// </summary>
    public DialogArgs DialogArgs { get; set; }

    /// <summary>
    ///     The new value chosen by the user during command execution
    /// </summary>
    public object NewValue { get; private set; }

    /// <summary>
    ///     True if the command was successfully completed
    /// </summary>
    public bool Success { get; private set; }

    [UseWithObjectConstructor]
    public ExecuteCommandSet(IBasicActivateItems activator,
        [DemandsInitialization("A single object on which you want to change a given property")]
        IMapsDirectlyToDatabaseTable setOn,
        [DemandsInitialization("Name of a property you want to change e.g. Description")]
        string property,
        [DemandsInitialization(
            "New value to assign, this will be parsed into a valid Type if property is not a string")]
        string value) : base(activator)
    {
        _setOn = setOn;

        _property = GetProperty(_setOn, property);

        if (_setOn is IMightBeReadOnly m) SetImpossibleIfReadonly(m);

        if (_property == null)
        {
            SetImpossible($"Unknown Property '{property}'");

            //suggest similar sounding properties
            var suggestions =
                _setOn.GetType().GetProperties().Where(c =>
                        CultureInfo.CurrentCulture.CompareInfo.IndexOf(c.Name, property, CompareOptions.IgnoreCase) >=
                        0)
                    .ToArray();

            if (suggestions.Any())
            {
                var msg = new StringBuilder($"Unknown Property '{property}'");
                msg.AppendLine();
                msg.AppendLine("Did you mean:");
                foreach (var s in suggestions)
                    msg.AppendLine(s.Name);
                activator.Show(msg.ToString());
            }
        }
        else
        {
            var picker = new CommandLineObjectPicker(new[] { value ?? "NULL" }, activator);

            if (!picker.HasArgumentOfType(0, _property.PropertyType))
                SetImpossible($"Provided value could not be converted to '{_property.PropertyType}'");
            else
                NewValue = picker[0].GetValueForParameterOfType(_property.PropertyType);
        }
    }

    private static PropertyInfo GetProperty(IMapsDirectlyToDatabaseTable setOn, string property)
    {
        var props = setOn.GetType().GetProperties();

        return props.FirstOrDefault(p => string.Equals(p.Name, property)) ??
               props.FirstOrDefault(p => string.Equals(p.Name, property, StringComparison.InvariantCultureIgnoreCase));
    }

    public ExecuteCommandSet(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable setOn, PropertyInfo property) :
        base(activator)
    {
        _setOn = setOn;
        _property = property;
        _getValueAtExecuteTime = true;

        if (string.Equals("ID", property?.Name)) SetImpossible("ID property cannot be changed");
    }

    public ExecuteCommandSet(IBasicActivateItems activator, Func<IMapsDirectlyToDatabaseTable> setOnFunc,
        PropertyInfo property) : base(activator)
    {
        _setOnFunc = setOnFunc;
        _property = property;
        _getValueAtExecuteTime = true;

        if (string.Equals("ID", property?.Name)) SetImpossible("ID property cannot be changed");
    }

    public override string GetCommandName()
    {
        if (!string.IsNullOrEmpty(OverrideCommandName)) return OverrideCommandName;

        return _property != null ? $"Set {_property.Name}" : base.GetCommandName();
    }

    public override void Execute()
    {
        base.Execute();

        if (_property == null)
            return;

        var on = _setOn;

        if (_setOnFunc != null) on = _setOnFunc();

        if (on == null)
            return;

        if (_getValueAtExecuteTime)
        {
            var populatedNewValueWithRelationship = false;

            // If the property we are getting a value for is a foreign key ID field then we should show the user the compatible objects
            if (_property.GetCustomAttribute(typeof(RelationshipAttribute)) is RelationshipAttribute rel &&
                (_property.PropertyType == typeof(int) || _property.PropertyType == typeof(int?)))
                if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(rel.Cref))
                {
                    IMapsDirectlyToDatabaseTable[] available;

                    // is there a method that can be called to find compatible children for populating this property?
                    if (!string.IsNullOrWhiteSpace(rel.ValueGetter))
                    {
                        //get available from that method
                        var method = on.GetType().GetMethod(rel.ValueGetter, Type.EmptyTypes) ?? throw new Exception(
                            $"Could not find a method called '{rel.ValueGetter}' on Type '{on.GetType()}'.  This was specified as a ValueGetter on Property {_property.Name}");
                        try
                        {
                            available = ((IEnumerable<IMapsDirectlyToDatabaseTable>)method.Invoke(on, null)).ToArray();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                $"Error running method '{rel.ValueGetter}' on Type '{on.GetType()}'.  This was specified as a ValueGetter on Property {_property.Name}",
                                ex);
                        }
                    }
                    else
                    {
                        available = BasicActivator.GetAll(rel.Cref).ToArray();
                    }

                    NewValue = BasicActivator.SelectOne(_property.Name, available)?.ID;
                    populatedNewValueWithRelationship = true;
                }

            if (!populatedNewValueWithRelationship)
            {
                if (BasicActivator.SelectValueType(GetDialogArgs(on), _property.PropertyType, _property.GetValue(on),
                        out var chosen))
                {
                    NewValue = chosen;
                }
                else
                {
                    Success = false;
                    return;
                }
            }
        }

        ShareManager.SetValue(_property, NewValue, on);
        ((DatabaseEntity)on).SaveToDatabase();

        Success = true;
        Publish((DatabaseEntity)on);
    }

    private DialogArgs GetDialogArgs(IMapsDirectlyToDatabaseTable on)
    {
        if (DialogArgs != null)
            return DialogArgs;

        return on is ISqlParameter p && _property.Name.Equals(nameof(ISqlParameter.Value))
            ? AnyTableSqlParameter.GetValuePromptDialogArgs(p)
            : new DialogArgs
            {
                WindowTitle = $"Set value for '{_property.Name}'",
                EntryLabel = _property.Name
            };
    }
}