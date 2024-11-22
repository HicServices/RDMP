// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandClonePipeline : BasicCommandExecution
{
    private readonly Pipeline _pipeline;

    public ExecuteCommandClonePipeline(IBasicActivateItems activator, Pipeline pipeline)
        : base(activator)
    {
        _pipeline = pipeline;
        if (_pipeline == null)
            SetImpossible("You can only clone an existing pipeline");
    }

    public override void Execute()
    {
        base.Execute();

        _pipeline.Clone();
        Publish(_pipeline);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Pipeline, OverlayKind.Link);
}