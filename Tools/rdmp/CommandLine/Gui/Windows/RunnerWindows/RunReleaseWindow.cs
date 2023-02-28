// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using System;
using System.Linq;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows
{
    class RunReleaseWindow : RunEngineWindow<ReleaseOptions>
    {
        private IExtractionConfiguration[] configs;

        public RunReleaseWindow(IBasicActivateItems activator, IProject project):base(activator,()=>new ReleaseOptions())
        {
            configs = project.ExtractionConfigurations.Where(c => !c.IsReleased).ToArray();
        }
        public RunReleaseWindow(IBasicActivateItems activator, ExtractionConfiguration ec) : base(activator, () => new ReleaseOptions())
        {
            configs = new IExtractionConfiguration[] { ec };
        }


        protected override void AdjustCommand(ReleaseOptions opts, CommandLineActivity activity)
        {
            base.AdjustCommand(opts, activity);

            var useCase = new ReleaseUseCase();

            var compatible = useCase.FilterCompatiblePipelines(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>()).ToArray();

            if (!compatible.Any())
            {
                throw new Exception("No compatible pipelines");
            }

            var pipe = BasicActivator.SelectOne("Release Pipeline", compatible, null, true);

            if (pipe == null)
            {
                throw new OperationCanceledException();
            }

            opts.Pipeline = pipe.ID.ToString();
            opts.Configurations = string.Join(",",configs.Select(c=>c.ID.ToString()).ToArray());
            opts.ReleaseGlobals = true;

            // all datasets
            opts.SelectedDataSets = null;
        }
    }
}
