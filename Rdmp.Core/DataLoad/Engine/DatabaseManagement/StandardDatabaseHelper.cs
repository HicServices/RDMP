// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.EntityNaming;

namespace Rdmp.Core.DataLoad.Engine.DatabaseManagement;

/// <summary>
///     Stores the location of all the databases (RAW, STAGING, LIVE) available during a Data Load (See LoadMetadata).
/// </summary>
public class StandardDatabaseHelper
{
    public INameDatabasesAndTablesDuringLoads DatabaseNamer { get; set; }

    public Dictionary<LoadBubble, DiscoveredDatabase> DatabaseInfoList = new();

    //Constructor
    internal StandardDatabaseHelper(DiscoveredDatabase liveDatabase, INameDatabasesAndTablesDuringLoads namer,
        DiscoveredServer rawServer)
    {
        DatabaseNamer = namer;


        foreach (var stage in new[] { LoadBubble.Raw, LoadBubble.Staging, LoadBubble.Live })
        {
            var stageName = DatabaseNamer.GetDatabaseName(liveDatabase.GetRuntimeName(), stage);
            DatabaseInfoList.Add(stage,
                stage == LoadBubble.Raw
                    ? rawServer.ExpectDatabase(stageName)
                    : liveDatabase.Server.ExpectDatabase(stageName));
        }
    }


    // Indexer declaration.
    // If index is out of range, the temps array will throw the exception.
    public DiscoveredDatabase this[LoadBubble index] => DatabaseInfoList[index];
}