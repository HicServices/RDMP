// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using System;
using System.Linq;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows;

internal class RunExtractionWindow : RunEngineWindow<ExtractionOptions>
{
    public RunExtractionWindow(IBasicActivateItems activator, ExtractionConfiguration ec) : base(activator,
        () => GetRunCommand(ec))
    {
    }

    private static ExtractionOptions GetRunCommand(ExtractionConfiguration ec) =>
        new()
        {
            ExtractionConfiguration = ec.ID.ToString(),
            ExtractGlobals = true
        };


    protected override void AdjustCommand(ExtractionOptions opts, CommandLineActivity activity)
    {
        base.AdjustCommand(opts, activity);

        var useCase = ExtractionPipelineUseCase.DesignTime();

        var compatible = useCase
            .FilterCompatiblePipelines(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>())
            .ToArray();

        if (!compatible.Any()) throw new Exception("No compatible pipelines");

        var pipe = BasicActivator.SelectOne("Extraction Pipeline", compatible, null, true) ??
                   throw new OperationCanceledException();
        opts.Pipeline = pipe.ID.ToString();
    }
}