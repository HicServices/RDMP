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
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.ImportExport;

/// <summary>
/// Identifies an object in the local Catalogue database (or DataExport database) which was imported from an external catalogue (See ObjectExport).  The SharingUID
/// allows you to always identify which local object represents a remoted shared object (e.g. available from a web service).  The remote object will have a different
///  ID but the same SharingUID).  Sometimes you will import whole networks of objects which might have shared object dependencies in this case newly imported
/// networks will reference existing imported objects where they are already available.
/// 
/// <para>This table exists to avoid all the unmaintainability/scalability of IDENTITY INSERT whilst also ensuring referential integrity of object shares and preventing
/// duplication of imported objects.</para>
/// </summary>
public class ObjectImport : ReferenceOtherObjectDatabaseEntity
{
    #region Database Properties

    private string _sharingUID;

    #endregion

    /// <summary>
    /// The globally unique identifier for referring to the shared object.  This allows the object to be updated later / new versions to be distributed
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

    public ObjectImport()
    {
    }

    /// <summary>
    /// Use GetImportAs to access this
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="sharingUID"></param>
    /// <param name="localObject"></param>
    internal ObjectImport(ICatalogueRepository repository, string sharingUID, IMapsDirectlyToDatabaseTable localObject)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectRepositoryType", localObject.Repository.GetType().Name },
            { "ReferencedObjectType", localObject.GetType().Name },
            { "ReferencedObjectID", localObject.ID },
            { "SharingUID", sharingUID }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    /// <inheritdoc/>
    public ObjectImport(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        SharingUID = r["SharingUID"].ToString();
    }

    /// <inheritdoc/>
    public override string ToString() => $"I::{ReferencedObjectType}::{SharingUID}";
}