using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Newtonsoft.Json;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets
{
    public class HDRDatasetProvider : PluginDatasetProvider
    {
        private HttpClient _client;
        public HDRDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration) : base(activator, configuration)
        {
            _client = new HttpClient();
            var credentials = Repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
            var apiKey = credentials.GetDecryptedPassword();
            _client.DefaultRequestHeaders.Add("x-application-id", credentials.Username);
            _client.DefaultRequestHeaders.Add("x-client-id", apiKey);
        }
        public override void AddExistingDataset(string name, string url)
        {
            var response = Task.Run(async () => await _client.GetAsync(url)).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                HDRDataset hdrDataset = JsonConvert.DeserializeObject<HDRDataset>(detailsString);
                var datasetName = string.IsNullOrWhiteSpace(name) ? hdrDataset?.data.versions?.First().metadata.metadata.summary.title : name;
                var dataset = new Curation.Data.Datasets.Dataset(Repository, datasetName)
                {
                    Url = url,
                    Type = this.ToString(),
                    Provider_ID = Configuration.ID,
                    //DigitalObjectIdentifier = hdrDataset.data.versions.First().metadata.metadata.summary.doiName.ToString(),
                    Folder = $"\\{Configuration.Name}",
                };
                dataset.SaveToDatabase();
                Activator.Publish(dataset);
            }
            else
            {
                throw new Exception("Cannot access dataset at provided url");
            }
        }

        public override Dataset Create()
        {
            throw new NotImplementedException();
        }

        public HDRDataset FetchHDRDataset(Dataset dataset)
        {
            var response = Task.Run(async () => await _client.GetAsync(dataset.Url)).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                Console.WriteLine(detailsString);
                HDRDataset hdrDataset = JsonConvert.DeserializeObject<HDRDataset>(detailsString);
                return hdrDataset;
            }
            throw new Exception("Unable to fetch HDR dataset");
        }

        public override Dataset FetchDatasetByID(int id)
        {
            throw new NotImplementedException();
        }

        public override List<Dataset> FetchDatasets()
        {
            throw new NotImplementedException();
        }

        public override void Update(string uuid, PluginDataset datasetUpdates)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            var update = (HDRDataset)datasetUpdates;
            var updateObj = new HDRUpdateObject() {
                team_id = update.data.team_id,
                user_id = update.data.user_id,
                create_origin= update.data.create_origin,
                metadata = update.data.versions.First().metadata
            };
            System.Text.Json.JsonSerializer.Serialize<Data>(stream, ((HDRDataset)datasetUpdates).data, serializeOptions);
            var jsonString = Encoding.UTF8.GetString(stream.ToArray());
            var uri = $"{Configuration.Url}/v1/integrations/datasets/{uuid}";
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PatchAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Error");
            }
        }

        private class HDRUpdateObject
        {
            public int team_id { get; set; }
            public int user_id { get; set; }
            public string create_origin { get; set; }
            public  Metadata metadata { get; set; }
        }
    }

    
}
