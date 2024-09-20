using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
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

        public PureDatasetProvider(ICatalogueRepository repository,DatasetProviderConfiguration configuration) : base(repository, configuration)
        {
            _client = new HttpClient();
            var credentials = repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
            var apiKey = credentials.GetDecryptedPassword();
            _client.DefaultRequestHeaders.Add("api-key", apiKey);
            //FetchDatasets();
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
                    Type = this.ToString()
                };
                dataset.SaveToDatabase();
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

        public override Curation.Data.Datasets.Dataset FetchDatasetByID(int id)
        {
            return null;
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
