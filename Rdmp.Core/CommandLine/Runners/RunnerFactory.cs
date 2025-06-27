// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
/// Constructs the respective <see cref="IRunner"/> based on the supplied <see cref="RDMPCommandLineOptions"/> Type
/// </summary>
public class RunnerFactory
{
    public static IRunner CreateRunner(IBasicActivateItems activator, RDMPCommandLineOptions command)
    {
        return command.Command == CommandLineActivity.none
            ? throw new Exception($"No command has been set on '{command.GetType().Name}'")
            : command switch
            {
                DleOptions dleOpts => new DleRunner(activator,dleOpts),
                DqeOptions dqeOpts => new DqeRunner(dqeOpts),
                AnalyticsOptions analyticsOpt => new AnalyticsRunner(analyticsOpt),
                CacheOptions cacheOpts => new CacheRunner(cacheOpts),
                ExtractionOptions extractionOpts => new ExtractionRunner(activator, extractionOpts),
                ReleaseOptions releaseOpts => new ReleaseRunner(activator,releaseOpts),
                CohortCreationOptions cohortOpts => new CohortCreationRunner(cohortOpts),
                PackOptions packOpts => new PackPluginRunner(packOpts),
                ExecuteCommandOptions executeOpts => new ExecuteCommandRunner(executeOpts),
                _ => throw new Exception($"RDMPCommandLineOptions Type '{command.GetType()}'")
            };
    }
}