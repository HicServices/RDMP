// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using RDMPStartup.Options;
using RDMPStartup.Options.Abstracts;

namespace RDMPStartup.Runners
{
    /// <summary>
    /// Constructs the respective <see cref="IRunner"/> based on the supplied <see cref="RDMPCommandLineOptions"/> Type
    /// </summary>
    public class RunnerFactory
    {
        public IRunner CreateRunner(RDMPCommandLineOptions command)
        {
            if (command.Command == CommandLineActivity.none)
                throw new Exception("No command has been set on '" + command.GetType().Name + "'");

            var dleOpts = command as DleOptions;
            var dqeOpts = command as DqeOptions;
            var cacheOpts = command as CacheOptions;
            var listOpts = command as ListOptions;
            var extractionOpts = command as ExtractionOptions;
            var releaseOpts = command as ReleaseOptions;
            var cohortOpts = command as CohortCreationOptions;

            if (dleOpts != null)
                return new DleRunner(dleOpts);

            if(dqeOpts != null)
                return new DqeRunner(dqeOpts);

            if(cacheOpts != null)
                return new CacheRunner(cacheOpts);

            if(listOpts != null)
                return new ListRunner(listOpts);

            if (extractionOpts != null)
                return new ExtractionRunner(extractionOpts);

            if(releaseOpts != null)
                return new ReleaseRunner(releaseOpts);

            if (cohortOpts != null)
                return new CohortCreationRunner(cohortOpts);
            
            throw new Exception("RDMPCommandLineOptions Type '" + command.GetType() + "'");
        }
        
    }
}
