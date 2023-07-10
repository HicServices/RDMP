// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.LoadExecutionUIs;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandExecuteCacheProgress:BasicUICommandExecution,IAtomicCommandWithTarget
{
    private CacheProgress _cp;

    [UseWithObjectConstructor]
    public ExecuteCommandExecuteCacheProgress(IActivateItems activator, CacheProgress cp) : base(activator)
    {
        _cp = cp;
    }

    public ExecuteCommandExecuteCacheProgress(IActivateItems activator)
        : base(activator)
    {

    }

    public override string GetCommandHelp()
    {
        return "Runs the caching activity.  This usually involves long term incremental fetching and storing data ready for load";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Execute);
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _cp = (CacheProgress) target;
        return this;
    }

    public override void Execute()
    {
        base.Execute();

        _cp ??= SelectOne<CacheProgress>(Activator.RepositoryLocator.CatalogueRepository);
            
        if(_cp == null)
            return;
            
        Activator.Activate<ExecuteCacheProgressUI, CacheProgress>(_cp);
    }
}