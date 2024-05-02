// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandViewFilterMatchData : ExecuteCommandViewDataBase, IAtomicCommand
{
    private readonly IFilter _filter;
    private readonly IContainer _container;

    private readonly ViewType _viewType;
    private ColumnInfo _columnInfo;
    private ColumnInfo[] _candidates;

    /// <summary>
    ///     Views an extract of data from a column that matches a given <paramref name="filter" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="filter"></param>
    /// <param name="viewType"></param>
    /// <param name="columnName"></param>
    /// <param name="toFile"></param>
    public ExecuteCommandViewFilterMatchData(IBasicActivateItems activator,
        [DemandsInitialization("The filter you want to view matching data on (e.g. an AggregateFilter)")]
        IFilter filter,
        [DemandsInitialization("What kind of data do you want to fetch")]
        ViewType viewType = ViewType.TOP_100,
        [DemandsInitialization(
            "If filter is not implicitly tied to a specific column, pass the name of the column for whom you want to view data.")]
        string columnName = null,
        [DemandsInitialization(ToFileDescription)]
        FileInfo toFile = null) : this(activator, viewType, toFile)
    {
        _filter = filter;

        if (!string.IsNullOrWhiteSpace(columnName))
        {
            var c = filter.GetCatalogue();
            var candidates = GetCandidates(c);

            // match on exact name?
            _columnInfo = candidates.FirstOrDefault(c =>
                c.GetRuntimeName().Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            if (_columnInfo == null)
                throw new Exception($"Could not find a ColumnInfo called '{columnName}' in Catalogue '{c}'");
        }
        else
        {
            _columnInfo = filter.GetColumnInfoIfExists();

            //there is a single column associated with the filter?
            if (_columnInfo != null)
                return;
        }

        // there is no single column associated with the filter so get user to pick one of them
        PopulateCandidates(filter.GetCatalogue(), filter);
    }

    public ExecuteCommandViewFilterMatchData(IBasicActivateItems activator, IContainer container,
        ViewType viewType = ViewType.TOP_100) : this(activator, viewType, null)
    {
        _container = container;

        PopulateCandidates(container.GetCatalogueIfAny(), container);
    }

    private void PopulateCandidates(Catalogue catalogue, object rootObj)
    {
        _candidates = GetCandidates(catalogue);

        if (!_candidates.Any())
            SetImpossible($"No ColumnInfo is associated with '{rootObj}'");
    }

    private ColumnInfo[] GetCandidates(Catalogue catalogue)
    {
        if (catalogue == null)
        {
            SetImpossible("Filter has no Catalogue");
            return null;
        }

        return catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Select(e => e.ColumnInfo)
            .Where(c => c != null).Distinct().ToArray();
    }

    protected ExecuteCommandViewFilterMatchData(IBasicActivateItems activator, ViewType viewType, FileInfo toFile) :
        base(activator, toFile)
    {
        _viewType = viewType;
    }

    public override string GetCommandName()
    {
        return _viewType switch
        {
            ViewType.TOP_100 => "View Extract",
            ViewType.Aggregate => "View Aggregate",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected override IViewSQLAndResultsCollection GetCollection()
    {
        _columnInfo ??= SelectOne(_candidates, _columnInfo != null ? _columnInfo.Name : "");

        if (_columnInfo == null)
            return null;

        ViewColumnExtractCollection collection = null;

        if (_filter != null)
            collection = new ViewColumnExtractCollection(_columnInfo, _viewType, _filter);
        if (_container != null)
            collection = new ViewColumnExtractCollection(_columnInfo, _viewType, _container);

        return collection == null
            ? throw new Exception("ViewFilterMatchData Command had no filter or container")
            : (IViewSQLAndResultsCollection)collection;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Filter);
    }
}