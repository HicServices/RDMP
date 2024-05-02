// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CohortCommitting.Pipeline;

/// <summary>
///     All details required to create a holdout set from a cohort
/// </summary>
public sealed class CohortHoldoutLookupRequest : PipelineUseCase, ICanBeSummarised, ICohortHoldoutLookupRequest
{
    public CohortIdentificationConfiguration CIC { get; set; }
    public int Count { get; set; }
    public bool IsPercent { get; set; }

    public string Description { get; set; }

    public string WhereQuery { get; set; }

    public string Name { get; set; }

    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
    public string DateColumnName { get; set; }

    public CohortHoldoutLookupRequest(CohortIdentificationConfiguration cic, string name, int count, bool isPercent,
        string description = "", string minDate = null, string maxDate = null, string dateColumnName = null)
    {
        CIC = cic;
        Name = name;
        Count = count;
        IsPercent = isPercent;
        Description = description;
        if (DateTime.TryParseExact(minDate, "DD/MM/YYYY", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsedMinDate))
            MinDate = parsedMinDate;
        if (DateTime.TryParseExact(maxDate, "DD/MM/YYYY", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsedMaxDate))
            MaxDate = parsedMaxDate;
        DateColumnName = dateColumnName;
        AddInitializationObject(this);
    }

    public string GetSummary(bool includeName, bool includeId)
    {
        return $"Cohort Holdout: {Name}";
    }


    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        return new DataFlowPipelineContext<CohortIdentificationConfiguration>
        {
            MustHaveDestination = typeof(ICohortPipelineDestination),
            MustHaveSource = typeof(IDataFlowSource<CohortIdentificationConfiguration>)
        };
    }

    public void Check(ICheckNotifier notifier)
    {
        throw new NotImplementedException();
    }
}