// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Curation.FilterImporting;

/// <inheritdoc/>
public class ExtractionFilterUIOptions : FilterUIOptions
{
    private ISqlParameter[] _globals;
    private ITableInfo[] _tables;
    private IColumn[] _columns;

    public ExtractionFilterUIOptions(ExtractionFilter masterCatalogueFilter) : base(masterCatalogueFilter)
    {
        var c = masterCatalogueFilter.ExtractionInformation.CatalogueItem.Catalogue;

        var colInfo = masterCatalogueFilter.GetColumnInfoIfExists();

        if (colInfo == null)
            throw new MissingColumnInfoException("No ColumnInfo found for filter '" + masterCatalogueFilter + "'");

        _globals = colInfo.TableInfo.GetAllParameters();
        _tables = c.GetTableInfoList(false);
        _columns = c.GetAllExtractionInformation(ExtractionCategory.Any);

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