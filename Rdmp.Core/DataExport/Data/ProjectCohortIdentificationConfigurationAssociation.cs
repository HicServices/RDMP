// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="IProjectCohortIdentificationConfigurationAssociation" />
public class ProjectCohortIdentificationConfigurationAssociation : DatabaseEntity,
    IProjectCohortIdentificationConfigurationAssociation, IInjectKnown<CohortIdentificationConfiguration>
{
    #region Database Properties

    private int _project_ID;
    private int _cohortIdentificationConfiguration_ID;

    #endregion

    /// <inheritdoc />
    public int Project_ID
    {
        get => _project_ID;
        set => SetField(ref _project_ID, value);
    }

    /// <summary>
    ///     The <see cref="Curation.Data.Cohort.CohortIdentificationConfiguration" /> which is associated with the given
    ///     <see cref="Project_ID" />.
    /// </summary>
    public int CohortIdentificationConfiguration_ID
    {
        get => _cohortIdentificationConfiguration_ID;
        set => SetField(ref _cohortIdentificationConfiguration_ID, value);
    }


    #region Relationships

    /// <inheritdoc cref="Project_ID" />
    [NoMappingToDatabase]
    public IProject Project => Repository.GetObjectByID<Project>(Project_ID);

    private Lazy<CohortIdentificationConfiguration> _knownCic;

    /// <inheritdoc cref="CohortIdentificationConfiguration_ID" />
    [NoMappingToDatabase]
    public CohortIdentificationConfiguration CohortIdentificationConfiguration =>
        //handles the object having been deleted and somehow that deletion is missed
        _knownCic.Value;

    #endregion

    public ProjectCohortIdentificationConfigurationAssociation()
    {
        ClearAllInjections();
    }

    /// <summary>
    ///     Declares in the <paramref name="repository" /> database that the given <paramref name="cic" /> cohort query is
    ///     associated with the supplied <paramref name="project" />.
    ///     This is usually done after using the query to build an <see cref="IExtractableCohort" /> (But it can be done
    ///     manually by the user too).
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="project"></param>
    /// <param name="cic"></param>
    public ProjectCohortIdentificationConfigurationAssociation(IDataExportRepository repository, Project project,
        CohortIdentificationConfiguration cic)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Project_ID", project.ID },
            { "CohortIdentificationConfiguration_ID", cic.ID }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");

        ClearAllInjections();
    }

    internal ProjectCohortIdentificationConfigurationAssociation(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Project_ID = Convert.ToInt32(r["Project_ID"]);
        CohortIdentificationConfiguration_ID = Convert.ToInt32(r["CohortIdentificationConfiguration_ID"]);

        ClearAllInjections();
    }


    public void InjectKnown(CohortIdentificationConfiguration instance)
    {
        _knownCic = new Lazy<CohortIdentificationConfiguration>(instance);
    }

    public void ClearAllInjections()
    {
        _knownCic = new Lazy<CohortIdentificationConfiguration>(FetchCohortIdentificationConfiguration);
    }

    private CohortIdentificationConfiguration FetchCohortIdentificationConfiguration()
    {
        return DataExportRepository.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ID",
            CohortIdentificationConfiguration_ID).SingleOrDefault();
    }


    /// <summary>
    ///     Returns the associated <see cref="CohortIdentificationConfiguration_ID" /> Name
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var assoc = CohortIdentificationConfiguration;
        return assoc == null ? "Orphan Association" : assoc.Name;
    }

    public bool ShouldBeReadOnly(out string reason)
    {
        reason = null;
        return CohortIdentificationConfiguration?.ShouldBeReadOnly(out reason) ?? false;
    }

    /// <inheritdoc />
    public string GetDeleteMessage()
    {
        return "remove CohortIdentificationConfiguration from the Project";
    }

    /// <inheritdoc />
    public string GetDeleteVerb()
    {
        return "Remove";
    }

    /// <summary>
    ///     Returns the <see cref="CohortIdentificationConfiguration_ID" />
    /// </summary>
    /// <returns></returns>
    public object MasqueradingAs()
    {
        return CohortIdentificationConfiguration;
    }
}