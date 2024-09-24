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
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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

        private string UrltoUUID(string url) => url.Split("/").Last();

        private bool CheckDatasetExistsAtURL(string url)
        {
            var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(url)}";
            var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public override void AddExistingDataset(string name, string url)
        {
            if (CheckDatasetExistsAtURL(url))
            {
                var dataset = new Curation.Data.Datasets.Dataset(Repository, name)
                {
                    Url = url,
                    Type = this.ToString(),
                    Provider_ID = Configuration.ID
                };
                dataset.SaveToDatabase();
                Activator.Publish(dataset);
            }
            else
            {
                throw new Exception("Cannot access dataset at provided url");
            }
        }

        public override Curation.Data.Datasets.Dataset Create()
        {
            throw new NotImplementedException();
        }

        private PureDataset FetchPureDataset(Curation.Data.Datasets.Dataset dataset)
        {
            var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(dataset.Url)}";
            var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
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
            //if (dataset is null)
            //{
            //    throw new Exception("Unable to find local dataset with ID");
            //}
            //if (dataset.Type != this.ToString())
            //{
            //    var x = this.ToString();
            //    throw new Exception("Dataset is of the wrong type");
            //}
            //if (dataset.Provider_ID is null)
            //{
            //    throw new Exception("No provider specified");
            //}
            //var provider = Repository.GetAllObjectsWhere<DatasetProviderConfiguration>("ID", dataset.Provider_ID).FirstOrDefault();
            //if (provider is null)
            //{
            //    throw new Exception("Unable to locate provider");
            //}
            //var uri = $"{Configuration.Url}/data-sets/{UrltoUUID(dataset.Url)}";
            //var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            //if (response.StatusCode != System.Net.HttpStatusCode.OK)
            //{
            //    throw new Exception("Error");
            //}
            //var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            //PureDataset pd = JsonConvert.DeserializeObject<PureDataset>(detailsString);
            return dataset;
        }

        public override List<Curation.Data.Datasets.Dataset> FetchDatasets()
        {
            ////TODO will have to think about paginations
            //var uri = $"{Configuration.Url}/data-sets";
            //var response = Task.Run(async () => await _client.GetAsync(uri)).Result;
            //if(response.StatusCode != System.Net.HttpStatusCode.OK)
            //{
            //    throw new Exception("Error");
            //}
            //var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            //var jsonObjects = JsonObject.Parse(detailsString);
            //foreach(var item in jsonObjects.AsObject()) {

            //    if (item.Key == "items")
            //    {
            //        Console.WriteLine(item.ToString());
            //        //the values here are the datasets
            //    }
            //}
            return new List<Curation.Data.Datasets.Dataset>();

        }

        public override Curation.Data.Datasets.Dataset Update()
        {
            throw new NotImplementedException();
        }
    }
}
