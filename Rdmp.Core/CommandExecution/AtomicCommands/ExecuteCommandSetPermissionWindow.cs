// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandSetPermissionWindow : BasicCommandExecution, IAtomicCommandWithTarget
{
    private readonly CacheProgress _cacheProgress;
    private PermissionWindow _window;

    public ExecuteCommandSetPermissionWindow(IBasicActivateItems activator, CacheProgress cacheProgress) :
        base(activator)
    {
        _cacheProgress = cacheProgress;
        _window = null;

        if (!activator.CoreChildProvider.AllPermissionWindows.Value.Any())
            SetImpossible("There are no PermissionWindows created yet");
    }

    public override string GetCommandHelp() => "Restrict caching execution to the given time period";

    public override void Execute()
    {
        base.Execute();

        _window ??= SelectOne<PermissionWindow>(BasicActivator.RepositoryLocator.CatalogueRepository);

        if (_window == null)
            return;

        _cacheProgress.PermissionWindow_ID = _window.ID;
        _cacheProgress.SaveToDatabase();

        Publish(_cacheProgress);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Link);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        if (target is PermissionWindow window)
            _window = window;

        return this;
    }
}