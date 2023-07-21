// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

internal class ExecuteCommandCreateNewLoadProgress : BasicCommandExecution, IAtomicCommand
{
    private readonly LoadMetadata _loadMetadata;

    public ExecuteCommandCreateNewLoadProgress(IBasicActivateItems activator, LoadMetadata loadMetadata) :
        base(activator)
    {
        _loadMetadata = loadMetadata;
    }

    public override string GetCommandHelp()
    {
        return
            "Defines that the data load configuration has too much data to load in one go and that it must be loaded in date based batches (e.g. load 2001-01-01 to 2001-01-31)";
    }

    public override void Execute()
    {
        base.Execute();

        var lp = new LoadProgress((ICatalogueRepository)_loadMetadata.Repository, _loadMetadata);
        Publish(_loadMetadata);
        Emphasise(lp);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.LoadProgress, OverlayKind.Add);
    }
}