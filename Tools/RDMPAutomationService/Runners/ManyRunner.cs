using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
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

        public Dictionary<ICheckable, ToMemoryCheckNotifier> ChecksDictionary { get; private set; }

        protected ManyRunner(ConcurrentRDMPCommandLineOptions options)
        {
            _options = options;
            ChecksDictionary = new Dictionary<ICheckable, ToMemoryCheckNotifier>();
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            RepositoryLocator = repositoryLocator;
            Token = token;
            List<Task> tasks = new List<Task>();

            Semaphore semaphore = null;
            if (_options.MaxConcurrentExtractions != null)
                semaphore = new Semaphore(0, _options.MaxConcurrentExtractions.Value);

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
                        }
                            ));
                    }

                    break;
                case CommandLineActivity.check:

                    ICheckable[] checkables = GetCheckables(checkNotifier);
                    foreach (ICheckable checkable in checkables)
                    {
                        if (semaphore != null)
                            semaphore.WaitOne();

                        ICheckable checkable1 = checkable;
                        var memory = new ToMemoryCheckNotifier(checkNotifier);
                        ChecksDictionary.Add(checkable1, memory);

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
                        }
                            ));
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
    }
}