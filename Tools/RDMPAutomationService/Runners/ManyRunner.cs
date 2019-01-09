using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Checks;
using HIC.Logging.Listeners;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    public abstract class ManyRunner:IRunner
    {
        private readonly ConcurrentRDMPCommandLineOptions _options;

        protected IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        protected GracefulCancellationToken Token { get;private set; }

        private readonly Dictionary<ICheckable, ToMemoryCheckNotifier> _checksDictionary = new Dictionary<ICheckable, ToMemoryCheckNotifier>();

        /// <summary>
        /// Lock for all operations that read or write to <see cref="_checksDictionary"/>.  Use it if you want to enumerate / read the results
        /// </summary>
        private readonly object _oLock = new object();

        protected ManyRunner(ConcurrentRDMPCommandLineOptions options)
        {
            _options = options;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            RepositoryLocator = repositoryLocator;
            Token = token;
            List<Task> tasks = new List<Task>();

            Semaphore semaphore = null;
            if (_options.MaxConcurrentExtractions != null)
                semaphore = new Semaphore(_options.MaxConcurrentExtractions.Value, _options.MaxConcurrentExtractions.Value);

            Initialize();
            
            switch (_options.Command)
            {
                case CommandLineActivity.none:
                    break;
                case CommandLineActivity.run:
                        
                    object[] runnables = GetRunnables();

                    foreach (object runnable in runnables)
                    {
                        if (semaphore != null)
                            semaphore.WaitOne();

                        object r = runnable;
                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                ExecuteRun(r, new OverrideSenderIDataLoadEventListener(r.ToString(), listener));
                            }
                            finally
                            {
                                if (semaphore != null)
                                    semaphore.Release();
                            }
                        }));
                    }

                    break;
                case CommandLineActivity.check:

                    lock (_oLock)
                        _checksDictionary.Clear();

                    ICheckable[] checkables = GetCheckables(checkNotifier);
                    foreach (ICheckable checkable in checkables)
                    {
                        if (semaphore != null)
                            semaphore.WaitOne();

                        ICheckable checkable1 = checkable;
                        var memory = new ToMemoryCheckNotifier(checkNotifier);

                        lock (_oLock)
                            _checksDictionary.Add(checkable1, memory);

                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                checkable1.Check(memory);
                            }
                            finally
                            {
                                if (semaphore != null)
                                    semaphore.Release();
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

                if (arr.Length == 1)
                    return arr[0].Value;

                throw new InvalidOperationException("There were " + arr.Length + " Checkers of type " + typeof(T));
            }
        }

        /// <summary>
        /// Returns the results for the given <see cref="ICheckable"/> type which match the function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected KeyValuePair<ICheckable, ToMemoryCheckNotifier>[] GetCheckerResults<T>(Func<T, bool> func) where T : ICheckable
        {
            lock (_oLock)
                return GetCheckerResults<T>().Where(kvp=>func((T) kvp.Key)).ToArray();
        }

        protected KeyValuePair<ICheckable, ToMemoryCheckNotifier>[] GetCheckerResults<T>() where T:ICheckable
        {
            lock (_oLock)
                return _checksDictionary.Where(kvp => kvp.Key is T).ToArray();
        }
    }
}