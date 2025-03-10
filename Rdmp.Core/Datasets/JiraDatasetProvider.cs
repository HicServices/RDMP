using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using CommandLine.Text;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.XWPF.UserModel;
using Org.BouncyCastle.Utilities;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Collections.Generic;
using System.Data;
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
        var code = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{credentials.Username}:{apiKey}"));
        _client.DefaultRequestHeaders.Add("Authorization", $"Basic {code}");

        _client.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        _workspace = configuration.Organisation_ID;
    }

    public override void AddExistingDataset(string name, string url)
    {
        AddExistingDatasetWithReturn(name, url);
    }

    public override Dataset AddExistingDatasetWithReturn(string name, string url)
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
        return dataset;
    }

    private class CreateAtrObj
    {
        public string objectTypeId;
        public List<JiraPutAttribute> Attributes;
    }

    public override Dataset Create(Catalogue catalogue)
    {
        var url = $"{API_URL}{_workspace}/v1/object/create";
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };
        using var stream = new MemoryStream();


        //var schemaResponse = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/objectschema/list")).Result;



        string objectTypeId = "1";
        string nameAttributeId = "2";
        //if (schemaResponse.StatusCode == HttpStatusCode.OK)
        //{
        //    var x = new SchemaObject();
        //    x.id = "1";
        //    x.name = "error";
        //    _ = new SchemaList()
        //    {
        //        values = []
        //    };

        //    var detailsString = Task.Run(async () => await schemaResponse.Content.ReadAsStringAsync()).Result;
        //    SchemaList schemas = JsonConvert.DeserializeObject<SchemaList>(detailsString);
        //    var s = schemas.values.First(s => s.name == "TEST").id;//todo should be HIC
        //    //objectTypeId = schemas.values.First(s => s.name == "Dataset").id;
        //    //nameAttributeId = GetSchemaAttributes(objectTypeId).First(s => s.name == "Name").id;
        //}
        //else
        //{
        //    throw new Exception("Unable to fetch schemas");
        //}

        var o = new CreateAtrObj()
        {
            objectTypeId = objectTypeId,
            Attributes = new List<JiraPutAttribute>() { new JiraPutAttribute() {
                objectTypeAttributeId=nameAttributeId,
                objectAttributeValues = new List<JiraPutAttributeValueObject>(){
                    new JiraPutAttributeValueObject(){value=catalogue.Name}
                }
            } }
        };


        System.Text.Json.JsonSerializer.Serialize(stream, o, serializeOptions);
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = Task.Run(async () => await _client.PostAsync(url, httpContent)).Result;
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            JiraDataset jiraDataset = JsonConvert.DeserializeObject<JiraDataset>(detailsString);
            jiraDataset = FetchDatasetByID(int.Parse(jiraDataset.id)) as JiraDataset;
            UpdateUsingCatalogue(jiraDataset, catalogue);
            return jiraDataset;
        }
        else
        {
            throw new Exception("Unable to create");
        }
    }

    public override Dataset FetchDatasetByID(int id)
    {
        var response = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/object/{id}")).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            JiraDataset jiraDataset = JsonConvert.DeserializeObject<JiraDataset>(detailsString);

            return jiraDataset;
        }
        throw new Exception("Unable to fetch object by ID");
    }

    public override List<Dataset> FetchDatasets()
    {
        throw new NotImplementedException();
    }

    private class AtrObj
    {
        public List<JiraPutAttribute> Attributes;
    }

    public override void Update(string uuid, PluginDataset datasetUpdates)
    {
        var ds = (JiraDataset)datasetUpdates;
        List<JiraPutAttribute> jiraAttributes = [];
        foreach (var attribute in ds.attributes)
        {
            var obj = new JiraPutAttribute();
            obj.objectTypeAttributeId = attribute.objectTypeAttributeId;
            obj.objectAttributeValues = new List<JiraPutAttributeValueObject>();
            foreach (var v in attribute.objectAttributeValues)
            {
                obj.objectAttributeValues.Add(new JiraPutAttributeValueObject()
                {
                    value = v.value
                });
            }
            jiraAttributes.Add(obj);
        }
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };
        using var stream = new MemoryStream();
        var o = new AtrObj()
        {
            Attributes = jiraAttributes
        };
        System.Text.Json.JsonSerializer.Serialize(stream, o, serializeOptions);
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = Task.Run(async () => await _client.PutAsync($"{API_URL}{_workspace}/v1/object/{uuid}", httpContent)).Result;
        Console.WriteLine(response);
    }

    private class SchemaObject
    {
        public string id;
        public string name;
    }
    private string GetObjectTypeAttributeID(JiraDataset dataset, string name)
    {
        try
        {
            var a = dataset.attributes.Select(a => a.objectTypeAttribute);
            var b = a.Select(a => a.name);
            var c = a.Where(l => l.name == name).ToList();
            if (c.Any()) return c.First().id;
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }


    private List<SchemaObject> GetSchemaAttributes(string objectSchemaID)
    {
        var response = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/objectschema/{objectSchemaID}/attributes")).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var x = new SchemaObject();
            x.id = "1";
            x.name = "error";
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            List<SchemaObject> schema = JsonConvert.DeserializeObject<List<SchemaObject>>(detailsString);

            return schema;
        }
        throw new Exception("Unable to fetch object schema");

    }

    private Attribute GenerateUpdateAttribute(JiraDataset dataset, Catalogue catalogue, string name, string value)
    {
        var otai = GetObjectTypeAttributeID(dataset, name);
        if (otai is null)
        {
            var schema = GetSchemaAttributes(dataset.objectType.objectSchemaId);
            var item = schema.Where(s => s.name == name);
            if (item.Any())
            {
                otai = item.First().id;
            }
        }
        return new Attribute()
        {
            objectTypeAttributeId = otai,
            objectAttributeValues = new List<ObjectAttributeValue>() {
                new ObjectAttributeValue()
                {
                    value = value
                }
            }
        };
    }



    private class DatabaseValue
    {
        public string value;

    }

    private class Database
    {
        public string id;
        public string objectKey;
        public string label;
        public List<Attribute> attributes;
    }

    private class Databases
    {
        public List<Database> values;
    }

    private class DatabasePUT
    {
        public string objectId;
        public string objectTypeAttributeId;
        public List<DatabaseValue> objectAttributeValues;
    }

    public override void UpdateUsingCatalogue(JiraDataset dataset, Catalogue catalogue)
    {
        var updateDataset = new JiraDataset();
        updateDataset.attributes = new List<Attribute>();

        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Name", catalogue.Name));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Short Description", catalogue.ShortDescription));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Acronym", catalogue.Acronym));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "DOI", catalogue.Doi));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Update Frequency", ((Catalogue.UpdateFrequencies)catalogue.Update_freq).ToString()));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Initial Release Date", catalogue.DatasetReleaseDate.ToString()));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Update Lag", ((Catalogue.UpdateLagTimes)catalogue.UpdateLag).ToString()));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Is Deprecated", catalogue.IsDeprecated.ToString()));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "Is Project Specific", catalogue.IsProjectSpecific(Activator.RepositoryLocator.DataExportRepository).ToString()));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "RDMP_CatalogueID", catalogue.ID.ToString()));
        updateDataset.attributes.Add(GenerateUpdateAttribute(dataset, catalogue, "RDMP_CatalogueDB", (catalogue.CatalogueRepository as TableRepository).GetConnection().Connection.ConnectionString));
        var tableInfos = catalogue.CatalogueItems.Select(ci => ci.ColumnInfo.TableInfo).ToList();
        var databaseTableschema = GetSchemaAttributes(dataset.objectType.objectSchemaId).Where(s => s.name == "Database").First().id;

        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };

        var jsonString = "{\r\n  \"qlQuery\": \"objectType = Database\"\r\n}";
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var response = Task.Run(async () => await _client.PostAsync($"{API_URL}{_workspace}/v1/object/aql", httpContent)).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {

            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            Databases databases = JsonConvert.DeserializeObject<Databases>(detailsString);

            var dbUpdate = new DatabasePUT();
            dbUpdate.objectId = dataset.id;
            dbUpdate.objectTypeAttributeId = databaseTableschema;
            dbUpdate.objectAttributeValues = databases.values.Select(d => new DatabaseValue() { value = d.objectKey }).ToList();


            _ = new Database()
            {
                id = "1",
                objectKey = "1",
                label = "1",
                attributes = []
            };
            _ = new Databases()
            {
                values = []
            };
            _ = new DatabaseValue()
            {
                value = ""
            };


            var dbs = new List<Database>();
            foreach (var ti in tableInfos)
            {
                var o = databases.values.FirstOrDefault(db => db.attributes.Any(a => a.objectAttributeValues.First().value.ToString() == ti.Database[1..^1]) && db.attributes.Any(a => a.objectAttributeValues.First().value.ToString() == ti.Server));
                if (o is not null)
                {
                    dbs.Add(o);
                }
            }
            dbs = dbs.Distinct().ToList();


            using var stream = new MemoryStream();
            System.Text.Json.JsonSerializer.Serialize(stream, dbUpdate, serializeOptions);
            var dbUpdatejson = Encoding.UTF8.GetString(stream.ToArray());

            updateDataset.attributes.Add(new Attribute()
            {
                objectTypeAttributeId = databaseTableschema,
                objectAttributeValues = dbs.Select(v => new ObjectAttributeValue() { value = v.objectKey }).ToList()
            });
        }
        else
        {
            throw new Exception("Unable to fetch object schema");
        }

        Update(dataset.id, updateDataset);

        //update projects
        var projectSpecificIDs = Activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", catalogue.ID).Where(eds => eds.Project_ID != null).Select(eds => eds.Project_ID);
        var projectSpecifics = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().Where(p => projectSpecificIDs.Contains(p.ID));
        var projectsUsedIn = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().Where(p => p.ExtractionConfigurations.Any(ec => ec.GetAllExtractableDataSets().Any(eds => eds.Catalogue_ID == catalogue.ID)));
        var linkedProjects = projectSpecifics.Concat(projectsUsedIn).ToList().Distinct();
        foreach (var project in linkedProjects)
        {
            //find it in jira
            // link this dataset to that poroject

            //todo validate projects with RDMP IDs
        }
    }
}
