using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using CommandLine.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;

public class JiraDatasetProvider : PluginDatasetProvider
{
    private readonly string _workspace;
    private readonly HttpClient _client;

    private readonly string API_URL = "https://api.atlassian.com/jsm/assets/workspace/";


    public JiraDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration) : base(activator, configuration)
    {
        _client = new HttpClient();
        var credentials = Repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
        var apiKey = credentials.GetDecryptedPassword();
        var code =System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{credentials.Username}:{apiKey}"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", code);
        _workspace = configuration.Organisation_ID;
    }

    public override void AddExistingDataset(string name, string url)
    {
        AddExistingDatasetWithReturn(name, url);
    }

    public JiraDataset AddExistingDatasetWithReturn(string name, string url)
    {
        JiraDataset jiraDataset = (JiraDataset)FetchDatasetByID(int.Parse(url));
        var dataset = new Curation.Data.Datasets.Dataset(Repository, jiraDataset.name)
        {
            Url = jiraDataset._links.self,
            Type = this.ToString(),
            Provider_ID = Configuration.ID,
            Folder = $"\\{Configuration.Name}",
        };
        dataset.SaveToDatabase();
        Activator.Publish(dataset);
        return (JiraDataset)dataset;
    }

    public override Dataset Create()
    {
        throw new NotImplementedException();
    }

    public override Dataset FetchDatasetByID(int id)
    {
        var response = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/object/{id}")).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            JiraDataset jiraDataset= JsonConvert.DeserializeObject<JiraDataset>(detailsString);

            return jiraDataset;
        }
        throw new Exception("Unable to fetch object by ID");
    }

    public override List<Dataset> FetchDatasets()
    {
        throw new NotImplementedException();
    }

    public override void Update(string uuid, PluginDataset datasetUpdates)
    {
        var ds = (JiraDataset)datasetUpdates;
        List<JiraPutAttribute> attributes = [];
        foreach(var attribute in ds.attributes)
        {
            var obj = new JiraPutAttribute();
            obj.objectTypeAttributeId = attribute.objectTypeAttributeId;
            obj.objectAttributeValues = new List<JiraPutAttributeValueObject>();
            foreach(var v in attribute.objectAttributeValues)
            {
                obj.objectAttributeValues.Add(new JiraPutAttributeValueObject() { 
                    value =v.value
                });
            }
        }
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        using var stream = new MemoryStream();
        System.Text.Json.JsonSerializer.Serialize<List<JiraPutAttribute>>(stream, attributes, serializeOptions);
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = Task.Run(async () => await _client.PutAsync($"{API_URL}{_workspace}/v1/object/{uuid}", httpContent)).Result;

    }
}
