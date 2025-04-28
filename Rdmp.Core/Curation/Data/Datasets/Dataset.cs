// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;

using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rdmp.Core.Curation.Data.Datasets;

/// <inheritdoc cref="IDataset"/>

public class Dataset : DatabaseEntity, IDataset, IHasFolder 
{
    private string _name;
    private string _digitalObjectIdentifier;
    private string _source;
    private string _folder = FolderHelper.Root;
    private string _type = null;
    private string _url = null;
    private int? _providerId = null;

    /// <inheritdoc/>
    [DoNotImportDescriptions]
    [UsefulProperty]
    public string Folder
    {
        get => _folder;
        set => SetField(ref _folder, FolderHelper.Adjust(value));
    }

    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    [Unique]
    public string DigitalObjectIdentifier
    {
        get => _digitalObjectIdentifier;
        set => SetField(ref _digitalObjectIdentifier, value);
    }

    public string Source
    {
        get => _source;
        set => SetField(ref _source, value);
    }

    public string Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }
    public string Url
    {
        get => _url;
        set => SetField(ref _url, value);
    }

    public int? Provider_ID
    {
        get => _providerId;
        set => SetField(ref _providerId, value);
    }
    public override string ToString() => Name;

    public string GetID()
    {
        return ID.ToString();
    }

    public List<Catalogue> GetLinkedCatalogues()
    {
        return CatalogueRepository.GetAllObjectsWhere<CatalogueDatasetLinkage>("Dataset_ID", this.ID).Select(l => l.Catalogue).Distinct().ToList();
    }

    public Dataset(ICatalogueRepository catalogueRepository, string name)
    {
        catalogueRepository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"Name", name },
            {"Folder", _folder }
        });
    }

    public Dataset() { }
    internal Dataset(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        Name = r["Name"].ToString();
        Folder = r["Folder"].ToString();
        if (r["DigitalObjectIdentifier"] != DBNull.Value)
            DigitalObjectIdentifier = r["DigitalObjectIdentifier"].ToString();
        if (r["Source"] != DBNull.Value)
            Source = r["Source"].ToString();
        if (r["Type"] != DBNull.Value)
            Type = r["Type"].ToString();
        if (r["Url"] != DBNull.Value)
            Url = r["Url"].ToString();
        if (r["Provider_ID"] != DBNull.Value)
            Provider_ID = int.Parse(r["Provider_ID"].ToString());
    }
}