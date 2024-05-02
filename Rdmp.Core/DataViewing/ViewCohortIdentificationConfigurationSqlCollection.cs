// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

internal class ViewCohortIdentificationConfigurationSqlCollection : PersistableObjectCollection,
    IViewSQLAndResultsCollection
{
    public bool UseQueryCache { get; set; }

    public ViewCohortIdentificationConfigurationSqlCollection()
    {
    }

    public ViewCohortIdentificationConfigurationSqlCollection(CohortIdentificationConfiguration config) : this()
    {
        DatabaseObjects.Add(config);
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        if (UseQueryCache)
        {
            var cache = GetCacheServer();
            if (cache != null)
                yield return cache;
        }
    }

    private ExternalDatabaseServer GetCacheServer()
    {
        return CohortIdentificationConfiguration is { QueryCachingServer_ID: not null }
            ? CohortIdentificationConfiguration.QueryCachingServer
            : null;
    }


    public IDataAccessPoint GetDataAccessPoint()
    {
        var cache = GetCacheServer();

        if (UseQueryCache && cache != null)
            return cache;

        var builder = new CohortQueryBuilder(CohortIdentificationConfiguration, null);
        builder.RegenerateSQL();
        return new SelfCertifyingDataAccessPoint(builder.Results.TargetServer);
    }

    public string GetSql()
    {
        var builder = new CohortQueryBuilder(CohortIdentificationConfiguration, null);

        if (!UseQueryCache && CohortIdentificationConfiguration.QueryCachingServer_ID.HasValue)
            builder.CacheServer = null;


        return builder.SQL;
    }

    public string GetTabName()
    {
        return $"View {CohortIdentificationConfiguration}";
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
    }

    private CohortIdentificationConfiguration CohortIdentificationConfiguration =>
        DatabaseObjects.OfType<CohortIdentificationConfiguration>().SingleOrDefault();

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return GetDataAccessPoint()?.GetQuerySyntaxHelper();
    }
}