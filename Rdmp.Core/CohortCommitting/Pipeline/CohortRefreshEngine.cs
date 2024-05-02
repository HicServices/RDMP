// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CohortCommitting.Pipeline;

/// <summary>
///     Executes an ExtractionConfiguration's CohortRefreshPipeline which should result in the
///     CohortIdentificationConfiguration associated with the
///     ExtractionConfiguration (if any) being recalculated and a new updated set of patient identifiers committed as the
///     next version number in the cohort
///     database for that ExtractionConfiguration.
///     <para>
///         Use this class if you want to re-run a the patient identifiers of an ExtractionConfiguration without changing
///         the cohort identification configuration
///         query (say 1 month later you want to generate an extract with the new patients fitting cohort criteria).
///     </para>
/// </summary>
public class CohortRefreshEngine
{
    private readonly IDataLoadEventListener _listener;
    private readonly ExtractionConfiguration _configuration;

    public CohortCreationRequest Request { get; }

    public CohortRefreshEngine(IDataLoadEventListener listener, ExtractionConfiguration configuration)
    {
        _listener = listener;
        _configuration = configuration;
        Request = new CohortCreationRequest(configuration);
    }

    public void Execute()
    {
        var engine = Request.GetEngine(_configuration.CohortRefreshPipeline, _listener);

        //if the refresh pipeline is a cic source
        if (engine.SourceObject is CohortIdentificationConfigurationSource cicSource)
            //a cohort identification configuration is a complex query possibly with many cached subqueries, if we are refreshing the cic we will want to clear (and recache) identifiers
            //from the live tables
            cicSource.ClearCohortIdentificationConfigurationCacheBeforeRunning = true;

        engine.ExecutePipeline(new GracefulCancellationToken());

        var newCohort = Request.CohortCreatedIfAny;
        if (newCohort != null)
        {
            _configuration.Cohort_ID = newCohort.ID;
            _configuration.SaveToDatabase();
        }
    }
}