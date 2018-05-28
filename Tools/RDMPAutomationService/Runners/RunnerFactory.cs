using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using RDMPAutomationService.Options;

namespace RDMPAutomationService.Runners
{
    public class RunnerFactory
    {
        public IRunner CreateRunner(RDMPCommandLineOptions command)
        {
            var dleOpts = command as DleOptions;
            var dqeOpts = command as DqeOptions;
            var cacheOpts = command as CacheOptions;

            if (dleOpts != null)
                return new DleRunner(dleOpts);

            if(dqeOpts != null)
                return new DqeRunner(dqeOpts);

            if(cacheOpts != null)
                return new CacheRunner(cacheOpts);

            throw new Exception("RDMPCommandLineOptions Type '" + command.GetType() + "'");
        }


        //todo ListOptions
        /*case CommandLineActivity.list:

                    Console.WriteLine(string.Format("[ID] - Name"));

                    if (_options.LoadMetadata != 0)
                    {
                        var l = locator.CatalogueRepository.GetObjectByID<LoadMetadata>(_options.LoadMetadata);
                        Console.WriteLine("[{0}] - {1}", l.ID, l.Name);
                    }
                    else
                    {
                        foreach (LoadMetadata l in locator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                            Console.WriteLine("[{0}] - {1}", l.ID, l.Name);
                    }

                    return 0;*/
    }
}
