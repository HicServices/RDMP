// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CohortCommitting.Pipeline;

/// <summary>
/// All metadata details nessesary to create a cohort including which project it goes into, its name, version etc.  There are no identifiers for the cohort.
/// Also functions as the use case for cohort creation (to which it passes itself as an input object).
/// </summary>
public sealed class CohortHoldoutLookupRequest : PipelineUseCase, ICanBeSummarised, ICohortHoldoutLookupRequest
{
    public CohortIdentificationConfiguration CIC { get; set; }
    public int Count { get; set; }
    public bool IsPercent { get; set; }

    public string DescriptionForAuditLog { get; set; }

    public string Name { get; set; }
    public CohortHoldoutLookupRequest(CohortIdentificationConfiguration cic, string name, int count, bool isPercent, string descriptionForAuditLog)
    {
        CIC = cic;
        Name = name;
        Count = count;
        IsPercent = isPercent;
        DescriptionForAuditLog = descriptionForAuditLog;

        //AddInitializationObject(Project);
        AddInitializationObject(this);
    }
        public string GetSummary(bool includeName, bool includeId)
    {
        throw new NotImplementedException();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        throw new NotImplementedException();
    }
}