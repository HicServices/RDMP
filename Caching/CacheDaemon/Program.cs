using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CachingEngine;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CacheDaemon
{
    class Program
    {
        static int Main(string[] args)
        {
            // Expects comma-separated list of Cache Progress IDs
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: CacheDaemon <List of CacheProgress IDs>");
                return 1;
            }

            var locator = new RegistryRepositoryFinder();
            var startup = new Startup(locator);
            startup.DoStartup(new ThrowImmediatelyCheckNotifier());

            var progressIDList = args[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => Convert.ToInt32(p));

            // load the CacheProgress objects
            List<ICacheProgress> cacheProgressList = new List<ICacheProgress>(locator.CatalogueRepository.GetAllObjectsInIDList<CacheProgress>(progressIDList));

            var cachingHost = new CachingHost(locator.CatalogueRepository)
            {
                CacheProgressList = cacheProgressList
            };

            var listener = new ToConsoleDataLoadEventReceiver();
            var cancellationTokenSource = new GracefulCancellationTokenSource();
            var task = Task.Run(() => cachingHost.StartDaemon(listener, cancellationTokenSource.Token));

            Console.ReadLine();

            Console.WriteLine("Aborting...");
            cancellationTokenSource.Abort();
            task.Wait();

            return 0;
        }
    }
}
