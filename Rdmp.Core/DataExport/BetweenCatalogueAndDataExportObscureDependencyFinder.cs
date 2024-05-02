// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport;

/// <summary>
///     Prevents deleting objects in Catalogue database which are referenced by objects in Data Export database (between
///     databases referential integrity).  Also
///     handles cascading deletes between databases e.g. Deleting Project Associations when a Cohort Identification
///     Configuration is deleted (despite records being
///     in different databases).
/// </summary>
public class BetweenCatalogueAndDataExportObscureDependencyFinder : IObscureDependencyFinder
{
    private readonly IDataExportRepositoryServiceLocator _serviceLocator;

    /// <summary>
    ///     Sets up class to fobid deleting <see cref="Catalogue" /> that are in project extractions etc.
    /// </summary>
    /// <param name="serviceLocator"></param>
    public BetweenCatalogueAndDataExportObscureDependencyFinder(IDataExportRepositoryServiceLocator serviceLocator)
    {
        _serviceLocator = serviceLocator;
    }

    /// <inheritdoc />
    public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        //if there isn't a data export database then we don't care, delete away
        if (_serviceLocator.DataExportRepository == null)
            return;

        //they are trying to delete a catalogue
        if (oTableWrapperObject is Catalogue cata)
        {
            //they are deleting a catalogue! see if it has an ExtractableDataSet associated with it
            var dependencies = _serviceLocator.DataExportRepository
                .GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", cata.ID).ToArray();

            //we have any dependant catalogues?
            if (dependencies.Any())
                throw new Exception(
                    $"Cannot delete Catalogue {cata} because there are ExtractableDataSets which depend on them (IDs={string.Join(",", dependencies.Select(ds => ds.ID.ToString()))})");
        }
    }

    /// <inheritdoc />
    public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        //if the object being deleted is a CohortIdentificationConfiguration (in Catalogue database) then delete the associations it has to Projects in Data Export database
        if (oTableWrapperObject is CohortIdentificationConfiguration cic)
        {
            //data export functionality is not available?
            if (_serviceLocator.DataExportRepository == null)
                return;

            //delete all associations where the cic ID matches
            foreach (
                var association in
                _serviceLocator.DataExportRepository
                    .GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>()
                    .Where(assoc => assoc.CohortIdentificationConfiguration_ID == cic.ID))
                association.DeleteInDatabase();
        }
    }
}