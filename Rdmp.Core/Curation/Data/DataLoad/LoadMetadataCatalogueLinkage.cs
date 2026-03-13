// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;

using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <inheritdoc cref="ILoadMetadataCatalogueLinkage"/>
public class LoadMetadataCatalogueLinkage : DatabaseEntity, ILoadMetadataCatalogueLinkage
{

    private int _LoadMetadataID;
    private int _CatalogueID;
    private string _name;

    [NotNull]
    public int LoadMetadataID
    {
        get => _LoadMetadataID;
        set => SetField(ref _LoadMetadataID, value);
    }

    [NotNull]
    public int CatalogueID
    {
        get => _CatalogueID;
        set => SetField(ref _CatalogueID, value);
    }

    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public LoadMetadataCatalogueLinkage() { }

    /// <summary>
    /// Creates a link between a catalogue and a data load 
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="loadMetadata"></param>
    /// <param name="catalogue"></param>
    /// <param name="name"></param>
    public LoadMetadataCatalogueLinkage(ICatalogueRepository repository, ILoadMetadata loadMetadata, ICatalogue catalogue,string name=null)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"LoadMetadataID",  loadMetadata.ID},
            {"CatalogueID",  catalogue.ID },
            {"Name",name??$"Loading {loadMetadata.Name}({loadMetadata.ID})" }
        });
    }

    internal LoadMetadataCatalogueLinkage(ICatalogueRepository repository, DbDataReader r)
   : base(repository, r)
    {
        LoadMetadataID = int.Parse(r["LoadMetadataID"].ToString());
        CatalogueID = int.Parse(r["CatalogueID"].ToString());
        Name = r["Name"].ToString();
    }

    internal LoadMetadataCatalogueLinkage(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
    }
}
