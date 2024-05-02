// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.DataExtraction.UserPicks;

/// <summary>
///     Identifies a TableInfo that acts as a Lookup for a given dataset which is being extracted.  Lookup tables can be
///     extracted along with the extracted data
///     set (See ExtractableDatasetBundle).
/// </summary>
public class BundledLookupTable : IBundledLookupTable
{
    public ITableInfo TableInfo { get; set; }

    public BundledLookupTable(ITableInfo tableInfo)
    {
        if (!tableInfo.IsLookupTable())
            throw new Exception($"TableInfo {tableInfo} is not a lookup table");

        TableInfo = tableInfo;
    }

    public override string ToString()
    {
        return TableInfo.ToString();
    }

    /// <summary>
    ///     Reads lookup data from the <see cref="TableInfo" /> using <see cref="DataAccessContext.DataExport" />
    /// </summary>
    /// <returns></returns>
    public DataTable GetDataTable()
    {
        var tbl = TableInfo.Discover(DataAccessContext.DataExport);
        var server = tbl.Database.Server;

        var dt = new DataTable();
        dt.BeginLoadData();

        using var con = server.GetConnection();
        con.Open();
        using var da = server.GetDataAdapter(
            server.GetCommand(GetDataTableFetchSql(), con));
        da.Fill(dt);
        dt.EndLoadData();

        return dt;
    }

    public string GetDataTableFetchSql()
    {
        var catas = TableInfo.GetAllRelatedCatalogues().Where(IsLookupOnlyCatalogue).ToArray();

        if (catas.Length == 1)
        {
            // if there is a Catalogue associated with this TableInfo use its extraction instead
            var cata = catas[0];

            // Extract core columns only (and definitely not extraction identifiers)
            var eis = cata.GetAllExtractionInformation(ExtractionCategory.Core)
                .Where(static e => !e.IsExtractionIdentifier)
                .ToArray();

            if (eis.Length > 0)
            {
                var qb = new QueryBuilder(null, null, new[] { TableInfo });
                qb.AddColumnRange(eis);
                return qb.SQL;
            }

            throw new QueryBuildingException(
                $"Lookup table '{TableInfo}' has a Catalogue defined '{cata}' but it has no Core extractable columns");
        }

        return $"select * from {TableInfo.GetFullyQualifiedName()}";
    }

    /// <summary>
    ///     We only want Catalogues where all <see cref="CatalogueItem" />
    ///     are us (i.e. we don't want to pick up Catalogues where we are
    ///     Description column slotted in amongst the other columns).
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private bool IsLookupOnlyCatalogue(Catalogue arg)
    {
        var tables = arg.GetTableInfoList(true);
        return tables.Length == 1 && tables[0].ID == TableInfo.ID;
    }
}