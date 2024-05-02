// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Handles rules for cascading/preventing deleting database objects which cannot be directly implemented by database
///     constraints (e.g. foreign keys).  This includes
///     things such as preventing deleting Catalogues which have been used in data extraction projects.  Use property
///     OtherDependencyFinders to add new rules / logic for
///     tailoring deleting.
/// </summary>
public class CatalogueObscureDependencyFinder : IObscureDependencyFinder
{
    private readonly ICatalogueRepository _repository;

    /// <summary>
    ///     Sets the target upon which to apply delete/cascade obscure dependency rules
    /// </summary>
    /// <param name="repository"></param>
    public CatalogueObscureDependencyFinder(ICatalogueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Plugin and supplemental child IObscureDependencyFinders
    /// </summary>
    public List<IObscureDependencyFinder> OtherDependencyFinders = new();

    /// <inheritdoc />
    /// <remarks>Consults each <see cref="OtherDependencyFinders" /> to see if deleting is disallowed</remarks>
    /// <param name="oTableWrapperObject"></param>
    public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        foreach (var obscureDependencyFinder in OtherDependencyFinders)
            obscureDependencyFinder.ThrowIfDeleteDisallowed(oTableWrapperObject);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Consults each <see cref="OtherDependencyFinders" /> for CASCADE logic then deletes any
    ///     <see cref="AnyTableSqlParameter" /> declared
    ///     on the deleted object
    /// </remarks>
    /// <param name="oTableWrapperObject"></param>
    public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        foreach (var obscureDependencyFinder in OtherDependencyFinders)
            obscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);

        //Delete any SQLFilterParameters associated with the parent object (which has just been deleted!)
        if (AnyTableSqlParameter.IsSupportedType(oTableWrapperObject.GetType()))
            foreach (var p in _repository.GetAllParametersForParentTable(oTableWrapperObject))
                p.DeleteInDatabase();
    }

    /// <summary>
    ///     Adds a new instance of the supplied  <see cref="IObscureDependencyFinder" /> to
    ///     <see cref="OtherDependencyFinders" /> if it is a unique Type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repositoryLocator"></param>
    public void AddOtherDependencyFinderIfNotExists<T>(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        where T : IObscureDependencyFinder
    {
        if (OtherDependencyFinders.All(f => f.GetType() != typeof(T)))
            OtherDependencyFinders.Add((T)ObjectConstructor.Construct(typeof(T), repositoryLocator));
    }
}