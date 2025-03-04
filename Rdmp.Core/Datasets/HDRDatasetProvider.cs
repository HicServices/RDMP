using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using System;
using System.Collections.Generic;
using System.Data;
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
        private DatasetProviderConfiguration _configuration;
        public HDRDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration) : base(activator, configuration)
        {
            _client = new HttpClient();
            var credentials = Repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
            var apiKey = credentials.GetDecryptedPassword();
            _client.DefaultRequestHeaders.Add("x-application-id", credentials.Username);
            _client.DefaultRequestHeaders.Add("x-client-id", apiKey);
            _configuration = configuration;
        }
        public override Dataset AddExistingDatasetWithReturn(string name, string url)
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
                return dataset;
            }
            else
            {
                throw new Exception("Cannot access dataset at provided url");
            }
        }

        public override void AddExistingDataset(string name, string url)
        {
            AddExistingDatasetWithReturn(name, url);
        }

        public override Dataset Create()
        {
            throw new NotImplementedException();
        }

        public HDRDataset FetchHDRDataset(Dataset dataset)
        {
            var response = Task.Run(async () => await _client.GetAsync($"{dataset.Url}?schema_model=HDRUK&schema_version=3.0.0")).Result;
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
            var url = Configuration.Url + "/v1/datasets/" + id;
            var response = Task.Run(async () => await _client.GetAsync($"{url}?schema_model=HDRUK&schema_version=3.0.0")).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                Console.WriteLine(detailsString);
                HDRDataset hdrDataset = JsonConvert.DeserializeObject<HDRDataset>(detailsString);
                return hdrDataset;
            }
            throw new Exception("Unable to fetch HDR dataset");
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
            };
            serializeOptions.Converters.Add(new CustomDateTimeConverter());
            serializeOptions.Converters.Add(new CustomDateTimeConverterThreeMilliseconds());

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            var update = (HDRDataset)datasetUpdates;
            var updateObj = new HDRUpdateObject()
            {
                team_id = update.data.team_id,
                user_id = update.data.user_id,
                create_origin = update.data.create_origin,
                metadata = (new HDRDatasetPatch((HDRDataset)datasetUpdates)).metadata.metadata
            };
            //update.data.versions.First().metadata.metadata


            //System.Text.Json.JsonSerializer.Serialize<PatchMetadata>(stream, (new HDRDatasetPatch((HDRDataset)datasetUpdates)).metadata, serializeOptions);
            System.Text.Json.JsonSerializer.Serialize<PatchSubMetadata>(stream, updateObj.metadata, serializeOptions);
            var jsonString = Encoding.UTF8.GetString(stream.ToArray());
            var uri = $"{Configuration.Url}/v1/integrations/datasets/{uuid}?schema_model=HDRUK&schema_version=3.0.0";
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var x = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                throw new Exception(x);
            }
        }

        private class HDRUpdateObject
        {
            public int team_id { get; set; }
            public int user_id { get; set; }
            public string create_origin { get; set; }
            public  PatchSubMetadata metadata { get; set; }
        }
    }

    
}
