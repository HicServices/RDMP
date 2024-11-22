// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewEmptyCatalogue(IBasicActivateItems activator) : BasicCommandExecution(activator)
{
    public string TargetFolder { get; set; }

    public override string GetCommandHelp() =>
        @"Create a new dataset not yet linked to any underlying database columns\tables";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Problem);

    public override void Execute()
    {
        base.Execute();

        var c = new Catalogue(BasicActivator.RepositoryLocator.CatalogueRepository, $"New Catalogue {Guid.NewGuid()}");

        if (TargetFolder != null)
        {
            c.Folder = TargetFolder;
            c.SaveToDatabase();
        }


        Publish(c);
        Emphasise(c);
    }
}