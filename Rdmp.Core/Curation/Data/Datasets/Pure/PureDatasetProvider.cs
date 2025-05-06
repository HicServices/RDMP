using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Dml.Diagram;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Datasets.Pure.PureDatasetItem;
using Rdmp.Core.ReusableLibraryCode.Progress;
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

namespace Rdmp.Core.Curation.Data.Datasets.Pure
{
    class PureDatasetProvider : PluginDatasetProvider
    {
        private HttpClient _client;
        private readonly string _url;
        public PureDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration, HttpClient client = null) : base(activator, configuration)
        {
            _client = new HttpClient();
            var credentials = Repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
            var apiKey = credentials.GetDecryptedPassword();
            _client.DefaultRequestHeaders.Add("api-key", apiKey);
            _url = Configuration.Url.TrimEnd('/');
        }
        public override void AddExistingDataset(string name, string url)
        {
            AddExistingDatasetWithReturn(name, url);
        }

        public override Dataset AddExistingDatasetWithReturn(string name, string url)
        {
            var uri = $"{_url}/data-sets/{UrltoUUID(url)}";
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
                    Folder = $"\\Pure\\{Configuration.Name}",
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

        public override Dataset FetchDatasetByID(int id)
        {
            var uri = $"{_url}/data-sets/{UrltoUUID(id.ToString())}";
            var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Dataset with ID '{id}' does cannot be found");
            }
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
            return pd;
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
            System.Text.Json.JsonSerializer.Serialize<PureDataset>(stream, (PureDataset)datasetUpdates, serializeOptions);
            var jsonString = Encoding.UTF8.GetString(stream.ToArray());
            var uri = $"{Configuration.Url}/data-sets/{uuid}";
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Error");
            }
        }

        public override void UpdateUsingCatalogue(Dataset dataset, Catalogue catalogue)
        {
            var existingDataset = (PureDataset)FetchDatasetByID(int.Parse(dataset.Url.Split("/").Last()));
            var datasetUpdate = new PureDataset();
            datasetUpdate.Title = new ENGBWrapper(catalogue.Name);
            var datasetDescriptionTerm = "/dk/atira/pure/dataset/descriptions/datasetdescription";
            var description = existingDataset.Descriptions.Where(d => d.Term.URI == datasetDescriptionTerm).FirstOrDefault();
            if (description is null)
            {
                //error
                Console.WriteLine("No known description!");
            }
            else
            {
                description.Value = new ENGBWrapper(catalogue.Description);
                datasetUpdate.Descriptions = new List<PureDescription> { description };
            }
            if (catalogue.StartDate != null)
            {
                datasetUpdate.TemporalCoveragePeriod.StartDate = new PureDate((DateTime)catalogue.StartDate);
            }
            if (catalogue.EndDate != null)
            {
                datasetUpdate.TemporalCoveragePeriod.EndDate = new PureDate((DateTime)catalogue.EndDate);
            }
            if (catalogue.Geographical_coverage != null)
            {
                datasetUpdate.Geolocation = new Geolocation();
                datasetUpdate.Geolocation.GeographicalCoverage = new ENGBWrapper(catalogue.Geographical_coverage);
                if (existingDataset.Geolocation != null)
                {
                    datasetUpdate.Geolocation.Point = existingDataset.Geolocation.Point;
                    datasetUpdate.Geolocation.Polygon = existingDataset.Geolocation.Polygon;
                }
            }
            Update(existingDataset.UUID, datasetUpdate);
        }

        public override Dataset Create(Catalogue catalogue)
        {
            var uri = $"{_url}/data-sets/";
            var newDataset = new PureDataset();
            newDataset.Title = new ENGBWrapper(catalogue.Name);

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
            System.Text.Json.JsonSerializer.Serialize<PureDataset>(stream, newDataset, serializeOptions);
            var jsonString = Encoding.UTF8.GetString(stream.ToArray());
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Error: {response.StatusCode}");
            }
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
            UpdateUsingCatalogue(pd, catalogue);
            return FetchDatasetByID(pd.PureId.Value);
        }

        public override string GetRemoteURL(Dataset dataset)
        {
            var existingDataset = (PureDataset)FetchDatasetByID(int.Parse(dataset.Url.Split("/").Last()));
            return existingDataset.PortalURL;
        }
        private static string UrltoUUID(string url) => url.Split("/").Last();
    }
}
