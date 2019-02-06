// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// This data class reflects a single row in a cohortDefinition table (see ExternalCohortTable).  It may also reflect one that does not exist yet
    /// in which case it will have a null ID (e.g. in the case where you are trying to create a new cohort using an identifier list).
    /// </summary>
    public class CohortDefinition : ICohortDefinition
    {
        public int? ID { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public int ProjectNumber { get; set; }
        public IExternalCohortTable LocationOfCohort { get; private set; }

        /// <summary>
        /// The cohort replaced if uploading a new version
        /// </summary>
        public IExtractableCohort CohortReplacedIfAny { get; set; }

        public CohortDefinition(int? id, string description, int version, int projectNumber, IExternalCohortTable locationOfCohort)
        {
            ID = id;
            Description = description;
            Version = version;
            ProjectNumber = projectNumber;
            LocationOfCohort = locationOfCohort;

            if (string.IsNullOrWhiteSpace(description))
                throw new NullReferenceException("Description for cohort with ID " + id + " is blank this is not permitted");
        }
        
        public bool IsAcceptableAsNewCohort(out string matchDescription)
        {
                //if there is an ID
                if (ID != null)
                    if (ExtractableCohort.GetImportableCohortDefinitions((ExternalCohortTable) LocationOfCohort).Any(t => t.ID == ID))
                        //the same ID already exists
                    {
                        matchDescription = "Found a cohort in " + LocationOfCohort + " with the ID " + ID;
                        return false;
                    }
                        

                bool foundSimilar =
                    ExtractableCohort.GetImportableCohortDefinitions((ExternalCohortTable)LocationOfCohort)//see if there is one with the same name
                        .Any(t => t.Description.Equals(Description) && t.Version.Equals(Version));//and description (it might have a different ID but it is still against the rules)

            if (foundSimilar)
            {
                matchDescription = "Found an existing cohort called " + Description + " with version " + Version + " in " + LocationOfCohort;
                return false;
            }

            //did not find any conflicting cohort definitions
            matchDescription = null;
            return true;
        }

        public override string ToString()
        {
            return Description + "(Version " + Version + ", ID="+ID+")";
        }
    }
}