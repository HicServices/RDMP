// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandOpenInExplorer : BasicUICommandExecution
{
    private readonly FileSystemInfo _info;

    public ExecuteCommandOpenInExplorer(IActivateItems activator, FileSystemInfo info) : base(activator)
    {
        _info = info;

        if (_info is not { Exists: true })
            SetImpossible("Path not found");
    }

    public override void Execute()
    {
        base.Execute();

        if (_info != null)
            UsefulStuff.ShowPathInWindowsExplorer(_info);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.CatalogueFolder);
}