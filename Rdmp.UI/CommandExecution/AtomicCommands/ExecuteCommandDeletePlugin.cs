// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandDeletePlugin : BasicUICommandExecution
{
    private readonly LoadModuleAssembly _assembly;

    public ExecuteCommandDeletePlugin(IActivateItems activator, LoadModuleAssembly assembly) : base(activator)
    {
        _assembly = assembly;
      }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Plugin, OverlayKind.Delete);

    public override void Execute()
    {
        base.Execute();
        if (!YesNo($"Are you sure you want to delete {_assembly}?", "Delete Plugin")) return;

        try
        {
            _assembly.Delete();
            Show("Changes will take effect on restart");
        }
        catch (SystemException ex)
        {
            Show($"Could not delete the {_assembly} plugin.", ex);
        }
    }
}