// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.CohortDescribing;

/// <summary>
/// Creates ExtractableCohortDescription objects for each of your cohorts, this involves issuing an async request to the cohort endpoints to calculate things like
/// Count, CountDistinct etc.  The ExtractableCohortDescription objects returned from Create will not be populated with values until the async finishes and will have only
/// placeholder values like "Loading..." etc
/// </summary>
public class CohortDescriptionFactory
{
    private ExternalCohortTable[] _sources;
    private ExtractableCohort[] _cohorts;

    /// <summary>
    /// Creates ExtractableCohortDescription objects for each of your cohorts, this involves issuing an async request to the cohort endpoints to calculate things like
    /// Count, CountDistinct etc.  The ExtractableCohortDescription objects returned from Create will not be populated with values until the async finishes and will have only
    /// placeholder values like "Loading..." etc
    /// </summary>
    /// <param name="repository">The DataExportRepository containing all your cohort references (ExtractableCohorts)</param>
    public CohortDescriptionFactory(IDataExportRepository repository)
    {
        _sources = repository.GetAllObjects<ExternalCohortTable>();
        _cohorts = repository.GetAllObjects<ExtractableCohort>();
    }

    /// <summary>
    /// Starts 1 async fetch request for each cohort endpoint e.g. NormalCohorts ExternalCohortTable database contains 100 cohorts while FreakyCohorts ExternalCohortTable database
    /// has another 30.
    /// 
    /// <para>These async requests are managed by the CohortDescriptionDataTableAsyncFetch object which has a callback for completion.  Each ExtractableCohortDescription subscribes to
    /// the callback to self populate</para>
    /// </summary>
    /// <returns></returns>
    public Dictionary<CohortDescriptionDataTableAsyncFetch, ExtractableCohortDescription[]> Create()
    {
        var toReturn = new Dictionary<CohortDescriptionDataTableAsyncFetch, ExtractableCohortDescription[]>();

        foreach (var source in _sources)
        {
            //setup the async data retrieval which can take a long time if there are a lot of cohorts or millions of identifiers
            var asyncFetch = new CohortDescriptionDataTableAsyncFetch(source);
            var cohorts = _cohorts.Where(c => c.ExternalCohortTable_ID == source.ID)
                .Select(c => new ExtractableCohortDescription(c, asyncFetch)).ToArray();

            asyncFetch.Begin();

            toReturn.Add(asyncFetch, cohorts);
        }

        return toReturn;
    }
}