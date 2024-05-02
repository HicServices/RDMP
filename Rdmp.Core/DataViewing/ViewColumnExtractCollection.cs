// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     Builds a query to fetch data in a <see cref="ColumnInfo" /> (Based on the <see cref="ViewType" />)
/// </summary>
public class ViewColumnExtractCollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    public ViewType ViewType { get; private set; }

    /// <summary>
    ///     The SELECT column (can be null if this instance was constructed using a <see cref="ColumnInfo" />)
    /// </summary>
    public ExtractionInformation ExtractionInformation =>
        DatabaseObjects.OfType<ExtractionInformation>().SingleOrDefault();

    /// <summary>
    ///     The SELECT column (can be null if this instance was constructed using a <see cref="ExtractionInformation" />)
    /// </summary>
    public ColumnInfo ColumnInfo => DatabaseObjects.OfType<ColumnInfo>().SingleOrDefault();


    #region Constructors

    /// <summary>
    ///     for persistence, do not use
    /// </summary>
    public ViewColumnExtractCollection()
    {
    }

    public ViewColumnExtractCollection(ColumnInfo c, ViewType viewType, IFilter filter = null) : this()
    {
        DatabaseObjects.Add(c);
        if (filter != null)
            DatabaseObjects.Add(filter);
        ViewType = viewType;
    }

    public ViewColumnExtractCollection(ColumnInfo c, ViewType viewType, IContainer container) : this()
    {
        DatabaseObjects.Add(c);
        if (container != null)
            DatabaseObjects.Add(container);
        ViewType = viewType;
    }

    public ViewColumnExtractCollection(ExtractionInformation ei, ViewType viewType, IFilter filter = null) : this()
    {
        DatabaseObjects.Add(ei);
        if (filter != null)
            DatabaseObjects.Add(filter);
        ViewType = viewType;
    }

    public ViewColumnExtractCollection(ExtractionInformation ei, ViewType viewType, IContainer container) : this()
    {
        DatabaseObjects.Add(ei);
        if (container != null)
            DatabaseObjects.Add(container);
        ViewType = viewType;
    }

    #endregion

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

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        if (GetFilterIfAny() is ConcreteFilter f)
            yield return f;

        if (GetContainerIfAny() is ConcreteContainer c)
            yield return c;

        yield return GetTableInfo() as TableInfo;
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        return GetTableInfo();
    }

    private ITableInfo GetTableInfo()
    {
        return ExtractionInformation != null
            ? ExtractionInformation.ColumnInfo?.TableInfo
            : (ITableInfo)ColumnInfo?.TableInfo;
    }

    public string GetSql()
    {
        var qb = new QueryBuilder(null, null, new[] { GetTableInfo() });

        if (ViewType == ViewType.TOP_100)
            qb.TopX = 100;

        if (ViewType == ViewType.Distribution)
            AddDistributionColumns(qb);
        else
            qb.AddColumn(GetIColumn());

        var filter = GetFilterIfAny();
        var container = GetContainerIfAny();

        if (filter != null && container != null)
            throw new Exception("Cannot generate SQL with both filter and container");

        if (filter != null && !string.IsNullOrWhiteSpace(filter.WhereSQL))
            qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(new MemoryCatalogueRepository(), null,
                new[] { filter }, FilterContainerOperation.AND);
        else if (container != null) qb.RootFilterContainer = container;

        if (ViewType == ViewType.Aggregate)
            qb.AddCustomLine("count(*) as Count,", QueryComponent.QueryTimeColumn);

        var sql = qb.SQL;

        if (ViewType == ViewType.Aggregate)
            sql += $"{Environment.NewLine} GROUP BY {GetColumnSelectSql()}";

        if (ViewType == ViewType.Aggregate)
            sql += $"{Environment.NewLine} ORDER BY count(*) DESC";

        return sql;
    }

    private IColumn GetIColumn()
    {
        if (ExtractionInformation != null) return ExtractionInformation;
        return ColumnInfo != null ? new ColumnInfoToIColumn(new MemoryRepository(), ColumnInfo) : (IColumn)null;
    }

    private void AddDistributionColumns(QueryBuilder qb)
    {
        var repo = new MemoryRepository();
        qb.AddColumn(new SpontaneouslyInventedColumn(repo, "CountTotal", "count(1)"));
        qb.AddColumn(new SpontaneouslyInventedColumn(repo, "CountNull",
            $"SUM(CASE WHEN {GetColumnSelectSql()} IS NULL THEN 1 ELSE 0  END)"));
        qb.AddColumn(new SpontaneouslyInventedColumn(repo, "CountZero",
            $"SUM(CASE WHEN {GetColumnSelectSql()} = 0 THEN 1  ELSE 0 END)"));

        qb.AddColumn(new SpontaneouslyInventedColumn(repo, "Max", $"max({GetColumnSelectSql()})"));
        qb.AddColumn(new SpontaneouslyInventedColumn(repo, "Min", $"min({GetColumnSelectSql()})"));

        switch (ColumnInfo.GetQuerySyntaxHelper().DatabaseType)
        {
            case DatabaseType.MicrosoftSQLServer:
                qb.AddColumn(new SpontaneouslyInventedColumn(repo, "stdev ", $"stdev({GetColumnSelectSql()})"));
                break;
            case DatabaseType.MySql:
            case DatabaseType.Oracle:
                qb.AddColumn(new SpontaneouslyInventedColumn(repo, "stddev ", $"stddev({GetColumnSelectSql()})"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        qb.AddColumn(new SpontaneouslyInventedColumn(repo, "avg", $"avg({GetColumnSelectSql()})"));
    }

    /// <summary>
    ///     Returns the column Select SQL (without alias) for use in query building
    /// </summary>
    /// <returns></returns>
    private string GetColumnSelectSql()
    {
        return GetIColumn().SelectSQL;
    }

    public string GetTabName()
    {
        return $"{GetIColumn()}({ViewType})";
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
        if (ColumnInfo != null) autoComplete.Add(ColumnInfo);
    }

    private IFilter GetFilterIfAny()
    {
        return (IFilter)DatabaseObjects.SingleOrDefault(o => o is IFilter);
    }

    private IContainer GetContainerIfAny()
    {
        return (IContainer)DatabaseObjects.SingleOrDefault(o => o is IContainer);
    }


    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        var c = ColumnInfo;
        return c?.GetQuerySyntaxHelper();
    }
}