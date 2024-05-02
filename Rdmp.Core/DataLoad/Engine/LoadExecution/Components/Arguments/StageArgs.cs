// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;

/// <summary>
///     Identifies the database target of a given DLE LoadStage (e.g. AdjustRaw would contain a DiscoveredDatabase pointed
///     at the RAW database). Also includes
///     the location of the load directory
/// </summary>
public class StageArgs : IStageArgs
{
    public DiscoveredDatabase DbInfo { get; }
    public ILoadDirectory RootDir { get; }

    //Mandatory
    public LoadStage LoadStage { get; }

    public StageArgs(LoadStage loadStage, DiscoveredDatabase database, ILoadDirectory projectDirectory)
    {
        LoadStage = loadStage;
        DbInfo = database;
        RootDir = projectDirectory;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var props = GetType().GetProperties();
        return props.ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(this, null));
    }
}