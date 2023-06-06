﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandViewCommits : BasicUICommandExecution
{
    private readonly IMapsDirectlyToDatabaseTable _o;

    public ExecuteCommandViewCommits(IActivateItems activator, IMapsDirectlyToDatabaseTable o) : base(activator)
    {
        _o = o;
        OverrideCommandName = "View History";

        if (
            !activator.RepositoryLocator.CatalogueRepository
                .GetAllObjectsWhere<Memento>(nameof(Memento.ReferencedObjectID), o.ID)
                .Any(m => m.IsReferenceTo(o)))
        {
            SetImpossible("No commits have been made yet");
        }
    }
    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Commit);
    }
    public override void Execute()
    {
        base.Execute();

        ((IActivateItems)BasicActivator).ShowWindow(new CommitsUI(Activator, _o));
    }
}