// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.Constraints;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Engine.Migration;

/// <summary>
/// Converts a list of TableInfos into MigrationColumnSets to achieve migration of records from STAGING to LIVE during a DLE execution.
/// </summary>
public class MigrationConfiguration
{
    private readonly DiscoveredDatabase _fromDatabaseInfo;
    private readonly LoadBubble _fromBubble;
    private readonly LoadBubble _toBubble;
    private readonly INameDatabasesAndTablesDuringLoads _namer;

    public MigrationConfiguration(DiscoveredDatabase fromDatabaseInfo, LoadBubble fromBubble, LoadBubble toBubble,
        INameDatabasesAndTablesDuringLoads namer)
    {
        _fromDatabaseInfo = fromDatabaseInfo;
        _fromBubble = fromBubble;
        _toBubble = toBubble;
        _namer = namer;
    }

    public IList<MigrationColumnSet> CreateMigrationColumnSetFromTableInfos(List<ITableInfo> tableInfos,
        List<ITableInfo> lookupTableInfos, IMigrationFieldProcessor migrationFieldProcessor)
    {
        //treat null values as empty
        tableInfos ??= new List<ITableInfo>();
        lookupTableInfos ??= new List<ITableInfo>();

        var columnSet = new List<MigrationColumnSet>();

        foreach (var tableInfo in tableInfos.Union(lookupTableInfos))
        {
            var fromTableName = tableInfo.GetRuntimeName(_fromBubble, _namer);
            var toTableName = tableInfo.GetRuntimeName(_toBubble, _namer);

            var fromTable =
                _fromDatabaseInfo
                    .ExpectTable(
                        fromTableName); //Staging doesn't have schema e.g. even if live schema is not dbo STAGING will be

            var toTable = DataAccessPortal
                .ExpectDatabase(tableInfo, DataAccessContext.DataLoad)
                .ExpectTable(toTableName, tableInfo.Schema);

            if (!fromTable.Exists())
                if (lookupTableInfos
                    .Contains(tableInfo)) //its a lookup table which doesn't exist in from (Staging) - nevermind
                    continue;
                else
                    throw new Exception(
                        $"Table {fromTableName} was not found on on server {_fromDatabaseInfo.Server} (Database {_fromDatabaseInfo})"); //its not a lookup table if it isn't in STAGING that's a problem!

            columnSet.Add(new MigrationColumnSet(fromTable, toTable, migrationFieldProcessor));
        }

        var sorter = new RelationshipTopologicalSort(columnSet.Select(c => c.DestinationTable));
        columnSet = columnSet
            .OrderBy(s => ((ReadOnlyCollection<DiscoveredTable>)sorter.Order).IndexOf(s.DestinationTable)).ToList();

        return columnSet;
    }
}