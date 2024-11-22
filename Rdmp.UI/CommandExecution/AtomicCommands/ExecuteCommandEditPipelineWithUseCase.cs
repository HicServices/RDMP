// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandEditPipelineWithUseCase : BasicUICommandExecution
{
    private readonly Pipeline _pipeline;
    private readonly PipelineUseCase _useCase;

    public ExecuteCommandEditPipelineWithUseCase(IActivateItems itemActivator, Pipeline pipeline,
        PipelineUseCase useCase) : base(itemActivator)
    {
        _pipeline = pipeline;
        _useCase = useCase;
    }

    public override void Execute()
    {
        base.Execute();

        //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
        var dialog = new ConfigurePipelineUI(Activator, _pipeline, _useCase,
            Activator.RepositoryLocator.CatalogueRepository);
        dialog.ShowDialog();

        Publish(_pipeline);
    }
}