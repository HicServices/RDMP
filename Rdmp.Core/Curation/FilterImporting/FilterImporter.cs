// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;

namespace Rdmp.Core.Curation.FilterImporting;

/// <summary>
///     Facilitates the import of one IFilter type into the scope of another IFilter collection.  IFilters are lines of
///     WHERE Sql.  This class is what allows you to import
///     a Catalogue level filter (ExtractionFilter) into specific deployment containers e.g. during cohort creation task
///     you can import a copy of 'prescribed @drugX' into
///     your AggregateFilterContainer (this will actually create an AggregateFilter) and then import 2 more copies of that
///     IFilter.  This class ensures that parameter names
///     are unique so you can change the value of @drugX for each new IFilter imported.
/// </summary>
public class FilterImporter
{
    private readonly IFilterFactory _factory;
    private readonly ISqlParameter[] _globals;

    /// <summary>
    ///     Specifies overriding definitions for <see cref="ISqlParameter" /> which should be used with all
    ///     <see cref="IFilter" /> built by this class.  When you import
    ///     a master filter that has parameters then this array will be consulted.  If a matching parameter name is found then
    ///     the imported parameter will have that value
    ///     rather than its default.
    ///     <para>
    ///         <seealso cref="ExtractionFilterParameterSet" />
    ///     </para>
    /// </summary>
    public ISqlParameter[] AlternateValuesToUseForNewParameters { get; set; }

    /// <summary>
    ///     Sets up the factory to import filters which may or may not have parameters on them.
    /// </summary>
    /// <param name="factory">Determines which Type of <see cref="IFilter" /> is created</param>
    /// <param name="globals">
    ///     If filters being imported have parameters that match the names of these globals the global value
    ///     will be used to override.
    /// </param>
    public FilterImporter(IFilterFactory factory, ISqlParameter[] globals)
    {
        _factory = factory;
        _globals = globals;
    }

    /// <summary>
    ///     Creates a copy of the <paramref name="fromMaster" /> filter and any parameters it might have.  This will handle
    ///     collisions on parameter name with
    ///     <paramref name="existingFiltersAlreadyInScope" /> and will respect any globals this class was constructed with.
    /// </summary>
    /// <param name="containerToImportOneInto"></param>
    /// <param name="fromMaster"></param>
    /// <param name="existingFiltersAlreadyInScope"></param>
    /// <returns></returns>
    public IFilter ImportFilter(IContainer containerToImportOneInto, IFilter fromMaster,
        IFilter[] existingFiltersAlreadyInScope)
    {
        if (fromMaster is ExtractionFilter extractionFilter &&
            extractionFilter.ExtractionInformation.ColumnInfo == null)
            throw new Exception(
                $"Could not import filter {extractionFilter} because it could not be traced back to a ColumnInfo");

        //If user is trying to publish a filter into the Catalogue as a new master top level filter, make sure it is properly documented
        if (_factory is ExtractionFilterFactory)
            if (!IsProperlyDocumented(fromMaster, out var reason))
                throw new Exception($"Cannot clone filter called '{fromMaster.Name}' because:{reason}");

        //Handle problems with existing filters
        existingFiltersAlreadyInScope ??= Array.Empty<IFilter>();

        if (existingFiltersAlreadyInScope.Contains(fromMaster))
            throw new ArgumentException(
                "Master filter (that you are trying to import) cannot be part of the existing filters collection!");

        //Ensure that the new filter has a unique name within the scope
        var name = fromMaster.Name;

        while (existingFiltersAlreadyInScope.Any(f => f.Name.Equals(name)))
            name = $"Copy of {name}";

        //create the filter
        var newFilter = _factory.CreateNewFilter(name);

        //Now copy across all the values from the master
        newFilter.Description = fromMaster.Description;
        newFilter.IsMandatory = fromMaster.IsMandatory;
        newFilter.WhereSQL = fromMaster.WhereSQL;

        //if we are down cloning from a master ExtractionFilter so record that the new filter is
        if (fromMaster is ExtractionFilter)
            newFilter.ClonedFromExtractionFilter_ID = fromMaster.ID; //make the new filters parent the master

        //if we are up cloning we are publishing a child into being a new master catalogue filter (ExtractionFilter)
        if (newFilter is ExtractionFilter)
        {
            newFilter.Description +=
                $"{Environment.NewLine} Published by {Environment.UserName} on {DateTime.Now} from object {fromMaster.GetType().Name} with ID {fromMaster.ID}";
            fromMaster.ClonedFromExtractionFilter_ID =
                newFilter.ID; //Make the newly created master our parent (since we are published)
        }

        if (containerToImportOneInto != null) newFilter.FilterContainer_ID = containerToImportOneInto.ID;

        newFilter.SaveToDatabase();

        //If there are some filters already in scope then we need to take into account their parameters when it comes to importing, so fetch a union of all the parameters
        var existingFiltersParametersAlreadyInScope =
            existingFiltersAlreadyInScope.SelectMany(f => f.GetAllParameters()).ToArray();

        //now create parameters while respecting globals
        var parameterCreator = new ParameterCreator(_factory, _globals,
            AlternateValuesToUseForNewParameters ?? fromMaster.GetAllParameters());
        parameterCreator.CreateAll(newFilter,
            existingFiltersParametersAlreadyInScope); //Create the parameters while handling the existing parameters in scope

        return newFilter;
    }


    /// <summary>
    ///     Imports a collection of IFilters of one type into another type.  Destination type corresponds to the factory.
    ///     Returns the newly created filters.
    /// </summary>
    /// <param name="containerToImportInto"></param>
    /// <param name="allMasters"></param>
    /// <param name="existingFiltersAlreadyInScope"></param>
    /// <returns></returns>
    public IFilter[] ImportAllFilters(IContainer containerToImportInto, IFilter[] allMasters,
        IFilter[] existingFiltersAlreadyInScope)
    {
        var createdSoFar = new List<IFilter>();

        existingFiltersAlreadyInScope ??= Array.Empty<IFilter>();

        foreach (var master in allMasters)
        {
            var added = ImportFilter(containerToImportInto, master,
                createdSoFar.Union(existingFiltersAlreadyInScope).ToArray());
            createdSoFar.Add(added);
        }

        return createdSoFar.Except(existingFiltersAlreadyInScope).ToArray();
    }

    /// <summary>
    ///     Returns true if the <paramref name="filter" /> has a proper description, name etc.  This helps prevent poorly
    ///     documented master filters.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static bool IsProperlyDocumented(IFilter filter, out string reason)
    {
        reason = null;

        if (string.IsNullOrWhiteSpace(filter.Description))
            reason = "There is no description";
        else if (filter.Description.Length <= 20)
            reason = "Description is not long enough (minimum length is 20 characters)";
        else if (string.IsNullOrWhiteSpace(filter.WhereSQL))
            reason = "WhereSQL is not populated";

        //if we have not yet found a reason to complain, look at parameters for a reason to complain
        if (reason == null)
            //check to see if there's a problem with the parameters
            foreach (var filterParameter in filter.GetAllParameters())
                if (!ExtractionFilterParameter.IsProperlyDocumented(filterParameter, out var reasonParameterRejected))
                {
                    reason = $"Parameter '{filterParameter.ParameterName}' was rejected :{reasonParameterRejected}";
                    break;
                }

        return reason == null;
    }
}