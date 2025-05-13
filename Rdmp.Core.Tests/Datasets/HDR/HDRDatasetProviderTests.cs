using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using Rdmp.Core.Curation.Data.Datasets.HDR;
using JustEat.HttpClientInterception;

namespace Rdmp.Core.Tests.Datasets.HDR
{
    class HDRDatasetProviderTests : DatabaseTests
    {
        private DatasetProviderConfiguration _configuration;
        private HDRDatasetProvider _provider;
        private Catalogue _catalogue;
        private HttpClient _mockHttp;
        private HttpClientInterceptorOptions options = new HttpClientInterceptorOptions { ThrowOnMissingRegistration = true };

        [TearDown]
        public void OneTimeTeardown()
        {
            _mockHttp.Dispose();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var dataAccessCredentials = new DataAccessCredentials(CatalogueRepository, "name")
            {
                Username = "test",
                Password = "test"
            };
            dataAccessCredentials.SaveToDatabase();

            _configuration = new DatasetProviderConfiguration(CatalogueRepository, "Test Provider", typeof(HDRDatasetProvider).ToString(), "https://doesNotExist.preprod.hdruk.cloud/api", dataAccessCredentials.ID, "1234");

            _configuration.SaveToDatabase();

            _provider = new HDRDatasetProvider(new ThrowImmediatelyActivator(RepositoryLocator), _configuration, _mockHttp);
            _catalogue = new Catalogue(CatalogueRepository, "HDR test Catalogue");
            _catalogue.SaveToDatabase();

        }
        private void SetupHttpClient(HttpRequestInterceptionBuilder builder)
        {
            builder.RegisterWith(options);
            _mockHttp = options.CreateHttpClient();
            _provider = new HDRDatasetProvider(new ThrowImmediatelyActivator(RepositoryLocator), _configuration, _mockHttp);
        }

        [Test]
        public void CreateProviderTest()
        {
            Assert.DoesNotThrow(() => _configuration.GetProviderInstance(new ThrowImmediatelyActivator(RepositoryLocator)));
        }
        [Test]
        public void AddExistingDatasetTest_Success()
        {
            var url = "999";
            var builder = new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("doesnotexist.preprod.hdruk.cloud")
                .ForPath("api/v1/datasets/999")
                .ForQuery("schema_model=HDRUK&schema_version=3.0.0")
                .Responds()
                .WithStatus(200).WithJsonContent(SucessResponseString);
            SetupHttpClient(builder);
            string name = "AddExistingDatasetTest_Success";
            Assert.DoesNotThrow(() => _provider.AddExistingDataset(name, url));
            var dataset = CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Datasets.Dataset>().FirstOrDefault(d => d.Name == name);
            Assert.That(dataset, Is.Not.Null);
            dataset.DeleteInDatabase();
        }
        [Test]
        public void AddExistingDatasetTest_BadRemote()
        {
            var url = "999";
            var builder = new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("doesnotexist.preprod.hdruk.cloud")
                .ForPath("api/v1/datasets/999")
                .ForQuery("schema_model=HDRUK&schema_version=3.0.0")
                .Responds()
                .WithStatus(400);
            SetupHttpClient(builder);
            string name = "HDRAddExistingDatasetTest_BadRemote";
            Assert.Throws<Exception>(() => _provider.AddExistingDataset(name, url));
            var dataset = CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Datasets.Dataset>().FirstOrDefault(d => d.Name == name);
            Assert.That(dataset, Is.Null);
        }

        [Test]
        public void AddExistingDatasetWithReturnTest_Success()
        {
            var url = "999";
            var builder = new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("doesnotexist.preprod.hdruk.cloud")
                .ForPath("api/v1/datasets/999")
                .ForQuery("schema_model=HDRUK&schema_version=3.0.0")
                .Responds()
                .WithStatus(200).WithJsonContent(SucessResponseString);
            SetupHttpClient(builder);
            string name = "AddExistingDatasetTest_Success";
            Assert.DoesNotThrow(() => _provider.AddExistingDatasetWithReturn(name, url));
            var dataset = CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Datasets.Dataset>().FirstOrDefault(d => d.Name == name);
            Assert.That(dataset, Is.Not.Null);
            dataset.DeleteInDatabase();
        }
        [Test]
        public void AddExistingDatasetWithReturnTest_BadRemote()
        {
            var url = "999";
            var builder = new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("doesnotexist.preprod.hdruk.cloud")
                .ForPath("api/v1/datasets/999")
                .ForQuery("schema_model=HDRUK&schema_version=3.0.0")
                .Responds()
                .WithStatus(400);
            SetupHttpClient(builder);
            string name = "HDRAddExistingDatasetTest_BadRemote";
            Assert.Throws<Exception>(() => _provider.AddExistingDatasetWithReturn(name, url));
            var dataset = CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Datasets.Dataset>().FirstOrDefault(d => d.Name == name);
            Assert.That(dataset, Is.Null);
        }

        //FetchDatasetByID
        [Test]
        public void FetchDatasetByIDTest_Success()
        {
            var url = "999";
            var builder = new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
               .ForHost("doesnotexist.preprod.hdruk.cloud")
                .ForPath("api/v1/datasets/999")
                .ForQuery("schema_model=HDRUK&schema_version=3.0.0")
                .Responds()
                .WithStatus(200).WithJsonContent(SucessResponseString);
            SetupHttpClient(builder);
            Assert.DoesNotThrow(() => _provider.FetchDatasetByID(int.Parse(url)));
        }
        [Test]
        public void FetchDatasetByIDTest_BadRemote()
        {
            var url = "999";
            var builder = new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
              .ForHost("doesnotexist.preprod.hdruk.cloud")
                .ForPath("api/v1/datasets/999")
                .ForQuery("schema_model=HDRUK&schema_version=3.0.0")
                .Responds()
                .WithStatus(400);
            SetupHttpClient(builder);
            Assert.Throws<Exception>(() => _provider.FetchDatasetByID(int.Parse(url)));
        }

        private object SucessResponseString = new
        {
            id = "22",
            name = "AddExistingDatasetWithReturnTest_Success",
            type = "Dataset",
            url = "22",
            source = "Jira",
            _links = new
            {
                self = "https://api.atlassian.com/jsm/assets/workspace/1234/v1/object/22",
            }
        };

        private object SucessCreateString = new
        {

        };

        private List<object> SucessObjectTypesString = [new { name = "Dataset", id = 1, objectSchemaId = 4 }];

        private List<object> SuccessObjectTypeAttributes = [
            new {name="Name",id=2 }
            ];

        private object SucessAQLResult = new
        {
            values = new List<object>()
        };
    }
}
