// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;

/// <summary>
///     Stores all the user defined arguments of a ProcessTask (See DemandsInitializationAttribute) and all the runtime
///     arguments for the stage it is in
///     within the data load (See StageArgs).  For example an IAttacher of type DelimitedFlatFileAttacher could be declared
///     as a ProcessTask in the Mounting
///     stage of a DLE configuration (LoadMetadata).  At runtime a RuntimeArgumentCollection would be created with the user
///     specified IArguments (e.g. Delimiter
///     is comma, file pattern is *.csv) and the IStageArgs (where RAW database to load is and what the ForLoading
///     directory is where the flat files can be found).
///     <para>
///         This class is used by RuntimeTask to hydrate the hosted instance (IAttacher, IDataProvider, IMutilateDataTables
///         etc) dictated by the user (in the
///         ProcessTask).
///     </para>
/// </summary>
public class RuntimeArgumentCollection
{
    public IStageArgs StageSpecificArguments { get; set; }

    public HashSet<IArgument> Arguments { get; }

    /// <summary>
    ///     Transition from the arguments defined in the data source (Catalogue Database) into a runtime state
    /// </summary>
    public RuntimeArgumentCollection(IArgument[] args, IStageArgs stageSpecificArgs)
    {
        StageSpecificArguments = stageSpecificArgs;
        Arguments = new HashSet<IArgument>();

        if (!args.Any())
            return;

        foreach (var processTaskArgument in args)
            AddArgument(processTaskArgument);
    }

    public void IterateAllArguments(Func<string, object, bool> func)
    {
        foreach (var arg in Arguments) func(arg.Name, arg.GetValueAsSystemType());

        if (StageSpecificArguments == null)
            return;

        foreach (var arg in StageSpecificArguments.ToDictionary())
            func(arg.Key, arg.Value);
    }

    public void AddArgument(IArgument processTaskArgument)
    {
        Arguments.Add(processTaskArgument);
    }

    public object GetCustomArgumentValue(string name)
    {
        var first = Arguments.SingleOrDefault(a => a.Name.Equals(name)) ??
                    throw new KeyNotFoundException($"Argument {name} was missing");
        try
        {
            return first.GetValueAsSystemType();
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Could not convert value '{first.Value}' into a {first.GetSystemType().FullName} for argument '{first.Name}'",
                e);
        }
    }

    public IEnumerable<KeyValuePair<string, object>> GetAllArguments()
    {
        if (StageSpecificArguments != null)
            foreach (var kvp in StageSpecificArguments.ToDictionary())
                yield return kvp;

        foreach (var argument in Arguments)
            yield return new KeyValuePair<string, object>(argument.Name, argument.GetValueAsSystemType());
    }

    public IEnumerable<KeyValuePair<string, T>> GetAllArgumentsOfType<T>()
    {
        if (StageSpecificArguments != null)
            foreach (var kvp in StageSpecificArguments.ToDictionary())
                if (kvp.Value.GetType() == typeof(T))
                    yield return new KeyValuePair<string, T>(kvp.Key, (T)kvp.Value);

        foreach (var argument in Arguments)
            if (argument.GetSystemType() == typeof(T))
                yield return new KeyValuePair<string, T>(argument.Name, (T)argument.GetValueAsSystemType());
    }
}