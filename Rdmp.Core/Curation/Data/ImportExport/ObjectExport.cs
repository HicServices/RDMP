// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.ImportExport;

/// <summary>
/// Identifies an object in the local Catalogue database (or DataExport database) which has been shared externally (via its SharingUID).  The use of a SharingUID
/// allows multiple external users to access and import the shared object (and any dependant objects).  Having an ObjectExport declared on an object prevents it from
/// being deleted (see ObjectSharingObscureDependencyFinder) since this would leave external users with orphaned objects.
/// </summary>
public class ObjectExport : ReferenceOtherObjectDatabaseEntity, IInjectKnown<IMapsDirectlyToDatabaseTable>
{
    #region Database Properties

    private string _sharingUID;

    #endregion

    /// <summary>
    /// The globally unique identifier for refering to the shared object.  This allows the object to be updated later / new versions to be distributed
    /// even though the ID is different (e.g. it has been imported into another instance of RDMP).
    /// </summary>
    public string SharingUID
    {
        get => _sharingUID;
        set => SetField(ref _sharingUID, value);
    }

    /// <inheritdoc cref="SharingUID"/>
    [NoMappingToDatabase]
    public Guid SharingUIDAsGuid => Guid.Parse(SharingUID);

    public ObjectExport()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// use <see cref="ShareManager.GetNewOrExistingExportFor"/> for easier access to this constructor
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="objectForSharing"></param>
    /// <param name="guid"></param>
    internal ObjectExport(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectForSharing, Guid guid)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", objectForSharing.ID },
            { "ReferencedObjectType", objectForSharing.GetType().Name },
            { "ReferencedObjectRepositoryType", objectForSharing.Repository.GetType().Name },
            { "SharingUID", guid.ToString() }
        });

        if (ID == 0 ||  !repository.Equals(Repository))
            throw new ArgumentException("Repository failed to properly hydrate this class");

        ClearAllInjections();
    }

    /// <inheritdoc/>
    public ObjectExport(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        SharingUID = r["SharingUID"].ToString();
    }

    public void InjectKnown(IMapsDirectlyToDatabaseTable instance)
    {
        _knownReferenceTo = new Lazy<IMapsDirectlyToDatabaseTable>(instance);
    }

    /// <inheritdoc/>
    public override string ToString() => _knownReferenceTo != null
        ? $"E::{_knownReferenceTo.Value}"
        : $"E::{ReferencedObjectType}::{SharingUID}";

    private Lazy<IMapsDirectlyToDatabaseTable> _knownReferenceTo;

    public void ClearAllInjections()
    {
        _knownReferenceTo = null;
    }
}