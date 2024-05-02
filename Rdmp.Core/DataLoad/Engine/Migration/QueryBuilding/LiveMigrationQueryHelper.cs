// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;

/// <summary>
///     See MigrationQueryHelper
/// </summary>
public class LiveMigrationQueryHelper : MigrationQueryHelper
{
    private readonly int _dataLoadRunID;

    public LiveMigrationQueryHelper(MigrationColumnSet columnsToMigrate, int dataLoadRunID) : base(columnsToMigrate)
    {
        _dataLoadRunID = dataLoadRunID;
    }

    public override string BuildUpdateClauseForRow(string sourceAlias, string destAlias)
    {
        var parts = ColumnsToMigrate.FieldsToUpdate.Select(name => $"{destAlias}.[{name}] = {sourceAlias}.[{name}]")
            .ToList();
        parts.Add($"{destAlias}.{SpecialFieldNames.DataLoadRunID} = {_dataLoadRunID}");

        return string.Join(", ", parts);
    }

    public override string BuildInsertClause()
    {
        throw new NotImplementedException();
    }

    public static List<KeyValuePair<string, string>> GetListOfInsertColumnFields(MigrationColumnSet columnsToMigrate,
        int dataLoadRunID)
    {
        var inserts = new List<KeyValuePair<string, string>>();

        columnsToMigrate.FieldsToUpdate.ToList().ForEach(column =>
            inserts.Add(new KeyValuePair<string, string>($"[{column}]", $"source.[{column}]")));

        inserts.Add(new KeyValuePair<string, string>(SpecialFieldNames.DataLoadRunID, dataLoadRunID.ToString()));

        return inserts;
    }
}