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
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.DataLoad.Engine.Migration
{
    /// <summary>
    /// Checks that LIVE has appropriate fields to support the migration of records from STAGING to LIVE and assigns fields roles such that artifact fields
    /// that are generated as part of the load (i.e. computed columns) denoted by the prefix hic_ are not treated as differences in the dataset.  This means
    /// that records in STAGING with a new hic_dataLoadRunID (all of them because each load gets a unique number) will not be identified as UPDATES to the 
    /// LIVE data table and will be ignored (assuming that there are no differences in other fields that are Diffed).
    /// </summary>
    public class StagingToLiveMigrationFieldProcessor : IMigrationFieldProcessor
    {
        private readonly Regex _updateButDoNotDiffNormal = new Regex("^hic_", RegexOptions.IgnoreCase);
        private readonly Regex _updateButDoNotDiffExtended;

        public StagingToLiveMigrationFieldProcessor(Regex updateButDoNotDiff = null)
        {
            _updateButDoNotDiffExtended = updateButDoNotDiff;
        }

        public void ValidateFields(DiscoveredColumn[] sourceFields, DiscoveredColumn[] destinationFields)
        {
            if (!destinationFields.Any(f => f.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID, StringComparison.CurrentCultureIgnoreCase)))
                throw new MissingFieldException("Destination (Live) database table is missing field:" + SpecialFieldNames.DataLoadRunID);

            if (!destinationFields.Any(f=>f.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom,StringComparison.CurrentCultureIgnoreCase)))
                throw new MissingFieldException("Destination (Live) database table is missing field:" + SpecialFieldNames.ValidFrom);
        }

        public void AssignFieldsForProcessing(DiscoveredColumn field, List<DiscoveredColumn> fieldsToDiff, List<DiscoveredColumn> fieldsToUpdate)
        {
            //it is a hic internal field but not one of the overwritten, standard ones
            if (_updateButDoNotDiffNormal.IsMatch(field.GetRuntimeName()) 
                || 
                IsSupplementalMatch(field))
            
                fieldsToUpdate.Add(field);
            else
            {
                //it is not a hic internal field
                fieldsToDiff.Add(field);
                fieldsToUpdate.Add(field);
            }
        }

        private bool IsSupplementalMatch(DiscoveredColumn field)
        {
            if(_updateButDoNotDiffExtended == null)
                return false;

            //its a supplemental ignore e.g. MessageGuid
            bool match = _updateButDoNotDiffExtended.IsMatch(field.GetRuntimeName());

            if(match && field.IsPrimaryKey)
                throw new NotSupportedException("UpdateButDoNotDiff Pattern " + _updateButDoNotDiffExtended + " matched Primary Key column '" + field.GetRuntimeName() + "' this is not permitted");

            return match;
        }
    }
}
