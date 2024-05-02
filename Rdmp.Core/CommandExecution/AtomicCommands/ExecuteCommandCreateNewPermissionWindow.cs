// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewPermissionWindow : BasicCommandExecution, IAtomicCommandWithTarget
{
    private CacheProgress _cacheProgressToSetOnIfAny;

    public ExecuteCommandCreateNewPermissionWindow(IBasicActivateItems activator) : base(activator)
    {
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Add);
    }

    public override string GetCommandHelp()
    {
        return "Creates a new time window restriction on when loads can occur";
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _cacheProgressToSetOnIfAny = target as CacheProgress;
        return this;
    }

    public override void Execute()
    {
        base.Execute();

        if (TypeText("Permission Window Name", "Enter name for the PermissionWindow e.g. 'Nightly Loads'", 1000, null,
                out var name))
        {
            var newWindow = new PermissionWindow(BasicActivator.RepositoryLocator.CatalogueRepository)
            {
                Name = name
            };
            newWindow.SaveToDatabase();

            if (_cacheProgressToSetOnIfAny != null)
                new ExecuteCommandSetPermissionWindow(BasicActivator, _cacheProgressToSetOnIfAny).SetTarget(newWindow)
                    .Execute();

            Publish(newWindow);
            Activate(newWindow);
        }
    }
}