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
    /// <summary>
    /// Constructs the respective <see cref="IRunner"/> based on the supplied <see cref="RDMPCommandLineOptions"/> Type
    /// </summary>
    public class RunnerFactory
    {
        public IRunner CreateRunner(RDMPCommandLineOptions command)
        {
            var dleOpts = command as DleOptions;
            var dqeOpts = command as DqeOptions;
            var cacheOpts = command as CacheOptions;
            var listOpts = command as ListOptions;

            if (dleOpts != null)
                return new DleRunner(dleOpts);

            if(dqeOpts != null)
                return new DqeRunner(dqeOpts);

            if(cacheOpts != null)
                return new CacheRunner(cacheOpts);

            if(listOpts != null)
                return new ListRunner(listOpts);

            throw new Exception("RDMPCommandLineOptions Type '" + command.GetType() + "'");
        }
    }
}
