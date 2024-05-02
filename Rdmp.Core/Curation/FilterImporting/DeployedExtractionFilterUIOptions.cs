// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Curation.FilterImporting;

/// <inheritdoc />
public class DeployedExtractionFilterUIOptions : FilterUIOptions
{
    private readonly ISqlParameter[] _globals;
    private readonly ITableInfo[] _tables;
    private readonly IColumn[] _columns;

    public DeployedExtractionFilterUIOptions(DeployedExtractionFilter deployedExtractionFilter) : base(
        deployedExtractionFilter)
    {
        var selectedDataSet = deployedExtractionFilter.GetDataset();


        var ds = selectedDataSet.ExtractableDataSet;
        var c = selectedDataSet.ExtractionConfiguration;

        _tables = ds.Catalogue.GetTableInfoList(false);
        _globals = c.GlobalExtractionFilterParameters;

        var columns = new List<IColumn>();

        columns.AddRange(c.GetAllExtractableColumnsFor(ds));
        columns.AddRange(c.Project.GetAllProjectCatalogueColumns(ExtractionCategory.ProjectSpecific));

        _columns = columns.ToArray();
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