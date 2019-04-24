// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.DataFlowPipeline;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.Core.CommandLine.Options.Abstracts;
using Rdmp.Core.DataExport.CohortCreationPipeline;
using Rdmp.Core.DataExport.Data.DataTables;
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
        private ExtractionConfiguration _configuration;

        public CohortCreationRunner(CohortCreationOptions options)
        {
            _options = options;
            _configuration = _options.GetRepositoryLocator().DataExportRepository.GetObjectByID<ExtractionConfiguration>(_options.ExtractionConfiguration);
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            if (HasConfigurationPreviouslyBeenReleased())
                throw new Exception("Extraction Configuration has already been released");

            if (_options.Command == CommandLineActivity.run)
            {
                var engine = new CohortRefreshEngine(new ThrowImmediatelyDataLoadEventListener(), _configuration);
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
