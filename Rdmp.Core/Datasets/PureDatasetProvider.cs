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
namespace Rdmp.Core.Datasets
{
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

        private bool CheckDatasetExistsAtURL(string url)
        {
            var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(url)}";
            var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            return response.StatusCode == HttpStatusCode.OK;
        }

        public override void AddExistingDataset(string name, string url)
        {
            var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(url)}";
            var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
                var dataset = new Curation.Data.Datasets.Dataset(Repository, name)
                {
                    Url = url,
                    Type = this.ToString(),
                    Provider_ID = Configuration.ID,
                    DigitalObjectIdentifier = pd.DigitalObjectIdentifier,
                    Folder= Configuration.Name,
                };
                dataset.SaveToDatabase();
                Activator.Publish(dataset);
            }
            else
            {
                throw new Exception("Cannot access dataset at provided url");
            }
        }

        public Curation.Data.Datasets.Dataset Create(string title, string publisherUid, List<PurePerson> people, Visibility visibility, PureDate publicationDate)
        {
            //this works but i'm not sure it's linking up with the person correctly

            var pureDataset = new PureDataset();
            pureDataset.Title = new ENGBWrapper(title);
            pureDataset.ManagingOrganization = new System(Configuration.Organisation_ID, "Organization");
            pureDataset.Type = new URITerm("/dk/atira/pure/dataset/datasettypes/dataset/dataset", new ENGBWrapper("Dataset"));
            pureDataset.Publisher = new System(publisherUid, "Publisher");

            pureDataset.Persons = people;
            pureDataset.Visibility = visibility;
            pureDataset.Organizations = [new System(Configuration.Organisation_ID, "Organization")];
            pureDataset.PublicationAvailableDate = publicationDate;

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonString = JsonSerializer.Serialize(pureDataset, serializeOptions);

            var uri = $"{Configuration.Url}/data-sets";
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception("Error");
            }
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
            var dataset = new Curation.Data.Datasets.Dataset(Repository, pureDataset.Title.en_GB);
            dataset.Provider_ID = Configuration.ID;
            dataset.Url = pd.PortalURL;
            dataset.SaveToDatabase();
            return dataset;
        }

        public override void Update(string uuid, PluginDataset datasetUpdates)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonString = JsonSerializer.Serialize(datasetUpdates, serializeOptions);
            var uri = $"{Configuration.Url}/data-sets/{uuid}";
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception("Error");
            }
        }

        public PureDataset FetchPureDataset(Curation.Data.Datasets.Dataset dataset)
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

        public override Curation.Data.Datasets.Dataset Create()
        {
            throw new NotImplementedException();
        }
    }
}
