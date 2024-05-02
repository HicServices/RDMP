// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     This data class reflects a single row in a cohortDefinition table (see <see cref="ExternalCohortTable" />).  It may
///     also reflect
///     one that does not exist yet in which case it will have a null ID (e.g. in the case where you are trying to create a
///     new cohort
///     using an identifier list).
/// </summary>
public class CohortDefinition : ICohortDefinition
{
    /// <inheritdoc />
    public int? ID { get; set; }

    /// <inheritdoc />
    public string Description { get; set; }

    /// <inheritdoc />
    public int Version { get; set; }

    /// <inheritdoc />
    public int ProjectNumber { get; set; }

    /// <inheritdoc />
    public IExternalCohortTable LocationOfCohort { get; }

    /// <inheritdoc />
    public IExtractableCohort CohortReplacedIfAny { get; set; }

    /// <summary>
    ///     Sets up a new row for inserting (or reporting) from an <see cref="ExternalCohortTable" />.
    /// </summary>
    /// <param name="id">
    ///     The ID row read from the table (this is not an RDMP ID, it is an <see cref="IExtractableCohort.OriginID" />).  Pass
    ///     null if you
    ///     are trying to insert a new row and expect the database to allocate the ID itself as an autonum
    /// </param>
    /// <param name="description">
    ///     Unique string identifying the cohort, this should be the same for all cohorts that are
    ///     versions of one another
    /// </param>
    /// <param name="version">
    ///     The version number where there are multiple revisions to a cohort over time (these must share the
    ///     same <paramref name="description" />)
    /// </param>
    /// <param name="projectNumber">The <see cref="IProject.ProjectNumber" /> that the cohort can be used with</param>
    /// <param name="locationOfCohort">The database where the row will be written to (or read from)</param>
    public CohortDefinition(int? id, string description, int version, int projectNumber,
        IExternalCohortTable locationOfCohort)
    {
        ID = id;
        Description = description;
        Version = version;
        ProjectNumber = projectNumber;
        LocationOfCohort = locationOfCohort;

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description),
                id == null
                    ? "Cohorts must have a description"
                    : $"There is a cohort (with ID {id}) in {locationOfCohort.DefinitionTableName} which has a blank/null description.  You must fix this.");
    }

    /// <inheritdoc />
    public bool IsAcceptableAsNewCohort(out string matchDescription)
    {
        //if there is an ID
        if (ID != null)
            if (ExtractableCohort.GetImportableCohortDefinitions((ExternalCohortTable)LocationOfCohort)
                .Any(t => t.ID == ID))
                //the same ID already exists
            {
                matchDescription = $"Found a cohort in {LocationOfCohort} with the ID {ID}";
                return false;
            }


        try
        {
            var foundSimilar = ExtractableCohort
                .GetImportableCohortDefinitions(
                    (ExternalCohortTable)LocationOfCohort) //see if there is one with the same name
                .Any(t => t.Description.Equals(Description) &&
                          t.Version.Equals(
                              Version)); //and description (it might have a different ID but it is still against the rules)
            if (foundSimilar)
            {
                matchDescription =
                    $"Found an existing cohort called {Description} with version {Version} in {LocationOfCohort}";
                return false;
            }
        }
        catch (Exception ex)
        {
            matchDescription =
                $"Error occurred when checking for existing cohorts in the Project.  We were looking in {LocationOfCohort + Environment.NewLine + Environment.NewLine} Error was: {ExceptionHelper.ExceptionToListOfInnerMessages(ex)}";

            return false;
        }

        //did not find any conflicting cohort definitions
        matchDescription = null;
        return true;
    }

    /// <summary>
    ///     Returns the <see cref="Description" />, <see cref="Version" /> and <see cref="ID" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Description}(Version {Version}, ID={ID})";
    }
}