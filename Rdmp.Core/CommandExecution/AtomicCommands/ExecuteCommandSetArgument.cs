// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Sets the value of a named parameter on an RDMP module e.g. <see cref="ProcessTask"/>
/// </summary>
public class ExecuteCommandSetArgument : BasicCommandExecution
{
    private readonly IArgument _arg;
    private readonly object _value;

    private readonly bool _promptUser;

    public ExecuteCommandSetArgument(IBasicActivateItems activator, IArgumentHost _, IArgument arg, object value) :
        base(activator)
    {
        _arg = arg;
        _value = value;
    }


    /// <summary>
    /// Interactive constructor that prompts for value at execution time
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public ExecuteCommandSetArgument(IBasicActivateItems activator, IArgument arg) : base(activator)
    {
        _arg = arg;
        _promptUser = true;

        if (!activator.IsInteractive)
            SetImpossible("Activator does not support interactive mode");
    }

    /// <summary>
    /// Automatic/Unattended constructor, construction values will come from <paramref name="picker"/>
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="picker"></param>
    [UseWithCommandLine(
        ParameterHelpList = "<component> <argName> <argValue>",
        ParameterHelpBreakdown = @"component    Module to set value on e.g. ProcessTask:1
argName Name of an argument to set on the component e.g. Retry
argValue    New value for argument e.g. Null, True, Catalogue:5 etc")]
    public ExecuteCommandSetArgument(IBasicActivateItems activator, CommandLineObjectPicker picker) : base(activator)
    {
        if (picker.Arguments.Count != 3)
        {
            SetImpossible(
                $"Wrong number of parameters supplied to command, expected 3 but got {picker.Arguments.Count}");
            return;
        }

        if (!picker.HasArgumentOfType(0, typeof(IMapsDirectlyToDatabaseTable)))
        {
            SetImpossible("First parameter must be an IArgumentHost DatabaseEntity");
            return;
        }


        if (picker[0].GetValueForParameterOfType(typeof(IMapsDirectlyToDatabaseTable)) is not IArgumentHost host)
        {
            SetImpossible("First parameter must be an IArgumentHost");
            return;
        }

        var args = host.GetAllArguments().ToList();

        _arg = args.FirstOrDefault(a => a.Name.Equals(picker[1].RawValue));

        if (_arg == null)
        {
            SetImpossible(
                $"Could not find argument called '{picker[1].RawValue}' on '{host}'.  Arguments found were {string.Join(",", args.Select(a => a.Name))}");
            return;
        }

        Type argType;

        try
        {
            argType = _arg.GetConcreteSystemType();
        }
        catch (Exception e)
        {
            SetImpossible($"Failed to get system Type of argument:{e}");
            return;
        }

        if (argType == null)
        {
            SetImpossible($"Argument '{_arg.Name}' has no listed Type");
            return;
        }


        if (!picker[2].HasValueOfType(argType))
        {
            SetImpossible(
                $"Provided value '{picker[2].RawValue}' does not match expected Type '{argType.Name}' of argument '{_arg.Name}'");
            return;
        }

        _value = picker[2].GetValueForParameterOfType(argType);
    }

    public override void Execute()
    {
        base.Execute();

        var value = _value;

        if (_promptUser)
        {
            var invoker = new CommandInvoker(BasicActivator);
            try
            {
                value = invoker.GetValueForParameterOfType(new RequiredArgument(_arg));
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        _arg.SetValue(value);
        _arg.SaveToDatabase();

        if (_arg is DatabaseEntity de)
            Publish(de);
    }
}