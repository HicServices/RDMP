using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data;
public class Dataset : DatabaseEntity, IDataset
{
    string _name;
    string _digitalObjectIdentifier;
    string _source;

    [Unique]
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

    public override string ToString() => Name;


    public Dataset(ICatalogueRepository catalogueRepository, string name)
    {
        catalogueRepository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"Name", name }
        });
    }
    internal Dataset(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        Name = r["Name"].ToString();
        if (r["DigitalObjectIdentifier"] != DBNull.Value)
            DigitalObjectIdentifier = r["DigitalObjectIdentifier"].ToString();
        if (r["Source"] != DBNull.Value)
            Source = r["Source"].ToString();
    }
}