// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;

namespace Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;

/// <summary>
///     Reverse  of LiveMigrationQueryHelper.  Builds 'migration' query for updating STAGING to match LIVE when doing a
///     backfill data load (See
///     StagingBackfillMutilator).
/// </summary>
public class ReverseMigrationQueryHelper : MigrationQueryHelper
{
    public ReverseMigrationQueryHelper(MigrationColumnSet columnsToMigrate) : base(columnsToMigrate)
    {
    }

    public override string BuildUpdateClauseForRow(string sourceAlias, string destAlias)
    {
        return string.Join(", ", ColumnsToMigrate.FieldsToUpdate.Select(col =>
            string.Format(destAlias + ".[" + col + "] = " + sourceAlias + ".[" + col + "]", col.GetRuntimeName())));
    }

    public override string BuildInsertClause()
    {
        throw new NotImplementedException(
            "Do not attempt to insert data into staging from live, this query helper is purely for updating the destination.");
    }
}