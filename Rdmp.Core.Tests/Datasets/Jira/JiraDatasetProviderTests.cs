using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using JustEat.HttpClientInterception;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X509;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Curation.Data.Datasets.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Tests.Common;
using static System.Net.WebRequestMethods;

namespace Rdmp.Core.Tests.Datasets.Jira;

class JiraDatasetProviderTests : DatabaseTests
{

    private DatasetProviderConfiguration _configuration;
    private JiraDatasetProvider _provider;
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

        _configuration = new DatasetProviderConfiguration(CatalogueRepository, "Test Provider", typeof(JiraDatasetProvider).ToString(), "4", dataAccessCredentials.ID, "1234");

        _configuration.SaveToDatabase();

        _provider = new JiraDatasetProvider(new ThrowImmediatelyActivator(RepositoryLocator), _configuration, _mockHttp);
        _catalogue = new Catalogue(CatalogueRepository, "Jira test Catalogue");
        _catalogue.SaveToDatabase();

    }

    private void SetupHttpClient(HttpRequestInterceptionBuilder builder)
    {
        builder.RegisterWith(options);
        _mockHttp = options.CreateHttpClient();
        _provider = new JiraDatasetProvider(new ThrowImmediatelyActivator(RepositoryLocator), _configuration, _mockHttp);
    }

    [Test]
    public void CreateProviderTest()
    {
        Assert.DoesNotThrow(() => _configuration.GetProviderInstance(new ThrowImmediatelyActivator(RepositoryLocator)));
    }

    //AddExistingDataset
    [Test]
    public void AddExistingDatasetTest_Success()
    {
        var url = "999";
        var builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForHttps()
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/object/{url}")
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
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/object/{url}")
            .Responds()
            .WithStatus(400);
        SetupHttpClient(builder);
        string name = "AddExistingDatasetTest_BadRemote";
        Assert.Throws<Exception>(() => _provider.AddExistingDataset(name, url));
        var dataset = CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Datasets.Dataset>().FirstOrDefault(d => d.Name == name);
        Assert.That(dataset, Is.Null);
    }
    //AddExistingDatasetWithReturn
    [Test]
    public void AddExistingDatasetWithReturnTest_Success()
    {
        var url = "999";
        var builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForHttps()
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/object/{url}")
            .Responds()
            .WithStatus(200).WithJsonContent(SucessResponseString);
        SetupHttpClient(builder);
        string name = "AddExistingDatasetWithReturnTest_Success";
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
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/object/{url}")
            .Responds()
            .WithStatus(400);
        SetupHttpClient(builder);
        string name = "AddExistingDatasetWithReturnTest_BadRemote";
        Assert.Throws<Exception>(() => _provider.AddExistingDatasetWithReturn(name, url));
        var dataset = CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Datasets.Dataset>().FirstOrDefault(d => d.Name == name);
        Assert.That(dataset, Is.Null);
    }

    [Test]
    public void CreateDataset_BadObjectType()
    {
        var builder = new HttpRequestInterceptionBuilder()
        .Requests()
     .ForHttps()
        .ForHost("api.atlassian.com/jsm/assets/workspace")
        .ForPath($"/1234/v1/objectschema/4/objecttypes")
        .Responds()
        .WithStatus(400);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForPost()
        .ForHttps()
           .ForHost("api.atlassian.com/jsm/assets/workspace")
           .ForPath($"/1234/v1/object/create")
           .Responds()
           .WithStatus(201).WithJsonContent(SucessCreateString);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
            .Requests().ForHttps()
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/objecttype/1/attributes")
            .Responds()
            .WithStatus(200).WithJsonContent(SuccessObjectTypeAttributes);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
           .Requests()
           .ForHttps()
           .ForHost("api.atlassian.com/jsm/assets/workspace")
           .ForPath($"/1234/v1/object/999")
           .Responds()
           .WithStatus(200).WithJsonContent(SucessResponseString);
        SetupHttpClient(builder);

        var catalogue = new Catalogue(CatalogueRepository, "CreateDataset_Success");
        catalogue.SaveToDatabase();
        var e = Assert.Throws<Exception>(() => _provider.Create(catalogue));
        Assert.That(e.Message, Is.EqualTo("BadRequest: Unable to fetch Object Types"));
    }

    [Test]
    public void CreateDataset_UnableToCreate()
    {
        var builder = new HttpRequestInterceptionBuilder()
        .Requests()
     .ForHttps()
        .ForHost("api.atlassian.com/jsm/assets/workspace")
        .ForPath($"/1234/v1/objectschema/4/objecttypes")
        .Responds()
        .WithStatus(200).WithJsonContent(SucessObjectTypesString);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForPost()
        .ForHttps()
           .ForHost("api.atlassian.com/jsm/assets/workspace")
           .ForPath($"/1234/v1/object/create")
           .Responds()
           .WithStatus(400);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
            .Requests().ForHttps()
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/objecttype/1/attributes")
            .Responds()
            .WithStatus(200).WithJsonContent(SuccessObjectTypeAttributes);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
           .Requests()
           .ForHttps()
           .ForHost("api.atlassian.com/jsm/assets/workspace")
           .ForPath($"/1234/v1/object/999")
           .Responds()
           .WithStatus(200).WithJsonContent(SucessResponseString);
        SetupHttpClient(builder);

        var catalogue = new Catalogue(CatalogueRepository, "CreateDataset_Success");
        catalogue.SaveToDatabase();
        var e = Assert.Throws<Exception>(() => _provider.Create(catalogue));
        Assert.That(e.Message, Is.EqualTo("BadRequest: Unable to create Dataset"));
    }

    [Test]
    public void CreateDataset_NoMatchingSchema()
    {
        var builder = new HttpRequestInterceptionBuilder()
               .Requests()
            .ForHttps()
               .ForHost("api.atlassian.com/jsm/assets/workspace")
               .ForPath($"/1234/v1/objectschema/4/objecttypes")
               .Responds()
               .WithStatus(200).WithJsonContent(SucessObjectTypesString);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForPost()
        .ForHttps()
           .ForHost("api.atlassian.com/jsm/assets/workspace")
           .ForPath($"/1234/v1/object/create")
           .Responds()
           .WithStatus(201).WithJsonContent(SucessCreateString);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
            .Requests().ForHttps()
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/objecttype/1/attributes")
            .Responds()
            .WithStatus(400);
        SetupHttpClient(builder);

        builder = new HttpRequestInterceptionBuilder()
           .Requests()
           .ForHttps()
           .ForHost("api.atlassian.com/jsm/assets/workspace")
           .ForPath($"/1234/v1/object/999")
           .Responds()
           .WithStatus(200).WithJsonContent(SucessResponseString);
        SetupHttpClient(builder);

        var catalogue = new Catalogue(CatalogueRepository, "CreateDataset_Success");
        catalogue.SaveToDatabase();
        var e = Assert.Throws<Exception>(() => _provider.Create(catalogue));
        Assert.That(e.Message, Is.EqualTo("BadRequest: Unable to fetch Object Attributes for Schema 4"));
    }
    //FetchDatasetByID
    [Test]
    public void FetchDatasetByIDTest_Success()
    {
        var url = "999";
        var builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForHttps()
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/object/{url}")
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
            .ForHost("api.atlassian.com/jsm/assets/workspace")
            .ForPath($"/1234/v1/object/{url}")
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



