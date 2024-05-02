// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryBuilding;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Autocomplete;

/// <summary>
///     Creates autocomplete strings based on RDMP objects (e.g. <see cref="TableInfo" />)
/// </summary>
public partial class AutoCompleteProvider : IAutoCompleteProvider
{
    public HashSet<string> Items { get; set; } = new();

    /// <summary>
    ///     Array of images that items can be depicted with.  Use <see cref="ItemsWithImages" /> to index into
    ///     this array to get the image out
    /// </summary>
    public Image<Rgba32>[] Images;

    public Dictionary<string, int> ItemsWithImages { get; set; } = new();

    private const int TABLE_INFO_IDX = 0;
    private const int COLUMN_INFO_IDX = 1;
    private const int SQL_IDX = 2;
    private const int PARAMETER_IDX = 3;

    public AutoCompleteProvider()
    {
        Images = new Image<Rgba32>[4];

        Images[TABLE_INFO_IDX] = Image.Load<Rgba32>(CatalogueIcons.TableInfo);
        Images[COLUMN_INFO_IDX] = Image.Load<Rgba32>(CatalogueIcons.ColumnInfo);
        Images[SQL_IDX] = Image.Load<Rgba32>(CatalogueIcons.SQL);
        Images[PARAMETER_IDX] = Image.Load<Rgba32>(CatalogueIcons.ParametersNode);
    }

    public AutoCompleteProvider(IQuerySyntaxHelper helper) : this()
    {
        if (helper != null) AddSQLKeywords(helper);
    }

    /// <summary>
    ///     Splits <paramref name="arg" /> into individual autocomplete words for suggestions
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetBits(string arg)
    {
        return Words().Matches(arg).Select(m => m.Value);
    }

    public void Add(ITableInfo tableInfo)
    {
        Add(tableInfo, LoadStage.PostLoad);
    }


    public void Add(ColumnInfo columnInfo, ITableInfo tableInfo, string databaseName, LoadStage stage,
        IQuerySyntaxHelper syntaxHelper)
    {
        var col = columnInfo.GetRuntimeName(stage);
        var table = tableInfo.GetRuntimeName(stage);
        var dbName = tableInfo.GetDatabaseRuntimeName(stage);

        var fullySpecified = syntaxHelper.EnsureFullyQualified(dbName, tableInfo.Schema, table, col);

        AddUnlessDuplicate(fullySpecified);
        AddUnlessDuplicateImage(fullySpecified, COLUMN_INFO_IDX);
    }

    public void Add(ColumnInfo columnInfo)
    {
        AddUnlessDuplicate(columnInfo.GetFullyQualifiedName());
        AddUnlessDuplicateImage(columnInfo.GetFullyQualifiedName(), COLUMN_INFO_IDX);
    }

    private void Add(PreLoadDiscardedColumn discardedColumn, ITableInfo tableInfo, string rawDbName)
    {
        var colName = discardedColumn.GetRuntimeName();
        var representation = tableInfo.GetQuerySyntaxHelper()
            .EnsureFullyQualified(rawDbName, null, tableInfo.GetRuntimeName(), colName);
        AddUnlessDuplicate(representation);
        AddUnlessDuplicateImage(representation, COLUMN_INFO_IDX);
    }

    public void Add(IColumn column)
    {
        try
        {
            _ = column.GetRuntimeName();
        }
        catch (Exception)
        {
            return;
        }

        AddUnlessDuplicate(column.SelectSQL);
        AddUnlessDuplicateImage(column.SelectSQL, COLUMN_INFO_IDX);
    }

    private void AddUnlessDuplicate(string text)
    {
        Items.Add(text);
    }

    private void AddUnlessDuplicateImage(string fullySpecified, int idx)
    {
        ItemsWithImages.TryAdd(fullySpecified, idx);
    }

    public void AddSQLKeywords(IQuerySyntaxHelper syntaxHelper)
    {
        if (syntaxHelper == null)
            return;

        foreach (var kvp in syntaxHelper.GetSQLFunctionsDictionary())
        {
            AddUnlessDuplicate(kvp.Value);
            AddUnlessDuplicateImage(kvp.Value, SQL_IDX);
        }
    }

    public void Add(ISqlParameter parameter)
    {
        AddUnlessDuplicate(parameter.ParameterName);
        AddUnlessDuplicateImage(parameter.ParameterName, PARAMETER_IDX);
    }

    public void Add(ITableInfo tableInfo, LoadStage loadStage)
    {
        //we already have it or it is not setup properly
        if (string.IsNullOrWhiteSpace(tableInfo.Database) || string.IsNullOrWhiteSpace(tableInfo.Server))
            return;

        var runtimeName = tableInfo.GetRuntimeName(loadStage);
        var dbName = tableInfo.GetDatabaseRuntimeName(loadStage);

        var syntaxHelper = tableInfo.GetQuerySyntaxHelper();
        var fullSql = syntaxHelper.EnsureFullyQualified(dbName, null, runtimeName);


        foreach (var o in tableInfo.GetColumnsAtStage(loadStage))
            switch (o)
            {
                case PreLoadDiscardedColumn preDiscarded:
                    Add(preDiscarded, tableInfo, dbName);
                    break;
                case ColumnInfo columnInfo:
                    Add(columnInfo, tableInfo, dbName, loadStage, syntaxHelper);
                    break;
                default:
                    throw new Exception(
                        $"Expected IHasStageSpecificRuntimeName returned by TableInfo.GetColumnsAtStage to return only ColumnInfos and PreLoadDiscardedColumns.  It returned a '{o.GetType().Name}'");
            }

        AddUnlessDuplicate(fullSql);
        AddUnlessDuplicateImage(fullSql, TABLE_INFO_IDX);
    }

    public void Add(DiscoveredTable discoveredTable)
    {
        AddUnlessDuplicate(discoveredTable.GetFullyQualifiedName());
        AddUnlessDuplicateImage(discoveredTable.GetFullyQualifiedName(), TABLE_INFO_IDX);

        DiscoveredColumn[] columns = null;
        try
        {
            if (discoveredTable.Exists())
                columns = discoveredTable.DiscoverColumns();
        }
        catch (Exception)
        {
            //couldn't load nevermind
        }

        if (columns != null)
            foreach (var col in columns)
                Add(col);
    }

    private void Add(DiscoveredColumn discoveredColumn)
    {
        AddUnlessDuplicate(discoveredColumn.GetFullyQualifiedName());
        AddUnlessDuplicateImage(discoveredColumn.GetFullyQualifiedName(), COLUMN_INFO_IDX);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void Add(Type type)
    {
        Items.Add(type.Name);
    }

    public void Add(AggregateConfiguration aggregateConfiguration)
    {
        Add(aggregateConfiguration.Catalogue);
    }

    public void Add(ICatalogue catalogue)
    {
        foreach (var ei in catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            Add(ei);
    }

    [GeneratedRegex("\\b\\w*\\b")]
    private static partial Regex Words();
}