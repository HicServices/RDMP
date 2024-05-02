// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandSetUserSetting : BasicCommandExecution
{
    private readonly PropertyInfo _property;
    private readonly ErrorCode _errorCode;
    private readonly CheckResult _errorCodeValue;

    /// <summary>
    ///     The new value chosen by the user during command execution
    /// </summary>
    public object NewValue { get; }

    /// <summary>
    ///     True if the command was successfully completed
    /// </summary>
    public bool Success { get; private set; }

    [UseWithObjectConstructor]
    public ExecuteCommandSetUserSetting(IBasicActivateItems activator,
        [DemandsInitialization("Name of a property you want to change e.g. AllowIdentifiableExtractions")]
        string property,
        [DemandsInitialization(
            "New value to assign, this will be parsed into a valid Type if property is not a string")]
        string value) : base(activator)
    {
        // if user is calling to set an error code e.g. 'rdmp SetUserSetting R001 Success'
        var isCode = ErrorCodes.KnownCodes.FirstOrDefault(e => e.Code.Equals(property));

        if (isCode != null)
        {
            _errorCode = isCode;
            if (!Enum.TryParse<CheckResult>(value, out var result))
            {
                SetImpossible(
                    $"Invalid enum value.  When setting an error code you must supply a value of one of :{string.Join(',', Enum.GetNames<CheckResult>())}");
            }
            else
            {
                _errorCodeValue = result;
                NewValue = _errorCodeValue;
            }

            return;
        }

        _property = typeof(UserSettings).GetProperty(property, BindingFlags.Public | BindingFlags.Static);

        if (_property == null)
        {
            SetImpossible($"Unknown Property '{property}'");

            //suggest similar sounding properties
            var suggestions =
                typeof(UserSettings).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(c =>
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


    public override void Execute()
    {
        base.Execute();

        if (_errorCode != null)
        {
            UserSettings.SetErrorReportingLevelFor(_errorCode, _errorCodeValue);
            Success = true;
            return;
        }

        if (_property == null)
            return;

        ShareManager.SetValue(_property, NewValue, null);
        Success = true;
    }
}