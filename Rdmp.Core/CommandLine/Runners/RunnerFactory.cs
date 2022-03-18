// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;

namespace Rdmp.Core.CommandLine.Runners
{
    /// <summary>
    /// Constructs the respective <see cref="IRunner"/> based on the supplied <see cref="RDMPCommandLineOptions"/> Type
    /// </summary>
    public class RunnerFactory
    {
        public IRunner CreateRunner(IBasicActivateItems activator,RDMPCommandLineOptions command)
        {
            if (command.Command == CommandLineActivity.none)
                throw new Exception("No command has been set on '" + command.GetType().Name + "'");
            
            if (command is DleOptions dleOpts)
                return new DleRunner(dleOpts);

            if(command is DqeOptions dqeOpts)
                return new DqeRunner(dqeOpts);

            if(command is CacheOptions cacheOpts )
                return new CacheRunner(cacheOpts);
            
            if (command is ExtractionOptions extractionOpts )
                return new ExtractionRunner(activator,extractionOpts);

            if(command is ReleaseOptions releaseOpts )
                return new ReleaseRunner(releaseOpts);

            if (command is CohortCreationOptions cohortOpts )
                return new CohortCreationRunner(activator,cohortOpts);

            if(command is PackOptions packOpts )
                return new PackPluginRunner(packOpts);
            
            if(command is ExecuteCommandOptions executeOpts)
                return new ExecuteCommandRunner(executeOpts);
            
            throw new Exception("RDMPCommandLineOptions Type '" + command.GetType() + "'");
        }
        
    }
}
