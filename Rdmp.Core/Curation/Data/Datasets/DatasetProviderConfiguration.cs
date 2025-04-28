using MongoDB.Driver;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets;

/// <inheritdoc/>
public class DatasetProviderConfiguration : DatabaseEntity, IDatasetProviderConfiguration
{

    private string _type;
    private string _name;
    private string _url;
    private int _dataAccessCredentials;
    private string _organisationId;

    public DatasetProviderConfiguration() { }

    public DatasetProviderConfiguration(ICatalogueRepository repository, string name, string type, string url, int dataAccessCredentialsID, string organisationId)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"Name",name },
            {"Type",type},
            {"Url",url},
            {"DataAccessCredentials_ID",dataAccessCredentialsID },
            {"Organisation_ID", organisationId }
        });
    }

    internal DatasetProviderConfiguration(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Name = r["Name"].ToString();
        Type = r["Type"].ToString();
        Url = r["Url"].ToString();
        DataAccessCredentials_ID = int.Parse(r["DataAccessCredentials_ID"].ToString());
        Organisation_ID = r["Organisation_ID"].ToString();
    }


    public IDatasetProvider GetProviderInstance(IBasicActivateItems activator)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == Type);
            if (type != null)
            {
                return (IDatasetProvider)Activator.CreateInstance(type, activator, this);
            }
        }
        return null;
    }

    public override string ToString()
    {
        return _name;
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
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

    public int DataAccessCredentials_ID
    {
        get => _dataAccessCredentials;
        set => SetField(ref _dataAccessCredentials, value);
    }

    public string Organisation_ID
    {
        get => _organisationId;
        set => SetField(ref _organisationId, value);
    }
}