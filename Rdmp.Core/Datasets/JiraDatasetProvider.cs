using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Azure;
using CommandLine.Text;
using Newtonsoft.Json;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Datasets.JiraItems;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Terminal.Gui;
using static Rdmp.Core.Datasets.JiraItems.JiraAPIObjects;

namespace Rdmp.Core.Datasets;

public class JiraDatasetProvider : PluginDatasetProvider
{
    private readonly string _workspace;
    private readonly HttpClient _client;
    private readonly string API_URL = "https://api.atlassian.com/jsm/assets/workspace/";
    private readonly String DATASET = "Dataset";
    private readonly String PROJECT = "Project";
    private readonly String NAME = "Name";

    public JiraDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration, HttpClient httpClient=null) : base(activator, configuration)
    {
        _client = httpClient??new HttpClient();
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
            Folder = $"\\Jira\\{Configuration.Name}",
        };
        dataset.SaveToDatabase();
        Activator.Publish(dataset);
        return dataset;
    }

    private class CreateAtrObj
    {
        public string objectTypeId;
        public List<Attribute> Attributes;
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

        var response = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/objectschema/{Configuration.Url}/objecttypes")).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            List<ObjectType> objectSchemaTypes = JsonConvert.DeserializeObject<List<ObjectType>>(detailsString);
            var t = objectSchemaTypes.FirstOrDefault(e => e.name == DATASET);
            string objectTypeId = t.id;
            string nameAttributeId = "";
            var schema = GetSchemaAttributes(t.objectSchemaId);
            var item = schema.Where(s => s.name == NAME);
            if (item.Any())
            {
                nameAttributeId = item.First().id;
            }
            else
            {
                throw new Exception($"{response.StatusCode}: Unable to fetch Object Types");
            }
            var o = new CreateAtrObj()
            {
                objectTypeId = objectTypeId,
                Attributes = new List<Attribute>() { new Attribute() {
                objectTypeAttributeId=nameAttributeId,
                objectAttributeValues = new List<ObjectAttributeValue>(){
                    new(){value=catalogue.Name}
                }
                } }
            };


            System.Text.Json.JsonSerializer.Serialize(stream, o, serializeOptions);
            var jsonString = Encoding.UTF8.GetString(stream.ToArray());
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            response = Task.Run(async () => await _client.PostAsync(url, httpContent)).Result;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                JiraDataset jiraDataset = JsonConvert.DeserializeObject<JiraDataset>(detailsString);
                jiraDataset = FetchDatasetByID(int.Parse(jiraDataset.id)) as JiraDataset;
                UpdateUsingCatalogue(jiraDataset, catalogue);
                return jiraDataset;
            }
            else
            {
                throw new Exception($"{response.StatusCode}: Unable to create Dataset");
            }
        }
        else
        {
            throw new Exception($"{response.StatusCode}: Unable to fetch Object Types");
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
        throw new Exception($"{response.StatusCode}: Unable to fetch Dataset by ID {id}");
    }

    public override List<Dataset> FetchDatasets()
    {
        throw new NotImplementedException();
    }




    public override void Update(string uuid, PluginDataset datasetUpdates)
    {
        var ds = (JiraDataset)datasetUpdates;
        List<Attribute> jiraAttributes = [];
        foreach (var attribute in ds.attributes)
        {
            var obj = new Attribute();
            obj.objectTypeAttributeId = attribute.objectTypeAttributeId;
            obj.objectAttributeValues = new List<ObjectAttributeValue>();
            foreach (var v in attribute.objectAttributeValues)
            {
                obj.objectAttributeValues.Add(new ObjectAttributeValue()
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
        var o = new JiraAPIObjects.UpdateAttributes()
        {
            attributes = jiraAttributes
        };
        System.Text.Json.JsonSerializer.Serialize(stream, o, serializeOptions);
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = Task.Run(async () => await _client.PutAsync($"{API_URL}{_workspace}/v1/object/{uuid}", httpContent)).Result;
        if(response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"{response.StatusCode}: Unable to update Dataset");
        }
    }

    private static string GetObjectTypeAttributeID(JiraDataset dataset, string name)
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


    private List<ReferencedObject> _schemaObjects = [];

    private List<ReferencedObject> GetSchemaAttributes(string objectSchemaID)
    {

        if (_schemaObjects.Any()) return _schemaObjects;

        var otresponse = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/objectschema/{objectSchemaID}/objecttypes")).Result;
        if (otresponse.StatusCode == HttpStatusCode.OK)
        {
            var otdetailsString = Task.Run(async () => await otresponse.Content.ReadAsStringAsync()).Result;
            var _objectEntires = JsonConvert.DeserializeObject<List<Entry>>(otdetailsString);
            var o = _objectEntires.FirstOrDefault(o => o.name == DATASET);
            if (o is not null)
            {
                var id = o.id;
                var response = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/objecttype/{id}/attributes")).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                    _schemaObjects = JsonConvert.DeserializeObject<List<ReferencedObject>>(detailsString);
                    return _schemaObjects;
                }
                else
                {
                    throw new Exception($"{response.StatusCode}: Unable to fetch Object Attributes for Schema {objectSchemaID}");
                }
            }
            throw new Exception("Unable to find Dataset object");
        }
        else
        {
            throw new Exception($"{otresponse.StatusCode}: Unable to fetch Object Types");

        }



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
            JiraAPIObjects.AQLResult databases = JsonConvert.DeserializeObject<JiraAPIObjects.AQLResult>(detailsString);

            var dbUpdate = new Attribute();
            dbUpdate.objectId = dataset.id;
            dbUpdate.objectTypeAttributeId = databaseTableschema;
            dbUpdate.objectAttributeValues = databases.values.Select(d => new ObjectAttributeValue() { value = d.objectKey }).ToList();


            var dbs = new List<JiraAPIObjects.Value>();
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
            throw new Exception($"{response.StatusCode}: Unable to fetch Jira Database Object");
        }

        Update(dataset.id, updateDataset);

        //update projects
        var projectSpecificIDs = Activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", catalogue.ID).Where(eds => eds.Project_ID != null).Select(eds => eds.Project_ID);
        var projectSpecifics = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().Where(p => projectSpecificIDs.Contains(p.ID));
        var projectsUsedIn = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().Where(p => p.ExtractionConfigurations.Any(ec => ec.GetAllExtractableDataSets().Any(eds => eds.Catalogue_ID == catalogue.ID)));
        var linkedProjects = projectSpecifics.Concat(projectsUsedIn).ToList().Distinct();

        response = Task.Run(async () => await _client.GetAsync($"{API_URL}{_workspace}/v1/objectschema/{Configuration.Url}/objecttypes")).Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            List<ObjectType> objectSchemaTypes = JsonConvert.DeserializeObject<List<ObjectType>>(detailsString);
            var t = objectSchemaTypes.FirstOrDefault(e => e.name == PROJECT);



            foreach (var project in linkedProjects)
            {
                var catalogues = project.GetAllProjectCatalogues();//is this all of them?
                List<Dataset> datasets = catalogue.GetLinkedDatasets().Where(ds => ds.Provider_ID == Configuration.ID).ToList();



                jsonString = $"{{\r\n  \"qlQuery\": \"objectType = Project and \\\"Project ID\\\" startswith \\\"Project {project.ProjectNumber} \\\"\"\r\n}}";
                httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                response = Task.Run(async () => await _client.PostAsync($"{API_URL}{_workspace}/v1/object/aql", httpContent)).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                    JiraAPIObjects.AQLResult projects = JsonConvert.DeserializeObject<JiraAPIObjects.AQLResult>(detailsString);
                    var datasetID = projects.objectTypeAttributes.First(ota => ota.name == DATASET).id;
                    foreach (var jiraAssetProject in projects.values)
                    {
                        List<Attribute> jiraAttributes = [];
                        jiraAttributes.Add(new Attribute()
                        {
                            objectTypeAttributeId = datasetID,
                            objectAttributeValues = datasets.Select(ds => new ObjectAttributeValue() { value = ((JiraDataset)FetchDatasetByID(int.Parse(ds.Url.Split('/').Last()))).objectKey }).ToList()

                        });
                        serializeOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true,
                            IncludeFields = true
                        };
                        using var stream = new MemoryStream();
                        var o = new JiraAPIObjects.UpdateAttributes()
                        {
                            attributes = jiraAttributes
                        };
                        System.Text.Json.JsonSerializer.Serialize(stream, o, serializeOptions);
                        jsonString = Encoding.UTF8.GetString(stream.ToArray());
                        httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        response = Task.Run(async () => await _client.PutAsync($"{API_URL}{_workspace}/v1/object/{jiraAssetProject.id}", httpContent)).Result;
                        if(response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception($"{response.StatusCode}: Unable to Link Dataset to project {jiraAssetProject.id}");
                        }
                    }
                }
                else
                {
                    throw new Exception($"{response.StatusCode}: Unable to fetch Projects");
                }
            }
        }
        else
        {
            throw new Exception($"{response.StatusCode}: Unable to fetch Object types");
        }
    }
}
