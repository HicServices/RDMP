// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.Options;

namespace Rdmp.Core.Curation.FilterImporting;

/// <inheritdoc />
public class AggregateFilterUIOptions : FilterUIOptions
{
    private readonly ISqlParameter[] _globals;
    private readonly ITableInfo[] _tables;
    private readonly IColumn[] _columns;

    public AggregateFilterUIOptions(AggregateFilter aggregateFilter) : base(aggregateFilter)
    {
        var aggregateConfiguration = aggregateFilter.GetAggregate() ?? throw new Exception(
            $"AggregateFilter '{aggregateFilter}' (ID={aggregateFilter.ID}) does not belong to any AggregateConfiguration, is it somehow an orphan?");

        //it part of an AggregateConfiguration so get the same factory that is used by AggregateEditorUI to tell us about the globals and the columns
        var options = AggregateBuilderOptionsFactory.Create(aggregateConfiguration);
        _globals = options.GetAllParameters(aggregateConfiguration);

        //get all the tables
        _tables = aggregateConfiguration.Catalogue.GetTableInfoList(true);

        //but also add the ExtractionInformations and AggregateDimensions - in the case of PatientIndex table join usages (duplicates are ignored by _autoCompleteProvider)
        _columns = options.GetAvailableWHEREColumns(aggregateConfiguration);
    }

    public override ITableInfo[] GetTableInfos()
    {
        return _tables;
    }

    public override ISqlParameter[] GetGlobalParametersInFilterScope()
    {
        return _globals;
    }

    public override IColumn[] GetIColumnsInFilterScope()
    {
        return _columns;
    }
}