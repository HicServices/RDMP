// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using ADOX;
using DataExportLibrary.Interfaces.Data;

namespace DataExportLibrary.Data
{
    /// <summary>
    /// Information that is not held in RDMP about an ExtractableCohort but which must be fetched at runtime from the cohort database (ExternalCohortTable)
    /// 
    /// <para>Because RDMP is designed to support a wide range of cohort/release identifier allocation systems,  it takes a very hands-off approach to cohort tables. 
    /// Things like Cohort Version, Description and even ProjectNumber are not imported into RDMP because they may be part of an existing cohort management system
    /// and thus cannot be moved (creating cached/synchronized copies would just be a further pain).  </para>
    /// </summary>
    public class ExternalCohortDefinitionData : IExternalCohortDefinitionData
    {
        public ExternalCohortDefinitionData(DbDataReader r, string tableName)
        {
            ExternalProjectNumber = Convert.ToInt32(r["projectNumber"]);
            ExternalDescription = r["description"].ToString();
            ExternalVersion = Convert.ToInt32(r["version"]);
            ExternalCohortTableName = tableName;
            ExternalCohortCreationDate = ObjectToNullableDateTime(r["dtCreated"]);
        }

        public int ExternalProjectNumber { get; set; }
        public string ExternalDescription { get; set; }
        public int ExternalVersion { get; set; }
        public string ExternalCohortTableName { get; set; }
        public DateTime? ExternalCohortCreationDate { get; set; }

        public DateTime? ObjectToNullableDateTime(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return (DateTime)o;
        }
    }
}
