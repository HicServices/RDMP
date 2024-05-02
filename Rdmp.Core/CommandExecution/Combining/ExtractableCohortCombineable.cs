// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
///     <see cref="ICombineToMakeCommand" /> for an object of type <see cref="ExtractableCohort" />
/// </summary>
public class ExtractableCohortCombineable : ICombineToMakeCommand
{
    public int ExternalProjectNumber { get; set; }
    public ExtractableCohort Cohort { get; set; }

    public Exception ErrorGettingCohortData { get; private set; }

    public IExtractionConfiguration[] CompatibleExtractionConfigurations { get; set; }
    public Project[] CompatibleProjects { get; set; }

    public ExtractableCohortCombineable(ExtractableCohort extractableCohort)
    {
        Cohort = extractableCohort;

        try
        {
            ExternalProjectNumber = Cohort.GetExternalData(2).ExternalProjectNumber;
        }
        catch (Exception e)
        {
            ErrorGettingCohortData = e;
            return;
        }

        CompatibleProjects =
            extractableCohort.Repository.GetAllObjectsWhere<Project>("ProjectNumber", ExternalProjectNumber);
        CompatibleExtractionConfigurations = CompatibleProjects.SelectMany(p => p.ExtractionConfigurations).ToArray();
    }

    public string GetSqlString()
    {
        return Cohort.WhereSQL();
    }
}