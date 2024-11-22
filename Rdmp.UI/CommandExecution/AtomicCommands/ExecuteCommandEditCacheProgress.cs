// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal sealed class ExecuteCommandEditCacheProgress : BasicUICommandExecution
{
    private readonly CacheProgress _cacheProgress;

    public ExecuteCommandEditCacheProgress(IActivateItems activator, CacheProgress cacheProgress) : base(activator)
    {
        _cacheProgress = cacheProgress;
    }

    public override void Execute()
    {
        base.Execute();
        Activator.Activate<CacheProgressUI, CacheProgress>(_cacheProgress);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(_cacheProgress);

    public override string GetCommandHelp() =>
        "Change which pipeline is used to fetch data, what date the cache has progressed to etc";
}