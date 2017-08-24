using System;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceProcess;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CommandLine;

namespace CachingService
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CachingServiceOptions>(args);
            if (result.Errors.Any())
                Console.WriteLine(string.Join(",", result.Errors.Select(error => error.ToString())));

            var options = result.Value;

            // Override the default if requested
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException("Please need to supply a connection string (-s, --connection-string)");

            var connectionStringBulider = new SqlConnectionStringBuilder(options.ConnectionString);
            var catalogueRepository = new CatalogueRepository(connectionStringBulider);
            var cachingServiceProvider = new CachingServiceProvider(catalogueRepository);

            return options.Console ? RunInConsoleMode(cachingServiceProvider) : RunAsService(cachingServiceProvider);
        }

        private static int RunAsService(CachingServiceProvider cachingServiceProvider)
        {
            var servicesToRun = new ServiceBase[]
            {
                new CachingService(cachingServiceProvider)
            };

            ServiceBase.Run(servicesToRun);

            return 0;
        }

        private static int RunInConsoleMode(CachingServiceProvider cachingServiceProvider)
        {
            Console.WriteLine("Running in console mode");

            var listener = new CommonLoggingListener();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Stopping the caching provider");
                cachingServiceProvider.Stop(listener);
            };

            try
            {
                cachingServiceProvider.Start(null, listener);
                var task = cachingServiceProvider.Task;
                task.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                return 1;
            }

            return 0;
        }
    }
}
