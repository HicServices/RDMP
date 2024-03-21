// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;

/// <summary>
/// Creates StageArgs for each LoadStage based on the supplied LoadMetadata (load configuration).  This tells the DLE where each database is etc in the
/// RAW => STAGING => LIVE model rdmp uses for data loading.
/// </summary>
public class LoadArgsDictionary
{
    private readonly ILoadMetadata _loadMetadata;
    private readonly StandardDatabaseHelper _dbDeployInfo;

    public Dictionary<LoadStage, IStageArgs> LoadArgs { get; private set; }

    public LoadArgsDictionary(ILoadMetadata loadMetadata, StandardDatabaseHelper dbDeployInfo)
    {
        if (string.IsNullOrWhiteSpace(loadMetadata.LocationOfFlatFiles) && string.IsNullOrWhiteSpace(loadMetadata.LocationOfForLoadingDirectory))
            throw new Exception(
                $@"No Project Directory (LocationOfFlatFiles) has been configured on LoadMetadata {loadMetadata.Name}");

        _dbDeployInfo = dbDeployInfo;
        _loadMetadata = loadMetadata;

        LoadArgs = new Dictionary<LoadStage, IStageArgs>();
        foreach (LoadStage loadStage in Enum.GetValues(typeof(LoadStage)))
            LoadArgs.Add(loadStage, CreateLoadArgs(loadStage));
    }

    protected IStageArgs CreateLoadArgs(LoadStage loadStage)
    {
        return
            new StageArgs(loadStage,
                _dbDeployInfo[loadStage.ToLoadBubble()]
                , new LoadDirectory((string.IsNullOrWhiteSpace(_loadMetadata.LocationOfFlatFiles) ? _loadMetadata.LocationOfFlatFiles : _loadMetadata.LocationOfForLoadingDirectory).TrimEnd(new[] { '\\' })));
    }
}