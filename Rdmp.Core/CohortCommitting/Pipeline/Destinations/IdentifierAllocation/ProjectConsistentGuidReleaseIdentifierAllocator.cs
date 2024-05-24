// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.DataExport;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;

/// <summary>
/// Allocates a Guid for each private identifier supplied.  This is similar to <see cref="GuidReleaseIdentifierAllocator"/> except that it will preserve previous
/// allocations within the <see cref="Project"/>.  For example if you commit a cohort 'Cases' with private id '123' to project '10' then might get a guid 'abc...',
/// if you then submit verison 2 of the cohort you will get the same guid ('abc...') for persion '123'.
/// 
/// <para>Guids are always different between <see cref="Project"/> for example person 'abc' in project '10' will have a different Guid release identifier than if he
/// was committed to project '11' and it would be impossible to link the two release identifiers</para>
/// 
/// <para>Projects are differentiated by <see cref="Project.ProjectNumber"/> since this is what is stored in your cohort database</para>
/// </summary>
public class ProjectConsistentGuidReleaseIdentifierAllocator : IAllocateReleaseIdentifiers
{
    private int _projectNumber;
    private ICohortCreationRequest _request;
    private Dictionary<object, object> _releaseMap;

    /// <summary>
    /// Returns the existing anonymous release identifier for the <paramref name="privateIdentifier"/> if it has ever been
    /// uploaded to the given <see cref="Project"/> before otherwise returns a new unique guid as a string.
    /// </summary>
    /// <param name="privateIdentifier"></param>
    /// <returns></returns>
    public object AllocateReleaseIdentifier(object privateIdentifier)
    {
        //figure out all the historical release ids for private ids in the Project
        _releaseMap ??= GetReleaseMap();

        //if we have a historical release Id use it
        if (_releaseMap.TryGetValue(privateIdentifier, out var identifier))
            return identifier;

        //otherwise allocate a new guid and let's record it just for prosperity
        var toReturn = Guid.NewGuid().ToString();
        _releaseMap.Add(privateIdentifier, toReturn);

        return toReturn;
    }

    /// <summary>
    /// Figures out all the previously allocated release identifiers for private identifiers for cohorts assigned to the projectNumber
    /// </summary>
    /// <returns></returns>
    private Dictionary<object, object> GetReleaseMap()
    {
        var toReturn = new Dictionary<object, object>();

        var cohortDatabase = _request.NewCohortDefinition.LocationOfCohort.Discover();

        var ect = _request.NewCohortDefinition.LocationOfCohort;

        var syntax = cohortDatabase.Server.GetQuerySyntaxHelper();

        using var con = cohortDatabase.Server.GetConnection();
        var sql =
            $"SELECT DISTINCT {ect.PrivateIdentifierField},{ect.ReleaseIdentifierField} FROM {ect.TableName} JOIN {ect.DefinitionTableName} ON {ect.DefinitionTableForeignKeyField}={ect.DefinitionTableName}.id WHERE {ect.DefinitionTableName}.{syntax.EnsureWrapped("projectNumber")}={_projectNumber}";

        var syntaxHelper = ect.GetQuerySyntaxHelper();

        var priv = syntaxHelper.GetRuntimeName(ect.PrivateIdentifierField);
        var rel = syntaxHelper.GetRuntimeName(ect.ReleaseIdentifierField);

        con.Open();

        using var r = cohortDatabase.Server.GetCommand(sql, con).ExecuteReader();
        while (r.Read())
        {
            if (toReturn.TryGetValue(r[priv], out var oldValue))
                continue;
            // this causes issues with additiona l extractions
            //    throw new Exception(
            //        $"Private identifier '{r[priv]}' has more than 1 historical release identifier ({string.Join(",", oldValue, r[rel])}");
            toReturn.Add(r[priv], r[rel]);
        }

        return toReturn;
    }

    /// <inheritdoc/>
    public void Initialize(ICohortCreationRequest request)
    {
        if (request.Project != null)
        {
            if (!request.Project.ProjectNumber.HasValue)
                throw new ProjectNumberException($"Project {request.Project} must have a ProjectNumber");
        }
        else
        {
            if ((request.NewCohortDefinition?.ProjectNumber ?? 0) == 0)
                throw new ProjectNumberException(
                    "No Project was specified and NewCohortDefinition had no explicit project number");
        }

        _request = request;
        _projectNumber = request.Project?.ProjectNumber.Value ?? request.NewCohortDefinition?.ProjectNumber ?? 0;
    }
}