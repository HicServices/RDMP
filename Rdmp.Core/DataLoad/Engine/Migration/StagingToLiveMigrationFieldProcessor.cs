// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.DataLoad.Engine.Migration;

/// <summary>
///     Checks that LIVE has appropriate fields to support the migration of records from STAGING to LIVE and assigns fields
///     roles such that artifact fields
///     that are generated as part of the load (i.e. computed columns) denoted by the prefix hic_ are not treated as
///     differences in the dataset.  This means
///     that records in STAGING with a new hic_dataLoadRunID (all of them because each load gets a unique number) will not
///     be identified as UPDATES to the
///     LIVE data table and will be ignored (assuming that there are no differences in other fields that are Diffed).
/// </summary>
public class StagingToLiveMigrationFieldProcessor : IMigrationFieldProcessor
{
    private readonly Regex _updateButDoNotDiffExtended;
    private readonly Regex _ignore;
    private readonly ColumnInfo[] _alsoIgnore;

    /// <inheritdoc />
    public bool NoBackupTrigger { get; set; }

    public StagingToLiveMigrationFieldProcessor(Regex updateButDoNotDiff = null, Regex ignore = null,
        ColumnInfo[] alsoIgnore = null)
    {
        _updateButDoNotDiffExtended = updateButDoNotDiff;
        _ignore = ignore;
        _alsoIgnore = alsoIgnore;
    }

    public void ValidateFields(DiscoveredColumn[] sourceFields, DiscoveredColumn[] destinationFields)
    {
        if (NoBackupTrigger)
            return;

        if (!destinationFields.Any(f =>
                f.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID, StringComparison.CurrentCultureIgnoreCase)))
            throw new MissingFieldException(
                $"Destination (Live) database table is missing field:{SpecialFieldNames.DataLoadRunID}");

        if (!destinationFields.Any(f =>
                f.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom, StringComparison.CurrentCultureIgnoreCase)))
            throw new MissingFieldException(
                $"Destination (Live) database table is missing field:{SpecialFieldNames.ValidFrom}");
    }

    public void AssignFieldsForProcessing(DiscoveredColumn field, List<DiscoveredColumn> fieldsToDiff,
        List<DiscoveredColumn> fieldsToUpdate)
    {
        if (IgnoreColumnInfo(field))
            return;

        if (Ignore(field))
            return;

        //it is a hic internal field but not one of the overwritten, standard ones
        if (SpecialFieldNames.IsHicPrefixed(field)
            ||
            UpdateOnly(field))

        {
            fieldsToUpdate.Add(field);
        }
        else
        {
            //it is not a hic internal field
            fieldsToDiff.Add(field);
            fieldsToUpdate.Add(field);
        }
    }

    private bool IgnoreColumnInfo(DiscoveredColumn field)
    {
        if (_alsoIgnore == null)
            return false;

        // TODO: if a load targets 2 tables with the same name in different databases and one has a column marked ignore it could be ignored in both during load.  Note that `field` parameter is the from column not the to column

        //its a column we were told to ignore
        var match = _alsoIgnore.FirstOrDefault(c =>
            c.GetRuntimeName().Equals(field.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase) &&
            c.TableInfo.GetRuntimeName().Equals(field.Table.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase)
        );

        return match != null && field.IsPrimaryKey
            ? throw new NotSupportedException(
                $"ColumnInfo {match} is marked {nameof(ColumnInfo.IgnoreInLoads)} but is a Primary Key column this is not permitted")
            : match != null;
    }

    private bool Ignore(DiscoveredColumn field)
    {
        if (_ignore == null)
            return false;

        //its a global ignore based on regex ignore pattern?
        var match = _ignore.IsMatch(field.GetRuntimeName());

        return match && field.IsPrimaryKey
            ? throw new NotSupportedException(
                $"Ignore Pattern {_ignore} matched Primary Key column '{field.GetRuntimeName()}' this is not permitted")
            : match;
    }

    private bool UpdateOnly(DiscoveredColumn field)
    {
        if (_updateButDoNotDiffExtended == null)
            return false;

        //its a supplemental ignore e.g. MessageGuid
        var match = _updateButDoNotDiffExtended.IsMatch(field.GetRuntimeName());

        return match && field.IsPrimaryKey
            ? throw new NotSupportedException(
                $"UpdateButDoNotDiff Pattern {_updateButDoNotDiffExtended} matched Primary Key column '{field.GetRuntimeName()}' this is not permitted")
            : match;
    }
}