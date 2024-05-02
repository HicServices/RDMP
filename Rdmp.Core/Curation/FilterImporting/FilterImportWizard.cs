// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.QueryBuilding.Options;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.FilterImporting;

/// <summary>
///     Handles deploying <see cref="ExtractionFilter" /> instances into cohort identification / data extraction
///     <see cref="IContainer" />s.
///     This adds WHERE logic to the query the user is building.  The interactive bits of this class only come into effect
///     when there are
///     one or more <see cref="ExtractionFilterParameterSet" /> configured that they can select from.
/// </summary>
public class FilterImportWizard
{
    private readonly IBasicActivateItems _activator;

    public FilterImportWizard(IBasicActivateItems activator)
    {
        _activator = activator;
    }

    public IFilter Import(IContainer containerToImportOneInto, IFilter filterToImport)
    {
        return Import(containerToImportOneInto, filterToImport, null);
    }

    public IFilter Import(IContainer containerToImportOneInto, IFilter filterToImport,
        ExtractionFilterParameterSet parameterSet)
    {
        GetGlobalsAndFilters(containerToImportOneInto, out var globals, out var otherFilters);
        return Import(containerToImportOneInto, filterToImport, globals, otherFilters, parameterSet);
    }

    public IFilter ImportOneFromSelection(IContainer containerToImportOneInto, IFilter[] filtersThatCouldBeImported)
    {
        GetGlobalsAndFilters(containerToImportOneInto, out var global, out var otherFilters);
        return ImportOneFromSelection(containerToImportOneInto, filtersThatCouldBeImported, global, otherFilters);
    }

    public IEnumerable<IFilter> ImportManyFromSelection(IContainer containerToImportOneInto,
        IFilter[] filtersThatCouldBeImported)
    {
        GetGlobalsAndFilters(containerToImportOneInto, out var global, out var otherFilters);
        return ImportManyFromSelection(containerToImportOneInto, filtersThatCouldBeImported, global, otherFilters);
    }

    private IFilter Import(IContainer containerToImportOneInto, IFilter filterToImport,
        ISqlParameter[] globalParameters, IFilter[] otherFiltersInScope, ExtractionFilterParameterSet parameterSet)
    {
        var cancel = false;

        //Sometimes filters have some recommended parameter values which the user can pick from (e.g. filter Condition could have parameter value sets for 'Dementia', 'Alzheimers' etc
        var chosenParameterValues =
            parameterSet ?? AdvertiseAvailableFilterParameterSetsIfAny(filterToImport, out cancel);

        if (cancel)
            return null;

        var importer = containerToImportOneInto switch
        {
            AggregateFilterContainer => new FilterImporter(
                new AggregateFilterFactory((ICatalogueRepository)containerToImportOneInto.Repository),
                globalParameters),
            FilterContainer => new FilterImporter(
                new DeployedExtractionFilterFactory((IDataExportRepository)containerToImportOneInto.Repository),
                globalParameters),
            _ => throw new ArgumentException(
                $"Cannot import into IContainer of type {containerToImportOneInto.GetType().Name}",
                nameof(containerToImportOneInto))
        };

        //if there is a parameter value set then tell the importer to use these parameter values instead of the IFilter's default ones
        if (chosenParameterValues != null)
            importer.AlternateValuesToUseForNewParameters = chosenParameterValues.GetAllParameters();

        //create the filter
        var newFilter = importer.ImportFilter(containerToImportOneInto, filterToImport, otherFiltersInScope);

        //if we used custom parameter values we should update the filter name so the user is reminded that the concept of the filter includes both 'Condition' and the value they selected e.g. 'Dementia'
        if (chosenParameterValues != null)
        {
            newFilter.Name += $"_{chosenParameterValues.Name}";
            newFilter.SaveToDatabase();
        }

        //If we've not imported values but there is a parameter then ask for their values
        if (chosenParameterValues == null && newFilter.GetAllParameters().Any())
            foreach (var parameter in newFilter.GetAllParameters())
            {
                var initialText = parameter.Value;
                if (initialText == AnyTableSqlParameter.DefaultValue) initialText = null;

                if (_activator.IsInteractive && _activator.TypeText(
                        AnyTableSqlParameter.GetValuePromptDialogArgs(newFilter, parameter), 255, initialText,
                        out var param, false))
                {
                    parameter.Value = param;
                    parameter.SaveToDatabase();
                }
            }

        return newFilter;
    }

    private IFilter ImportOneFromSelection(IContainer containerToImportOneInto, IFilter[] filtersThatCouldBeImported,
        ISqlParameter[] globalParameters, IFilter[] otherFiltersInScope)
    {
        var chosenFilter = _activator.SelectOne("Import filter", filtersThatCouldBeImported);

        if (chosenFilter != null)
            return Import(containerToImportOneInto, (IFilter)chosenFilter, globalParameters, otherFiltersInScope, null);

        return null; //user chose not to import anything
    }

    private IEnumerable<IFilter> ImportManyFromSelection(IContainer containerToImportOneInto,
        IFilter[] filtersThatCouldBeImported, ISqlParameter[] globalParameters, IFilter[] otherFiltersInScope)
    {
        var results = _activator.SelectMany(new DialogArgs
        {
            WindowTitle = "Import Filter(s)",
            EntryLabel = "Import",
            TaskDescription =
                "The following Catalogue filters are available for importing.  Selecting a filter will make a new cloned copy in your WHERE container.  If a filter has declared parameters you may be prompted to pick from an existing predetermined set of values."
        }, typeof(ExtractionFilter), filtersThatCouldBeImported);

        if (results is not null)
            foreach (var f in results)
            {
                var i = Import(containerToImportOneInto, (IFilter)f, globalParameters, otherFiltersInScope, null);

                // returns null if cancelled
                if (i != null)
                    yield return i;
                else
                    // user cancelled import half way through
                    yield break;
            }
    }

    private ExtractionFilterParameterSet AdvertiseAvailableFilterParameterSetsIfAny(IFilter filter, out bool cancel)
    {
        cancel = false;

        //only advertise filter parameter sets if it is a master level filter (Catalogue level)
        if (filter is not ExtractionFilter extractionFilterOrNull)
            return null;

        var parameterSets =
            extractionFilterOrNull.Repository.GetAllObjectsWithParent<ExtractionFilterParameterSet>(
                extractionFilterOrNull);


        if (parameterSets.Any())
        {
            var chosen = _activator.SelectOne(new DialogArgs
            {
                WindowTitle = "Choose Parameter Set",
                TaskDescription =
                    @$"Filter '{filter}' has parameters ({string.Join(',', filter.GetAllParameters().Select(p => p.ParameterName))}).  There are existing parameter sets configured for these parameters.  Choose which parameter values to use with this filter"
            }, parameterSets);

            if (chosen != null)
                return chosen as ExtractionFilterParameterSet;
            cancel = true;
        }

        return null;
    }

    private void GetGlobalsAndFilters(IContainer containerToImportOneInto, out ISqlParameter[] globals,
        out IFilter[] otherFilters)
    {
        switch (containerToImportOneInto)
        {
            case AggregateFilterContainer aggregatecontainer:
            {
                var aggregate = aggregatecontainer.GetAggregate();
                var options = AggregateBuilderOptionsFactory.Create(aggregate);

                globals = options.GetAllParameters(aggregate);
                var root = aggregate.RootFilterContainer;
                otherFilters = root == null
                    ? Array.Empty<IFilter>()
                    : GetAllFiltersRecursively(root, new List<IFilter>()).ToArray();
                return;
            }
            case FilterContainer filtercontainer:
            {
                var selectedDataSet = filtercontainer.GetSelectedDataSetsRecursively() ??
                                      throw new Exception(
                                          $"Cannot import filter container {filtercontainer} because it does not belong to any SelectedDataSets");
                var config = selectedDataSet.ExtractionConfiguration;
                var root = selectedDataSet.RootFilterContainer;

                globals = config.GlobalExtractionFilterParameters;
                otherFilters = root == null
                    ? Array.Empty<IFilter>()
                    : GetAllFiltersRecursively(root, new List<IFilter>()).ToArray();

                return;
            }
            default:
                throw new Exception(
                    $"Container {containerToImportOneInto} was an unexpected Type:{containerToImportOneInto.GetType().Name}");
        }
    }

    private List<IFilter> GetAllFiltersRecursively(IContainer currentContainer, List<IFilter> foundSoFar)
    {
        foreach (var container in currentContainer.GetSubContainers())
            foundSoFar.AddRange(GetAllFiltersRecursively(container, foundSoFar));

        foundSoFar.AddRange(currentContainer.GetFilters());

        return foundSoFar;
    }
}