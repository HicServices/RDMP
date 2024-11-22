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

public class ExecuteCommandCreateNewCacheProgress : BasicCommandExecution
{
    private readonly LoadProgress _loadProgress;

    public ExecuteCommandCreateNewCacheProgress(IBasicActivateItems activator, LoadProgress loadProgress) :
        base(activator)
    {
        _loadProgress = loadProgress;

        if (_loadProgress.CacheProgress != null)
            SetImpossible("LoadProgress already has a CacheProgress associated with it");
    }

    public override string GetCommandHelp() =>
        "Defines that the load requires data that is intensive/expensive to fetch and that this fetching and storing to disk should happen independently of the loading";

    public override void Execute()
    {
        base.Execute();

        // If the LoadProgress doesn't have a corresponding CacheProgress, create it
        var cp = new CacheProgress(BasicActivator.RepositoryLocator.CatalogueRepository, _loadProgress);

        Publish(_loadProgress);
        Emphasise(cp);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Add);
}