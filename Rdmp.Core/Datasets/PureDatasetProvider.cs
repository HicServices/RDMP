using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text;
using System.Data;
namespace Rdmp.Core.Datasets;

/// <summary>
/// Provides access to a remore PURE system to access datasets
/// </summary>
public class PureDatasetProvider : PluginDatasetProvider
{
    private HttpClient _client;

    public PureDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration) : base(activator, configuration)
    {
        _client = new HttpClient();
        var credentials = Repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
        var apiKey = credentials.GetDecryptedPassword();
        _client.DefaultRequestHeaders.Add("api-key", apiKey);
    }

    public static string UrltoUUID(string url) => url.Split("/").Last();

    public bool CheckDatasetExistsAtURL(string url)
    {
        var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(url)}";
        var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
        return response.StatusCode == HttpStatusCode.OK;
    }

    public override void AddExistingDataset(string name, string url)
    {
        AddExistingDatasetWithReturn(name, url);
    }

    public override void Update(string uuid, PluginDataset datasetUpdates)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var options = new JsonWriterOptions
        {
            Indented = true
        };

        using var stream = new MemoryStream();
        JsonSerializer.Serialize<PureDataset>(stream, (PureDataset)datasetUpdates, serializeOptions);
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());
        var uri = $"{Configuration.Url}/data-sets/{uuid}";
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Error");
        }
    }

    public PureDataset FetchPureDataset(Dataset dataset)
    {
        var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(dataset.Url)}";
        var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Error");
        }
        var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
        PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
        return pd;
    }


    public override Curation.Data.Datasets.Dataset FetchDatasetByID(int id)
    {
        var dataset = Repository.GetAllObjectsWhere<Curation.Data.Datasets.Dataset>("ID", id).FirstOrDefault();
        return dataset;
    }

    public override List<Curation.Data.Datasets.Dataset> FetchDatasets()
    {
        return Repository.GetAllObjectsWhere<Curation.Data.Datasets.Dataset>("Provider_ID", Configuration.ID).ToList();
    }


    //public List<PurePublisher> FetchPublishers()
    //{
    //    var uri = $"{Configuration.Url}/publishers";
    //    var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
    //    if (response.StatusCode != HttpStatusCode.OK)
    //    {
    //        throw new Exception("Error");
    //    }
    //    var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
    //    List<PurePublisher> publishers = JsonConvert.DeserializeObject<List<PurePublisher>>(detailsString);
    //    return publishers;

    //}

    public override Curation.Data.Datasets.Dataset Create(Catalogue catalogue)
    {
        throw new NotImplementedException();
    }

    public override Dataset AddExistingDatasetWithReturn(string name, string url)
    {
        var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(url)}";
        var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
            var datasetName = string.IsNullOrWhiteSpace(name) ? pd.Title.En_GB : name;
            var dataset = new Curation.Data.Datasets.Dataset(Repository, datasetName)
            {
                Url = url,
                Type = this.ToString(),
                Provider_ID = Configuration.ID,
                DigitalObjectIdentifier = pd.DigitalObjectIdentifier,
                Folder = $"\\{Configuration.Name}",
            };
            dataset.SaveToDatabase();
            Activator.Publish(dataset);
            return dataset;
        }
        else
        {
            throw new Exception("Cannot access dataset at provided url");
        }
    }

    public override void UpdateUsingCatalogue(PluginDataset dataset, Catalogue catalogue)
    {
        throw new NotImplementedException();
    }
}
