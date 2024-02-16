
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Rdmp.Core.Curation.Data.DataLoad;

public class LoadMetadataCatalogueLinkage : DatabaseEntity, ILoadMetadataCatalogueLinkage
{

    private int _LoadMetadataID;
    private int _CatalogueID;

    public LoadMetadataCatalogueLinkage(ICatalogueRepository repository, ILoadMetadata loadMetadata, ICatalogue catalogue)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"LoadMetadataID",  loadMetadata.ID},
            {"CatalogueID",  catalogue.ID }
        });
    }

    internal LoadMetadataCatalogueLinkage(ICatalogueRepository repository, DbDataReader r)
   : base(repository, r)
    {
        LoadMetatdataID = int.Parse(r["LoadMetadataID"].ToString());
        CatalogueID = int.Parse(r["CatalogueID"].ToString());
    }

    [NotNull]
    public int LoadMetatdataID
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

}
