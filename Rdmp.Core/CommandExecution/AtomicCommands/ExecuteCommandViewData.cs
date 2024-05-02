// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using NLog;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandViewData : ExecuteCommandViewDataBase, IAtomicCommand
{
    private readonly IViewSQLAndResultsCollection _collection;
    private readonly ViewType _viewType;
    private readonly IMapsDirectlyToDatabaseTable _obj;
    private readonly bool _useCache;

    #region Constructors

    /// <summary>
    ///     Provides a view of a sample of records in a column/table
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="viewType"></param>
    /// <param name="toFile"></param>
    /// <param name="useCache"></param>
    /// <param name="obj"></param>
    /// <exception cref="ArgumentException"></exception>
    [UseWithObjectConstructor]
    public ExecuteCommandViewData(IBasicActivateItems activator,
        [DemandsInitialization("The object (ColumnInfo, TableInfo etc) you want to view a sample of")]
        IMapsDirectlyToDatabaseTable obj,
        [DemandsInitialization(
            "Optional. The view mode you want to see.  Options include 'TOP_100', 'Aggregate', 'Distribution' or 'All'",
            DefaultValue = ViewType.TOP_100)]
        ViewType viewType = ViewType.TOP_100,
        [DemandsInitialization(ToFileDescription)]
        FileInfo toFile = null,
        [DemandsInitialization(
            "Applies only to CohortIdentificationConfigurations.  Defaults to true.  Set to false to disable query cache use.")]
        bool useCache = true) : base(activator, toFile)
    {
        _viewType = viewType;
        _obj = obj;
        _useCache = useCache;

        switch (obj)
        {
            case TableInfo ti:
                ThrowIfNotSimpleSelectViewType();
                _collection = new ViewTableInfoExtractUICollection(ti, _viewType);
                break;
            case ColumnInfo col:
                _collection = CreateCollection(col);
                break;
            case ExtractionInformation ei:
                _collection = CreateCollection(ei);
                break;
            case Catalogue cata:
                ThrowIfNotSimpleSelectViewType();
                _collection = CreateCollection(cata);
                break;
            case CohortIdentificationConfiguration cic:
                ThrowIfNotSimpleSelectViewType();
                _collection = CreateCollection(cic);
                break;
            case ExtractableCohort ec:
                ThrowIfNotSimpleSelectViewType();
                _collection = CreateCollection(ec);
                break;
            case AggregateConfiguration ac:
                ThrowIfNotSimpleSelectViewType();
                _collection = CreateCollection(ac);
                break;
            default:
                throw new ArgumentException($"Object '{obj}' was not an object type compatible with this command");
        }
    }

    private IViewSQLAndResultsCollection CreateCollection(AggregateConfiguration ac)
    {
        var cic = ac.GetCohortIdentificationConfigurationIfAny();

        var collection = new ViewAggregateExtractUICollection(ac);

        //if it has a cic with a query cache AND it uses joinables.  Since this is a TOP 100 select * from dataset the cache on CHI is useless only patient index tables used by this query are useful if cached
        if (cic is { QueryCachingServer_ID: not null } && ac.PatientIndexJoinablesUsed.Any())
            collection.UseQueryCache = _useCache;

        collection.TopX = _viewType == ViewType.TOP_100 ? 100 : null;

        return collection;
    }

    private IViewSQLAndResultsCollection CreateCollection(ExtractableCohort ec)
    {
        return new ViewCohortExtractionUICollection(ec)
        {
            Top = _viewType == ViewType.TOP_100 ? 100 : -1,
            IncludeCohortID = false
        };
    }

    private IViewSQLAndResultsCollection CreateCollection(CohortIdentificationConfiguration cic)
    {
        if (_viewType == ViewType.TOP_100)
            LogManager.GetCurrentClassLogger()
                .Warn(
                    $"'{ViewType.TOP_100}' is not supported on '{nameof(CohortIdentificationConfiguration)}', '{ViewType.All}' will be used");

        return new ViewCohortIdentificationConfigurationSqlCollection(cic)
        {
            UseQueryCache = _useCache
        };
    }

    private void ThrowIfNotSimpleSelectViewType()
    {
        if (_viewType != ViewType.TOP_100 && _viewType != ViewType.All)
            throw new ArgumentException(
                $"Only '{nameof(ViewType.TOP_100)}' or '{nameof(ViewType.All)}' can be used for this object Type");
    }

    private IViewSQLAndResultsCollection CreateCollection(Catalogue cata)
    {
        return new ViewCatalogueDataCollection(cata)
        {
            TopX = _viewType == ViewType.All ? null : 100
        };
    }

    /// <summary>
    ///     Fetches the <paramref name="viewType" /> of the data in <see cref="ColumnInfo" /> <paramref name="c" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="viewType"></param>
    /// <param name="c"></param>
    public ExecuteCommandViewData(IBasicActivateItems activator, ViewType viewType, ColumnInfo c) : base(activator,
        null)
    {
        _viewType = viewType;
        _collection = CreateCollection(c);
    }

    public ExecuteCommandViewData(IBasicActivateItems activator, ViewType viewType, ExtractionInformation ei) : base(
        activator, null)
    {
        _viewType = viewType;
        _collection = CreateCollection(ei);
    }

    /// <summary>
    ///     Views the top 100 records of the <paramref name="tableInfo" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="tableInfo"></param>
    public ExecuteCommandViewData(IBasicActivateItems activator, TableInfo tableInfo) : base(activator, null)
    {
        _viewType = ViewType.TOP_100;
        _collection = new ViewTableInfoExtractUICollection(tableInfo, _viewType);
    }

    #endregion

    private IViewSQLAndResultsCollection CreateCollection(ColumnInfo c)
    {
        var toReturn = new ViewColumnExtractCollection(c, _viewType);

        if (!c.IsNumerical() && _viewType == ViewType.Distribution)
            SetImpossible("Column is not numerical");

        return toReturn;
    }

    private IViewSQLAndResultsCollection CreateCollection(ExtractionInformation ei)
    {
        var toReturn = new ViewColumnExtractCollection(ei, _viewType);
        if ((!ei.ColumnInfo?.IsNumerical() ?? false) && _viewType == ViewType.Distribution)
            SetImpossible("Column is not numerical");

        return toReturn;
    }

    public override string GetCommandName()
    {
        // if user has set an override, respect it
        if (!string.IsNullOrWhiteSpace(OverrideCommandName))
            return OverrideCommandName;

        return _obj is CohortIdentificationConfiguration
            ? _useCache ? "Query Builder SQL/Results" : "Query Builder SQL/Results (No Cache)"
            : $"View {_viewType.ToString().Replace("_", " ")}";
    }

    protected override IViewSQLAndResultsCollection GetCollection()
    {
        return _collection;
    }
}