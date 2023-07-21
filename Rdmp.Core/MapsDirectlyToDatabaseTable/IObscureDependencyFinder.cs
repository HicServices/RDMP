// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     Provides cross database/server referential integrity and referential integrity where the dependency requires
///     programming logic (for example preventing
///     deleting objects that are referenced in Catalogue.ValidatorXML).
///     <para>
///         Also handles CASCADEing between databases/servers as above (when a delete is permitted but it means other
///         objects should suddenly also be deleted).
///     </para>
///     <para>
///         IObscureDependencyFinders are global hooks that are installed into an IRepository (usually a TableRepository)
///         which ensure that the user cannot do
///         dangerous deletes / leave orphan objects where simple database level logic cannot be implemented to enforce the
///         rule (e.g. foreign key constraints).
///     </para>
/// </summary>
public interface IObscureDependencyFinder
{
    /// <summary>
    ///     Throws if there are relationships that cannot be handled with database constraints that should prevent the deletion
    ///     of the passed object.  For example a
    ///     CatalogueItem could have deletion prevented by the fact that it is referenced in a Catalogue ValidationXml (this
    ///     dependency could not be modelled at database
    ///     level).  An Exception is thrown if the delete is not allowed
    /// </summary>
    /// <param name="oTableWrapperObject"></param>
    void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject);

    /// <summary>
    ///     Callback for when an <see cref="IMapsDirectlyToDatabaseTable" /> object has just been deleted.  This method
    ///     automatically cleans up any objects which the
    ///     <see cref="IObscureDependencyFinder" /> thinks are dependent in a way similar to a CASCADE foreign key constraint.
    ///     This should only ever include cases
    ///     where it is not possible to model the behaviour at database level e.g. when the dependency is cross server/databse.
    ///     <para>
    ///         <remarks>
    ///             Do not attempt to Save or Delete or really do any other database level operations with the parameter
    ///             <paramref name="oTableWrapperObject" /> because
    ///             it will no longer exists in the database as a record (<see cref="IRevertable.Exists" /> will be false).
    ///         </remarks>
    ///     </para>
    /// </summary>
    /// <param name="oTableWrapperObject"></param>
    void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject);
}