// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution;

public class CommandInvoker
{
    private readonly IBasicActivateItems _basicActivator;
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

    /// <summary>
    /// Delegates provided by <see cref="_basicActivator"/> for fulfilling constructor arguments of the key Type
    /// </summary>
    private List<CommandInvokerDelegate> _argumentDelegates;

    /// <summary>
    /// Called when the user attempts to run a command marked <see cref="ICommandExecution.IsImpossible"/>
    /// </summary>
    public event EventHandler<CommandEventArgs> CommandImpossible;

    /// <summary>
    /// Called when a command completes successfully
    /// </summary>
    public event EventHandler<CommandEventArgs> CommandCompleted;

    public CommandInvoker(IBasicActivateItems basicActivator)
    {
        _basicActivator = basicActivator;
        _repositoryLocator = basicActivator.RepositoryLocator;

        _argumentDelegates = _basicActivator.GetDelegates();


        AddDelegate(typeof(ICatalogueRepository), true, p => _repositoryLocator.CatalogueRepository);
        AddDelegate(typeof(IDataExportRepository), true, p => _repositoryLocator.DataExportRepository);
        AddDelegate(_basicActivator.GetType(), true, p => _basicActivator);
        AddDelegate(typeof(IRDMPPlatformRepositoryServiceLocator), true, p => _repositoryLocator);
        AddDelegate(typeof(DirectoryInfo), false,
            p => _basicActivator.SelectDirectory($"Enter Directory for '{p.Name}'"));
        AddDelegate(typeof(FileInfo), false, p => _basicActivator.SelectFile($"Enter File for '{p.Name}'"));

        // Is the Global Check Notifier the best here?
        AddDelegate(typeof(ICheckNotifier), true, p => _basicActivator.GlobalErrorCheckNotifier);

        AddDelegate(typeof(Uri), false, p => new Uri(SelectText(p)));
        AddDelegate(typeof(Regex), false, p => new Regex(SelectText(p)));

        AddDelegate(typeof(string), false, SelectText);

        AddDelegate(typeof(Type), false, p =>
            _basicActivator.SelectType(
                new DialogArgs
                {
                    WindowTitle = $"Type needed for {p.Name}",
                    InitialSearchText = p.DefaultValue?.ToString()
                }, p.DemandIfAny?.TypeOf, out var chosen)
                ? chosen
                : throw new OperationCanceledException());

        AddDelegate(typeof(DiscoveredDatabase), false, p => _basicActivator.SelectDatabase(true, GetPromptFor(p)));
        AddDelegate(typeof(DiscoveredTable), false, p => _basicActivator.SelectTable(true, GetPromptFor(p)));

        AddDelegate(typeof(DatabaseEntity), false, p =>
            _basicActivator.SelectOne(
                new DialogArgs
                {
                    WindowTitle = GetPromptFor(p),
                    InitialObjectSelection = p.DefaultValue is IMapsDirectlyToDatabaseTable m
                        ? new IMapsDirectlyToDatabaseTable[] { m }
                        : null,
                    InitialSearchText = p.DefaultValue?.ToString()
                }, GetAllObjectsOfType(p.Type)));

        AddDelegate(typeof(IPipeline), false, SelectPipeline);
        AddDelegate(typeof(IMightBeDeprecated), false, SelectOne<IMightBeDeprecated>, true);
        AddDelegate(typeof(IDisableable), false, SelectOne<IDisableable>, true);
        AddDelegate(typeof(INamed), false, SelectOne<INamed>, true);
        AddDelegate(typeof(IDeleteable[]), false, SelectMany<IDeleteable>, true);
        AddDelegate(typeof(IDeleteable), false, SelectOne<IDeleteable>, true);
        AddDelegate(typeof(ILoggedActivityRootObject), false, SelectOne<ILoggedActivityRootObject>);
        AddDelegate(typeof(ICollectSqlParameters), false, SelectOne<ICollectSqlParameters>);
        AddDelegate(typeof(IRootFilterContainerHost), false, SelectOne<IRootFilterContainerHost>);

        AddDelegate(typeof(Enum), false, p => _basicActivator.SelectEnum(
            new DialogArgs
            {
                WindowTitle = GetPromptFor(p),
                InitialSearchText = p.DefaultValue?.ToString()
            }, p.Type, out var chosen)
            ? chosen
            : null);


        _argumentDelegates.Add(new CommandInvokerArrayDelegate(typeof(IMapsDirectlyToDatabaseTable), false, p =>
        {
            var available = GetAllObjectsOfType(p.Type.GetElementType());
            var result = _basicActivator.SelectMany(
                new DialogArgs
                {
                    WindowTitle = GetPromptFor(p),
                    InitialObjectSelection = p.DefaultValue == null || p.DefaultValue == DBNull.Value
                        ? null
                        : ((IEnumerable<IMapsDirectlyToDatabaseTable>)p.DefaultValue).ToArray()
                }, p.Type.GetElementType(), available);

            if (result == null)
                return null;

            var typedArray = Array.CreateInstance(p.Type.GetElementType(), result.Length);
            for (var i = 0; i < typedArray.Length; i++)
                typedArray.SetValue(result[i], i);

            return typedArray;
        }));


        AddDelegate(typeof(ICheckable), false,
            p => _basicActivator.SelectOne(GetPromptFor(p),
                _basicActivator.GetAll<ICheckable>()
                    .Where(p.Type.IsInstanceOfType)
                    .Cast<IMapsDirectlyToDatabaseTable>()
                    .ToArray()), true);

        // if we aren't asking for any of the above explicit interfaces (e.g. get user to pick an IDeletable)
        // then we might be something like IProject so let them pick any
        AddDelegate(typeof(IMapsDirectlyToDatabaseTable), false,
            p => _basicActivator.SelectOne(GetPromptFor(p),
                _basicActivator.GetAll<IMapsDirectlyToDatabaseTable>()
                    .Where(p.Type.IsInstanceOfType)
                    .ToArray()));

        AddDelegate(typeof(IPatcher), false, p =>
            {
                if (!_basicActivator.SelectType("Select Patcher (if any)", typeof(IPatcher), out var patcherType))
                    throw new OperationCanceledException();

                if (patcherType == null)
                    return null;

                try
                {
                    return Activator.CreateInstance(patcherType);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to call/find blank constructor of IPatcher Type '{patcherType}'", e);
                }
            }
        );

        _argumentDelegates.Add(new CommandInvokerValueTypeDelegate(p =>
            _basicActivator.SelectValueType(GetPromptFor(p), p.Type, p.DefaultValue, out var chosen)
                ? chosen
                : throw new OperationCanceledException()));
    }

    private string SelectText(RequiredArgument p) =>
        _basicActivator.TypeText(
            new DialogArgs
            {
                WindowTitle = "Value needed for parameter",
                EntryLabel = GetPromptFor(p)
            }
            , 1000, p.DefaultValue?.ToString(), out var result, false)
            ? result
            : throw new OperationCanceledException();

    private IPipeline SelectPipeline(RequiredArgument arg) => (IPipeline)_basicActivator.SelectOne(GetPromptFor(arg),
        _basicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>().ToArray());

    private static string GetPromptFor(RequiredArgument p) => $"Value needed for {p.Name} ({p.Type.Name})";

    private void AddDelegate(Type type, bool isAuto, Func<RequiredArgument, object> func,
        bool requireExactMatch = false)
    {
        _argumentDelegates.Add(new CommandInvokerDelegate(type, isAuto, func)
        {
            RequireExactMatch = requireExactMatch
        });
    }


    public IEnumerable<Type> GetSupportedCommands()
    {
        return MEF.GetAllTypes()?.Where(t => WhyCommandNotSupported(t) is null) ??
               throw new Exception("MEF property has not been initialized on the activator");
    }

    /// <summary>
    /// Constructs an instance of the <see cref="IAtomicCommand"/> and executes it.  Constructor parameters
    /// are populated from the (optional) <paramref name="picker"/> or the <see cref="IBasicActivateItems"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="picker"></param>
    public void ExecuteCommand(Type type, CommandLineObjectPicker picker)
    {
        ExecuteCommand(GetConstructor(type, picker), picker);
    }

    private void ExecuteCommand(ConstructorInfo constructorInfo, CommandLineObjectPicker picker)
    {
        var parameterValues = new List<object>();
        var complainAboutExtraParameters = true;

        var idx = 0;

        //for each parameter on the constructor we want to invoke
        foreach (var parameterInfo in constructorInfo.GetParameters())
        {
            var required = new RequiredArgument(parameterInfo);

            var argDelegate = GetDelegate(required);

            //if it is an easy one to automatically fill e.g. IBasicActivateItems
            if (argDelegate is { IsAuto: true })
            {
                parameterValues.Add(argDelegate.Run(required));
            }
            else
            //if the constructor argument is a picker, use the one passed in
            if (parameterInfo.ParameterType == typeof(CommandLineObjectPicker))
            {
                if (picker == null)
                    throw new ArgumentException(
                        $"Type {constructorInfo.DeclaringType} contained a constructor which took an {parameterInfo.ParameterType} but no picker was passed");

                parameterValues.Add(picker);

                //the parameters are expected to be consumed by the target constructors so its not really a problem if there are extra
                complainAboutExtraParameters = false;
            }
            else
            //if we have argument values specified
            if (picker != null)
            {
                //and the specified value matches the expected parameter type
                if (picker.HasArgumentOfType(idx, parameterInfo.ParameterType))
                {
                    //consume a value
                    parameterValues.Add(picker[idx].GetValueForParameterOfType(parameterInfo.ParameterType));
                    idx++;
                    continue;
                }

                // if user has not typed anything in for this parameter and it has a default value
                if (picker.Length <= idx && parameterInfo.HasDefaultValue)
                {
                    // then we should use the default value
                    parameterValues.Add(parameterInfo.DefaultValue);
                    idx++;
                    continue;
                }

                throw new Exception(
                    $"Expected parameter at index {idx} to be a {parameterInfo.ParameterType} (for parameter '{parameterInfo.Name}') but it was {(idx >= picker.Length ? "Missing" : picker[idx].RawValue)}");
            }
            else
            {
                parameterValues.Add(GetValueForParameterOfType(parameterInfo));
            }
        }

        if (picker != null && idx < picker.Length && complainAboutExtraParameters)
            throw new Exception($"Unrecognised extra parameter {picker[idx].RawValue}");

        var instance = (IAtomicCommand)constructorInfo.Invoke(parameterValues.ToArray());

        if (instance.IsImpossible)
        {
            CommandImpossible?.Invoke(this, new CommandEventArgs(instance));
            return;
        }

        instance.Execute();
        CommandCompleted?.Invoke(this, new CommandEventArgs(instance));
    }

    public object GetValueForParameterOfType(PropertyInfo propertyInfo) =>
        GetValueForParameterOfType(new RequiredArgument(propertyInfo));

    public object GetValueForParameterOfType(ParameterInfo parameterInfo) =>
        GetValueForParameterOfType(new RequiredArgument(parameterInfo));

    public object GetValueForParameterOfType(RequiredArgument a) => GetDelegate(a)?.Run(a);

    private T SelectOne<T>(RequiredArgument p)
    {
        return (T)_basicActivator.SelectOne(
            new DialogArgs
            {
                WindowTitle = GetPromptFor(p),
                InitialObjectSelection = p.DefaultValue is IMapsDirectlyToDatabaseTable m
                    ? new IMapsDirectlyToDatabaseTable[] { m }
                    : null,
                InitialSearchText = p.DefaultValue?.ToString()
            }
            , _basicActivator.GetAll(p.Type).Cast<IMapsDirectlyToDatabaseTable>().ToArray());
    }

    private T[] SelectMany<T>(RequiredArgument p) =>
        _basicActivator.SelectMany(
                new DialogArgs
                {
                    WindowTitle = p.Name,
                    InitialObjectSelection =
                        ((IEnumerable<T>)p.DefaultValue)?.Cast<IMapsDirectlyToDatabaseTable>().ToArray()
                }, typeof(T), _basicActivator.GetAll(p.Type).Cast<IMapsDirectlyToDatabaseTable>().ToArray())
            ?.Cast<T>()?.ToArray() ?? throw new OperationCanceledException();

    public string WhyCommandNotSupported(ConstructorInfo c) =>
        c.GetCustomAttribute<UseWithCommandLineAttribute>() != null
            ? null
            : c.GetParameters().Select(WhyCommandNotSupported).SkipWhile(string.IsNullOrEmpty).FirstOrDefault();

    public string WhyCommandNotSupported(ParameterInfo p) =>
        GetDelegate(new RequiredArgument(p)) != null ? "" : $"No delegate for {p.ParameterType}";

    private readonly ConcurrentDictionary<RequiredArgument, CommandInvokerDelegate> _delegateCache =
        new();

    public CommandInvokerDelegate GetDelegate(RequiredArgument argument) =>
        _delegateCache.GetOrAdd(argument, GetDelegateCacheMiss);

    private CommandInvokerDelegate GetDelegateCacheMiss(RequiredArgument required)
    {
        // Special-case IMapsDirectlyToDatabaseTable and Enum, because the pickers can generate arbitrary subtypes of that

        CommandInvokerDelegate match;
        if (required.Type.IsAssignableTo(typeof(IMapsDirectlyToDatabaseTable)))
            match = _argumentDelegates.FirstOrDefault(k => k.CanHandle(typeof(IMapsDirectlyToDatabaseTable)));
        else if (required.Type.IsAssignableTo(typeof(Enum)))
            match = _argumentDelegates.FirstOrDefault(k => k.CanHandle(typeof(Enum)));
        else
            match = _argumentDelegates.FirstOrDefault(k => k.CanHandle(required.Type));

        if (match != null)
            // prefer delegate if no user input required or running in interactive mode
            if (match.IsAuto || _basicActivator.IsInteractive)
                return match;


        // use the default value (preferred if non interactive)
        if (required.HasDefaultValue)
            return new CommandInvokerFixedValueDelegate(required.Type, required.DefaultValue);

        // return delegate anyway (could be null)
        return match;
    }

    public string WhyCommandNotSupported(Type t)
    {
        if (t.IsAbstract)
            return "Abstract";
        if (t.IsInterface)
            return "Interface";
        if (!typeof(IAtomicCommand).IsAssignableFrom(t))
            return "Not a command";

        if (_basicActivator.GetIgnoredCommands().Contains(t))
            return "Ignored";

        try
        {
            var constructor = GetConstructor(t, new CommandLineObjectPicker(Array.Empty<string>(), _basicActivator));

            return constructor == null ? "No constructor" : WhyCommandNotSupported(constructor);
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    /// <summary>
    /// Returns the first best constructor on the <paramref name="type"/> preferring those decorated with <see cref="UseWithObjectConstructorAttribute"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual ConstructorInfo GetConstructor(Type type) => GetConstructor(type, null);

    /// <summary>
    /// Returns the first best constructor on the <paramref name="type"/> preferring those decorated with <see cref="UseWithCommandLineAttribute"/>
    /// </summary>
    /// <param name="type">The type of command you want to fetch the constructor from</param>
    /// <param name="picker">The command line arguments that you want to use to hydrate the <paramref name="type"/> constructor</param>
    /// <returns></returns>
    public virtual ConstructorInfo GetConstructor(Type type, CommandLineObjectPicker picker = null)
    {
        var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (constructors.Length == 0)
            return null;

        ConstructorInfo[] importDecorated = null;

        //If we have a picker, look for a constructor that wants to run from the command line
        if (picker != null)
            importDecorated = constructors.Where(c => Attribute.IsDefined(c, typeof(UseWithCommandLineAttribute)))
                .ToArray();

        //otherwise look for a regular decorated constructor
        if (importDecorated == null || !importDecorated.Any())
            importDecorated = constructors.Where(c => Attribute.IsDefined(c, typeof(UseWithObjectConstructorAttribute)))
                .ToArray();

        return importDecorated.Any() ? importDecorated[0] : constructors[0];
    }

    private IMapsDirectlyToDatabaseTable[] GetAllObjectsOfType(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
            return _basicActivator.GetAll(type).ToArray();

        if (_repositoryLocator.CatalogueRepository.SupportsObjectType(type))
            return _repositoryLocator.CatalogueRepository.GetAllObjects(type).ToArray();
        return _repositoryLocator.DataExportRepository.SupportsObjectType(type)
            ? _repositoryLocator.DataExportRepository.GetAllObjects(type).ToArray()
            : null;
    }
}