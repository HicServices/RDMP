// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.DataLoad.Engine.Migration;

/// <summary>
///     IMigrationFieldProcessor for StagingBackfillMutilator (See StagingBackfillMutilator).
/// </summary>
public class BackfillMigrationFieldProcessor : IMigrationFieldProcessor
{
    public bool NoBackupTrigger
    {
        get => false;
        set
        {
            if (!value)
                throw new NotSupportedException("NoBackupTrigger must be false to perform this migration");
        }
    }

    public void ValidateFields(DiscoveredColumn[] sourceFields, DiscoveredColumn[] destinationFields)
    {
        if (!sourceFields.Any(c =>
                c.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID, StringComparison.CurrentCultureIgnoreCase)))
            throw new MissingFieldException(SpecialFieldNames.DataLoadRunID);


        if (!sourceFields.Any(c =>
                c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom, StringComparison.CurrentCultureIgnoreCase)))
            throw new MissingFieldException(SpecialFieldNames.ValidFrom);
    }

    public void AssignFieldsForProcessing(DiscoveredColumn field, List<DiscoveredColumn> fieldsToDiff,
        List<DiscoveredColumn> fieldsToUpdate)
    {
        //it is a hic internal field but not one of the overwritten, standard ones
        if (SpecialFieldNames.IsHicPrefixed(field))
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
}