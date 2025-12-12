using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.SecurityToken.Model;
using Azure;
using MathNet.Numerics;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt;
using NPOI.SS.UserModel.Charts;
using NPOI.XWPF.UserModel;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Dataset.Confluence;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Uses provided HDR Gateway credentials to populate the HDR gateway
    /// </summary>
    public class ExecuteCommandExportCataloguesToHDRGateway : BasicCommandExecution, IAtomicCommand
    {

        private readonly IBasicActivateItems _activator;
        private readonly string _url;
        private readonly int _team;
        private readonly string _appID;
        private readonly string _clientID;
        private readonly HttpClient _client = new();
        private readonly string _datasetsEndpoint = "/v2/datasets";

        public class HDRDatasetDataCustodian
        {
            public HDRDatasetDataCustodian() { }
            public HDRDatasetDataCustodian(Catalogue catalogue)
            {
                //todo check this
                name = catalogue.DataController;
                contactPoint = catalogue.Contact_details;
            }
            public string name { get; set; }//this needs to be populated
            public int identifier { get; set; } = 0;
            public string contactPoint { get; set; }//this needs to be populated
            public object logo { get; set; }
            public object description { get; set; }
            public object memberOf { get; set; }
        }

        public class HDRDatasetDocumentation
        {
            public HDRDatasetDocumentation() { }
            public HDRDatasetDocumentation(Catalogue catalogue)
            {
                description = catalogue.Description;
            }
            public string description { get; set; }//this needs to be populated;
        }
        private class HDRDatasetMetadataSummary
        {
            public HDRDatasetMetadataSummary(HDRDatasetObject datasetObject)
            {
                //todo
            }
            public HDRDatasetMetadataSummary(Catalogue catalogue)
            {
                title = catalogue.Name;
                populationSize = 0;
                @abstract = catalogue.ShortDescription;
                contactPoint = catalogue.Administrative_contact_email;
                dataCustodian = new HDRDatasetDataCustodian(catalogue);
            }
            public HDRDatasetMetadataSummary() { }
            public string @abstract { get; set; }//this needs to be populated
            public string contactPoint { get; set; }//this needs to be populated
            public List<object> keywords { get; set; }
            public object doiName { get; set; }
            public string title { get; set; }
            public HDRDatasetDataCustodian dataCustodian { get; set; } = new HDRDatasetDataCustodian();
            public int populationSize { get; set; }
            public object alternateIdentifiers { get; set; }
            public string controlledKeywords { get; set; }
        }
        public class HDRDatasetAccess
        {
            public HDRDatasetAccess() { }
            public HDRDatasetAccess(Catalogue catalogue)
            {
                //todo access rights
                accessRights = catalogue.Access_options;
                //jurisdiction = catalogue.Juristiction.Split(',').ToList();
            }
            public string deliveryLeadTime { get; set; }
            //public List<string> jurisdiction { get; set; }
            public string dataController { get; set; }
            public string dataProcessor { get; set; }
            public string accessRights { get; set; }//this needs to be populated
            public string accessService { get; set; }
            public string accessRequestCost { get; set; }
            public object accessServiceCategory { get; set; }
        }

        private class CustomDateTimeConverterThreeMilliseconds : JsonConverter<DateTime>
        {
            public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return existingValue;
            }


            public override void WriteJson(JsonWriter writer, DateTime value, Newtonsoft.Json.JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.InvariantCulture));
            }
        }

        public class HDRDatasetTemporal
        {
            public HDRDatasetTemporal() { }
            public HDRDatasetTemporal(Catalogue catalogue)
            {
                timeLag = catalogue.UpdateLag.ToString();
                publishingFrequency = catalogue.Update_freq.ToString();
            }

            [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
            public DateTime? endDate { get; set; }

            [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
            public DateTime startDate { get; set; } = DateTime.Now;
            public string timeLag { get; set; }//this needs to be populated
            public string publishingFrequency { get; set; }//this needs to be populated
            public object distributionReleaseDate { get; set; }

            public string accrualPeriodicity { get; set; }//this is the same as publishign frequency?
        }

        public class HDRDatasetFormatAndStandards
        {
            public HDRDatasetFormatAndStandards() { }
            public HDRDatasetFormatAndStandards(Catalogue catalogue)
            {
                //todo
            }

            public string conformsTo { get; set; } = "LOCAL";
            public string vocabularyEncodingSchemes { get; set; } = "LOCAL";
            public string languages { get; set; } = "en";
            public string formats { get; set; } = "CSV;,Database;";
        }

        public class HDRDatasetOrigin
        {
            public List<object> purpose { get; set; }
            public List<object> source { get; set; }
            public List<object> collectionSource { get; set; }
            public List<string> datasetType { get; set; }
            public List<string> datasetSubType { get; set; }
            public string imageContrast { get; set; }
        }

        public class HDRDatasetUsage
        {
            public List<string> dataUseLimitation { get; set; }
            public object resourceCreator { get; set; }
            public List<string> dataUseRequirements { get; set; } = [];
        }
        private class HDRDatasetAccessibility
        {
            public HDRDatasetAccessibility() { }
            public HDRDatasetAccessibility(HDRDatasetObject datasetObject)
            {
                //todo
            }
            public HDRDatasetAccessibility(Catalogue catalogue)
            {
                //access = new HDRDatasetAccess(catalogue);
                formatAndStandards = new HDRDatasetFormatAndStandards(catalogue);
            }
            public object access { get; set; }// = new HDRDatasetAccess();//todo there is an issue descerialising this
            public HDRDatasetUsage usage { get; set; }
            public HDRDatasetFormatAndStandards formatAndStandards { get; set; } = new HDRDatasetFormatAndStandards();
        }
        private class HDRDatasetProvenance
        {
            public HDRDatasetProvenance() { }
            public HDRDatasetProvenance(HDRDatasetObject datasetObject)
            {
                //todo
            }
            public HDRDatasetProvenance(Catalogue catalogue)
            {
                temporal = new HDRDatasetTemporal(catalogue);
            }
            public HDRDatasetOrigin origin { get; set; }
            public HDRDatasetTemporal temporal { get; set; } = new HDRDatasetTemporal();
        }
        public class HDRDatasetRevision
        {
            public string version { get; set; }
            public string url { get; set; }
        }

        private class HDRDatasetCoverage
        {

            public HDRDatasetCoverage() { }
            public HDRDatasetCoverage(HDRDatasetObject datasetObject)
            {
                //todo
            }
            public HDRDatasetCoverage(Catalogue catalogue)
            {
                spatial = catalogue.Country_of_origin;
            }
            public string spatial { get; set; } = "Prussia";
        }

        private class HDRDatasetRequired
        {

            public HDRDatasetRequired() { }
            public HDRDatasetRequired(HDRDatasetObject datasetObject)
            {
                //todo
            }
            public string issued { get; set; }
            public string version { get; set; }
            public string modified { get; set; }
            public string gatewayId { get; set; }
            public string gatewayPid { get; set; }

            public List<HDRDatasetRevision> revisions { get; set; } = new();
        }

        private class HDRDatasetMetadata
        {

            public HDRDatasetMetadata(Catalogue catalogue)
            {
                version = "1.0.0";
                modified = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                issued = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                summary = new HDRDatasetMetadataSummary(catalogue);
            }
            public HDRDatasetMetadata() { }
            public string identifier = "";
            public string version = "1.0.0";
            public List<HDRDatasetRevision> revisions = new List<HDRDatasetRevision>();
            public string modified;
            public string issued;
            public HDRDatasetMetadataSummary summary = new HDRDatasetMetadataSummary();
            public HDRDatasetAccessibility accessibility = new HDRDatasetAccessibility();
            public List<object> observations = new List<object>();
            public HDRDatasetDocumentation documentation = new HDRDatasetDocumentation();
            public HDRDatasetCoverage coverage = new HDRDatasetCoverage();
            public HDRDatasetProvenance provenance = new HDRDatasetProvenance();
            public List<object> tissueSampleCollection = new List<object> { };
            public HDRDatasetRequired required { get; set; }
        }

        private class HDRDatasetInterMetadata
        {
            public HDRDatasetMetadata metadata { get; set; }
        }

        private class HDRDatasetLatestMetadata
        {
            public int id { get; set; }
            public HDRDatasetInterMetadata metadata { get; set; }
        }

        private class HDRDatasetObject
        {
            public int id { get; set; }
            public int team_id { get; set; }
            public HDRDatasetLatestMetadata latest_metadata { get; set; }
        }

        private class HDRResponse
        {
            public int current_page { get; set; }
            public List<HDRDatasetObject> data { get; set; }
        }

        private class HDRDatasetCreationBody
        {
            public string status = "ACTIVE";
            public string create_origin = "MANUAL";
            public HDRDatasetMetadata metadata = new();
            public HDRDatasetCreationBody(Catalogue catalogue)
            {
                metadata = new HDRDatasetMetadata(catalogue);
            }
        }

        private class HDRDatasetUpdateMetadata
        {
            public HDRDatasetMetadataSummary summary { get; set; }
            public HDRDatasetCoverage coverage { get; set; }
            public HDRDatasetRequired required { get; set; }
            public HDRDatasetProvenance provenance { get; set; }
            public List<int> observations = [];
            public HDRDatasetAccessibility accessibility { get; set; }
            public List<int> tissuesSampleCollection = [];

            public HDRDatasetUpdateMetadata() { }
            public HDRDatasetUpdateMetadata(HDRDatasetObject datasetObject)
            {
                summary = datasetObject.latest_metadata.metadata.metadata.summary;//new HDRDatasetMetadataSummary(datasetObject);
                coverage = datasetObject.latest_metadata.metadata.metadata.coverage;// new HDRDatasetCoverage(datasetObject);
                required = datasetObject.latest_metadata.metadata.metadata.required; //new HDRDatasetRequired(datasetObject);
                provenance = datasetObject.latest_metadata.metadata.metadata.provenance; //new HDRDatasetProvenance(datasetObject);
                accessibility = datasetObject.latest_metadata.metadata.metadata.accessibility; //new HDRDatasetAccessibility(datasetObject);
            }
        }

        private class HDRDatasetUpdateInterMetadata
        {

            public HDRDatasetUpdateInterMetadata(HDRDatasetObject datasetObject)
            {
                metadata = new HDRDatasetUpdateMetadata(datasetObject);
            }

            public HDRDatasetUpdateMetadata metadata { get; set; }
        }

        private class HDRDatasetUpdateBody
        {
            public string status = "ACTIVE";
            public HDRDatasetUpdateInterMetadata metadata { get; set; }

            public HDRDatasetUpdateBody() { }
            public HDRDatasetUpdateBody(HDRDatasetObject datasetObject)
            {
                metadata = new HDRDatasetUpdateInterMetadata(datasetObject);
            }

        }

        private class HDRDatasetPUTBody
        {
            public string status = "ACTIVE";
            public string create_origin = "MANUAL";
            //todo put metadata/metadata in here for put specific

            //            {
            //    "status":"ACTIVE",
            //    "metadata": {
            //        "metadata": {
            //            "summary": {
            //                "title": "jf_Biochemistry3",
            //                "abstract": "12345",
            //                "controlledKeywords": null
            //            },
            //            "coverage": {
            //                "spatial": "Prussia"
            //            },
            //            "required": {
            //                "issued": "2025-12-09T14:53:07.260524Z",
            //                "version": "1.0.0",
            //                "modified": "2025-12-09T14:53:07.260541Z",
            //                "gatewayId": "1995",
            //                "revisions": [
            //                    {
            //                        "url": "https://web.preprod.hdruk.cloud/dataset/1995?version=1.0.0",
            //                        "version": "1.0.0"
            //                    }
            //                ],
            //                "gatewayPid": "7e01763e-3d8f-4801-8fe1-548ede6e2f4e"
            //            },
            //            "provenance": {
            //                "temporal": {
            //                    "timeLag": "Less than 1 week",
            //                    "startDate": "2025-12-09T14:53:06.9626377+00:00",
            //                    "accrualPeriodicity": "Quarterly",
            //                    "distributionReleaseDate": null
            //                }
            //            },
            //            "observations": [],
            //            "accessibility": {
            //    "access": {
            //        "accessRights": "123"
            //                },
            //                "formatAndStandards": {
            //        "formats": "CSV",
            //                    "languages": "en",
            //                    "conformsTo": "HL7 FHIR",
            //                    "vocabularyEncodingSchemes": "LOCAL"
            //                }
            //},
            //            "tissuesSampleCollection": []
            //        }
            //    }
            //}
        }

        public ExecuteCommandExportCataloguesToHDRGateway(IBasicActivateItems activator, string url, int team, string appID, string clientID)
        {
            _activator = activator;
            _url = url.TrimEnd('/');
            _team = team;
            _appID = appID;
            _clientID = clientID;
        }

        private HDRDatasetObject CatalogueExists(Catalogue catalogue)
        {
            var url = $"{_url}{_datasetsEndpoint}?title={catalogue.Name}&team={_team}&with_metadata=true";
            var response = Task.Run(async () => await _client.GetAsync(url)).Result;
            var contentString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            var content = JsonConvert.DeserializeObject<HDRResponse>(contentString);
            if (!content.data.Any()) return null;
            var found = content.data.FirstOrDefault(dataset => dataset.latest_metadata.metadata.metadata.summary.title == catalogue.Name);
            if (found != null) return found;
            return null;

        }

        private void CreateDataset(Catalogue catalogue)
        {
            var url = $"{_url}/v2/teams/{_team}/datasets?input_schema=HDRUK&input_version=3.0.0?input_schema=HDRUK&input_version=3.0.0";
            HDRDatasetCreationBody body = new HDRDatasetCreationBody(catalogue);
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IncludeFields = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var jsonString = System.Text.Json.JsonSerializer.Serialize(body, serializeOptions);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = Task.Run(async () => await _client.PostAsync(url, httpContent)).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                //couldn't create, so will try draft - maybe the metadata is missing something
                body.status = "DRAFT";
                jsonString = System.Text.Json.JsonSerializer.Serialize(body, serializeOptions);
                httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                response = Task.Run(async () => await _client.PostAsync(url, httpContent)).Result;
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine($"Unable to create dataset for {catalogue.Name}");
                    return;
                }
            }
            var contentString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            var content = JsonConvert.DeserializeObject<HDRResponse>(contentString);
            Console.WriteLine($"Created Dataset '{catalogue.Name}'");
        }

        private void UpdateDataset(Catalogue catalogue, HDRDatasetObject existingObject)
        {
            var url = $"{_url}/v2/teams/{_team}/datasets/{existingObject.id}?input_schema=HDRUK&input_version=3.0.0?input_schema=HDRUK&input_version=3.0.0";

            HDRDatasetUpdateBody body = new HDRDatasetUpdateBody(existingObject);
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IncludeFields = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            body.metadata.metadata.summary.dataCustodian = null;
            body.metadata.metadata.summary.@abstract = "todo123";
            body.metadata.metadata.summary.controlledKeywords = "this;,that;";
            body.metadata.metadata.provenance.temporal.timeLag = "Less than 1 week";
            body.metadata.metadata.provenance.temporal.distributionReleaseDate = null;
            body.metadata.metadata.provenance.temporal.accrualPeriodicity = "Quarterly";
            body.metadata.metadata.accessibility.access = new HDRDatasetAccess() { accessRights="123"};
            body.metadata.metadata.accessibility.usage = null;
            var jsonString = System.Text.Json.JsonSerializer.Serialize(body, serializeOptions);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = Task.Run(async () => await _client.PatchAsync(url, httpContent)).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var contentString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                Console.WriteLine($"unable to update Dataset '{catalogue.Name}': {contentString}");

            }
            Console.WriteLine($"Updated Dataset '{catalogue.Name}'");
        }

        public override void Execute()
        {
            base.Execute();
            _client.DefaultRequestHeaders.Add("x-application-id", _appID);
            _client.DefaultRequestHeaders.Add("x-client-id", _clientID);

            var catalogues = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
                .Where(c => !c.IsDeprecated && !c.IsInternalDataset && !c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository))
                .ToList();
            foreach (var catalogue in catalogues)
            {
                var existingDataset = CatalogueExists(catalogue);
                if (existingDataset != null)
                {
                    //update
                    UpdateDataset(catalogue, existingDataset);
                }
                else
                {
                    //create
                    CreateDataset(catalogue);
                }
            }

        }

    }
}
