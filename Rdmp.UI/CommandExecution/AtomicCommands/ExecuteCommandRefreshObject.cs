// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

/// <summary>
/// Publishes the fact that changes have been made to a <see cref="DatabaseEntity"/> which mean that other user interfaces in the
/// application may be now out of date (or no longer valid).  This will trigger the <see cref="RefreshBus"/> to call all listeners
/// </summary>
public class ExecuteCommandRefreshObject : BasicUICommandExecution
{
    private readonly DatabaseEntity _databaseEntity;

    public ExecuteCommandRefreshObject(IActivateItems activator, DatabaseEntity databaseEntity) : base(activator)
    {
        _databaseEntity = databaseEntity;

        if (_databaseEntity == null)
            SetImpossible("No DatabaseEntity was specified");

        Weight = 100.5f;
    }

    public override void Execute()
    {
        base.Execute();

        _databaseEntity.RevertToDatabaseState();
        BasicActivator.HardRefresh = true;
        Publish(_databaseEntity);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(FamFamFamIcons.arrow_refresh);
}