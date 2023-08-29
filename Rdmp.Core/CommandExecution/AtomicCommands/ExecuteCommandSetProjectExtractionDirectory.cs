// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandSetProjectExtractionDirectory : BasicCommandExecution, IAtomicCommand
{
    private readonly Project _project;

    public ExecuteCommandSetProjectExtractionDirectory(IBasicActivateItems activator, Project project) : base(activator)
    {
        _project = project;
    }

    public override string GetCommandHelp() =>
        "Change the location on disk where extracted artefacts are put when you run extraction configurations of this project";

    public override void Execute()
    {
        base.Execute();

        var dir = BasicActivator.SelectDirectory("Extraction Directory");

        if (dir == null) return;
        _project.ExtractionDirectory = dir.FullName;
        _project.SaveToDatabase();
        Publish(_project);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractionDirectoryNode, OverlayKind.Edit);
}