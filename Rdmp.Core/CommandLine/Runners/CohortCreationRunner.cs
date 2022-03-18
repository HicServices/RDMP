// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners
{
    /// <summary>
    /// Runner for Cohort Creation Tasks.
    /// </summary>
    public class CohortCreationRunner : IRunner
    {
        private readonly CohortCreationOptions _options;
        private readonly IBasicActivateItems _activator;
        private ExtractionConfiguration _configuration;

        public CohortCreationRunner(IBasicActivateItems activator, CohortCreationOptions options)
        {
            _activator = activator;
            _options = options;
            _configuration = _options.GetRepositoryLocator().DataExportRepository.GetObjectByID<ExtractionConfiguration>(_options.ExtractionConfiguration);
            _activator = activator;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            if (HasConfigurationPreviouslyBeenReleased())
                throw new Exception("Extraction Configuration has already been released");

            if (_options.Command == CommandLineActivity.run)
            {
                var engine = new CohortRefreshEngine(_activator, new ThrowImmediatelyDataLoadEventListener(), _configuration);
                engine.Execute();
            }

            return 0;
        }

        private bool HasConfigurationPreviouslyBeenReleased()
        {
            var previouslyReleasedStuff = _configuration.ReleaseLog;

            if (previouslyReleasedStuff.Any())
                return true;

            return false;
        }
    }
}
