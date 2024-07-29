// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

public abstract class ManyRunner : Runner
{
    private readonly ConcurrentRDMPCommandLineOptions _options;

    protected IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
    protected GracefulCancellationToken Token { get; private set; }

    private readonly Dictionary<ICheckable, ToMemoryCheckNotifier> _checksDictionary = new();

    /// <summary>
    /// Lock for all operations that read or write to <see cref="_checksDictionary"/>.  Use it if you want to enumerate / read the results
    /// </summary>
    private readonly object _oLock = new();

    protected ManyRunner(ConcurrentRDMPCommandLineOptions options)
    {
        _options = options;
    }

    public override int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null)
    {
        RepositoryLocator = repositoryLocator;
        Token = token;
        var tasks = new List<Task>();

        Semaphore semaphore = null;
        if (_options.MaxConcurrentExtractions != null)
            semaphore = new Semaphore(_options.MaxConcurrentExtractions.Value, _options.MaxConcurrentExtractions.Value);

        Initialize();

        switch (_options.Command)
        {
            case CommandLineActivity.none:
                break;
            case CommandLineActivity.run:

                var runnables = GetRunnables();

                foreach (var runnable in runnables)
                {
                    semaphore?.WaitOne();

                    var r = runnable;
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            ExecuteRun(r, new OverrideSenderIDataLoadEventListener(r.ToString(), listener));
                        }
                        finally
                        {
                            semaphore?.Release();
                        }
                    }));
                }

                break;
            case CommandLineActivity.check:

                lock (_oLock)
                {
                    _checksDictionary.Clear();
                }

                var checkables = GetCheckables(checkNotifier);
                foreach (var checkable in checkables)
                {
                    semaphore?.WaitOne();

                    var checkable1 = checkable;
                    var memory = new ToMemoryCheckNotifier(checkNotifier);

                    lock (_oLock)
                    {
                        _checksDictionary.Add(checkable1, memory);
                    }

                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            checkable1.Check(memory);
                        }
                        finally
                        {
                            semaphore?.Release();
                        }
                    }));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Task.WaitAll(tasks.ToArray());

        AfterRun();

        return 0;
    }

    protected abstract void Initialize();
    protected abstract void AfterRun();

    protected abstract ICheckable[] GetCheckables(ICheckNotifier checkNotifier);

    protected abstract object[] GetRunnables();
    protected abstract void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener);


    /// <summary>
    /// Returns the ToMemoryCheckNotifier that corresponds to the given checkable Type (of which there must only be one in the dictionary e.g. a globals checker).
    /// 
    /// <para>Use GetCheckerResults to get multiple </para>
    /// 
    /// <para>Returns null if there are no checkers of the given Type</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException">Thrown if there are more than 1 ICheckable of the type T</exception>
    /// <returns></returns>
    protected ToMemoryCheckNotifier GetSingleCheckerResults<T>() where T : ICheckable
    {
        return GetSingleCheckerResults<T>(s => true);
    }

    /// <summary>
    /// Returns the ToMemoryCheckNotifier that corresponds to the given checkable Type which matches the func.
    /// 
    /// <para>Returns null if there are no checkers of the given Type matching the func</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException">Thrown if there are more than 1 ICheckable of the type T</exception>
    /// <returns></returns>
    protected ToMemoryCheckNotifier GetSingleCheckerResults<T>(Func<T, bool> func) where T : ICheckable
    {
        lock (_oLock)
        {
            var arr = GetCheckerResults(func);

            if (arr.Length == 0)
                return null;

            return arr.Length == 1
                ? arr[0].Value
                : throw new InvalidOperationException($"There were {arr.Length} Checkers of type {typeof(T)}");
        }
    }

    /// <summary>
    /// Returns the results for the given <see cref="ICheckable"/> type which match the function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    protected KeyValuePair<ICheckable, ToMemoryCheckNotifier>[] GetCheckerResults<T>(Func<T, bool> func)
        where T : ICheckable
    {
        lock (_oLock)
        {
            return GetCheckerResults<T>().Where(kvp => func((T)kvp.Key)).ToArray();
        }
    }

    protected KeyValuePair<ICheckable, ToMemoryCheckNotifier>[] GetCheckerResults<T>() where T : ICheckable
    {
        lock (_oLock)
        {
            return _checksDictionary.Where(kvp => kvp.Key is T).ToArray();
        }
    }
}