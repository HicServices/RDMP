// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     Builds a query to fetch data from a <see cref="TableInfo" />
/// </summary>
public class ViewTableInfoExtractUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    public ViewType ViewType { get; private set; }

    /// <summary>
    ///     for persistence, do not use
    /// </summary>
    public ViewTableInfoExtractUICollection()
    {
    }

    public ViewTableInfoExtractUICollection(ITableInfo t, ViewType viewType, IFilter filter = null)
        : this()
    {
        DatabaseObjects.Add(t);

        if (filter != null)
            DatabaseObjects.Add(filter);
        ViewType = viewType;
    }

    public override string SaveExtraText()
    {
        return PersistStringHelper.SaveDictionaryToString(new Dictionary<string, string>
            { { "ViewType", ViewType.ToString() } });
    }

    public override void LoadExtraText(string s)
    {
        var value = PersistStringHelper.GetValueIfExistsFromPersistString("ViewType", s);
        ViewType = (ViewType)Enum.Parse(typeof(ViewType), value);
    }

    public object GetDataObject()
    {
        return DatabaseObjects.Single(o => o is ColumnInfo or Curation.Data.TableInfo);
    }

    public IFilter GetFilterIfAny()
    {
        return (IFilter)DatabaseObjects.SingleOrDefault(o => o is IFilter);
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        if (GetFilterIfAny() is ConcreteFilter filter)
            yield return filter;
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        return TableInfo;
    }

    public string GetSql()
    {
        var qb = new QueryBuilder(null, null);

        if (ViewType == ViewType.TOP_100)
            qb.TopX = 100;

        var memoryRepository = new MemoryCatalogueRepository();

        qb.AddColumnRange(TableInfo.ColumnInfos.Select(c => new ColumnInfoToIColumn(memoryRepository, c)).ToArray());

        var filter = GetFilterIfAny();
        if (filter != null)
            qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(memoryRepository, null, new[] { filter },
                FilterContainerOperation.AND);

        var sql = qb.SQL;

        return ViewType == ViewType.Aggregate
            ? throw new NotSupportedException("ViewType.Aggregate can only be applied to ColumnInfos not TableInfos")
            : sql;
    }

    public string GetTabName()
    {
        return $"{TableInfo}({ViewType})";
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
        autoComplete.Add(TableInfo);
    }

    public TableInfo TableInfo => DatabaseObjects.OfType<TableInfo>().SingleOrDefault();

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        var t = TableInfo;
        return t?.GetQuerySyntaxHelper();
    }
}