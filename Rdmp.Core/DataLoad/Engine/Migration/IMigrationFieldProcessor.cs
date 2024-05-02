// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using FAnsi.Discovery;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.DataLoad.Engine.Migration;

/// <summary>
///     Handles the routing of columns in a MigrationColumnSet to either FieldsToDiff or FieldsToUpdate during a MERGE
///     query in which records are written into
///     LIVE from STAGING.  Note that this interface is also usable to describe a reverse flow of records in which records
///     in STAGING are modified depending
///     on records/fields in LIVE (See StagingBackfillMutilator).
/// </summary>
public interface IMigrationFieldProcessor
{
    /// <summary>
    ///     True if there is not expected to be any backup trigger on the table i.e. <see cref="SpecialFieldNames" /> are not
    ///     going to be there
    /// </summary>
    bool NoBackupTrigger { get; set; }

    void ValidateFields(DiscoveredColumn[] fromColumns, DiscoveredColumn[] toColumns);

    /// <summary>
    ///     Assigns the current field to either Diff and/or Update (or neither).
    /// </summary>
    /// <param name="field">the field to assign to one/none/both lists</param>
    /// <param name="fieldsToDiff">
    ///     Fields that will have their values compared for change, to decide whether to overwrite destination data with source
    ///     data.
    ///     (some fields might not matter if they are different e.g. dataLoadRunID)
    /// </param>
    /// <param name="fieldsToUpdate">
    ///     Fields that will have their values copied across to the new table (this is usually a superset of fields to diff,
    ///     and also
    ///     includes all primary keys).
    /// </param>
    void AssignFieldsForProcessing(DiscoveredColumn field, List<DiscoveredColumn> fieldsToDiff,
        List<DiscoveredColumn> fieldsToUpdate);
}