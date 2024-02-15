using Amazon.Auth.AccessControlPolicy;
using MongoDB.Driver;
using Org.BouncyCastle.Asn1.X509;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.DataLoad;

public class LoadMetatadataCatalogueLinkage : DatabaseEntity, ILoadMetadataCatalogueLinkage
{

    private int _LoadMetadataID;
    private int _CatalogueID;

    public LoadMetatadataCatalogueLinkage(CatalogueRepository repository, LoadMetadata loadMetadata, Catalogue catalogue)
    {
        LoadMetatdataID = loadMetadata.ID;
        CatalogueID = catalogue.ID;
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"LoadMetadataID", LoadMetatdataID},
            {"CatalogueID", CatalogueID }
        });
    }

    internal LoadMetatadataCatalogueLinkage(ICatalogueRepository repository, DbDataReader r)
   : base(repository, r)
    {
        LoadMetatdataID = int.Parse(r["LoadMetatdataID"].ToString());
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
