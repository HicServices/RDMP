// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Sharing.Dependency;

/// <summary>
///     Handles preventing deletion of shareable references to existing classes e.g. if a Catalogue is shared (has an entry
///     in ObjectExport table) then you
///     cannot delete it.  Also handles cascading deletes of imported classes e.g. if a Catalogue was imported from
///     somewhere else (has an entry in ObjectImport) and
///     then you delete it the ObjectImport reference will also be deleted.
/// </summary>
public class ObjectSharingObscureDependencyFinder : IObscureDependencyFinder
{
    private readonly ShareManager _shareManager;

    public ObjectSharingObscureDependencyFinder(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        _shareManager = new ShareManager(repositoryLocator);
    }

    public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        if (_shareManager.IsExportedObject(oTableWrapperObject))
            throw new Exception(
                $"You cannot Delete '{oTableWrapperObject}' because it is an Exported object declared in the ObjectExport table");
    }

    public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        if (oTableWrapperObject.GetType() != typeof(ObjectImport))
            _shareManager.DeleteAllOrphanImportDefinitions();
    }
}