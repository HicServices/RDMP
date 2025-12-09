using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.SecurityToken.Model;
using Azure;
using MathNet.Numerics;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt;
using NPOI.SS.UserModel.Charts;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Dataset.Confluence;
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

        public class HDRDatasetMetadataSummary
        {
            public HDRDatasetMetadataSummary(Catalogue catalogue)
            {
                title = catalogue.Name;
                populationSize = 0;
                contactPoint = "";
            }
            public HDRDatasetMetadataSummary() { }
            public string @abstract { get; set; }
            public string contactPoint { get; set; }
            public List<object> keywords { get; set; }
            public object doiName { get; set; }
            public string title { get; set; }
            //public DataCustodian dataCustodian { get; set; }
            public int populationSize { get; set; }
            public object alternateIdentifiers { get; set; }
        }
        public class HDRDatasetAccess
        {
            public string deliveryLeadTime { get; set; }
            public List<string> jurisdiction { get; set; }
            public string dataController { get; set; }
            public string dataProcessor { get; set; }
            public string accessRights { get; set; }
            public string accessService { get; set; }
            public string accessRequestCost { get; set; }
            public object accessServiceCategory { get; set; }
        }

        private class CustomDateTimeConverterThreeMilliseconds : JsonConverter<DateTime>
        {
            public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return DateTime.Parse(reader.ReadAsString());
            }


            public override void WriteJson(JsonWriter writer, DateTime value, Newtonsoft.Json.JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.InvariantCulture));
            }
        }

        public class HDRDatasetTemporal
        {
            [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]

            public DateTime? endDate { get; set; }

            [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
            public DateTime startDate { get; set; }
            public string timeLag { get; set; }
            public string publishingFrequency { get; set; }
            public object distributionReleaseDate { get; set; }
        }

        public class HDRDatasetFormatAndStandards
        {
            public List<string> conformsTo { get; set; }
            public List<string> vocabularyEncodingScheme { get; set; }
            public List<string> language { get; set; }
            public List<string> format { get; set; }
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
        public class HDRDatasetAccessibility
        {
            public HDRDatasetAccess access { get; set; }
            public HDRDatasetUsage usage { get; set; }
            public HDRDatasetFormatAndStandards formatAndStandards { get; set; }
        }
        public class HDRDatasetProvenance
        {
            public HDRDatasetOrigin origin { get; set; }
            public HDRDatasetTemporal temporal { get; set; }
        }
        public class HDRDatasetRevision
        {
            public string version { get; set; }
            public string url { get; set; }
        }

        public class HDRDatasetMetadata
        {
       
            public HDRDatasetMetadata(Catalogue catalogue)
            {
                identifier = "";
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
            public HDRDatasetProvenance provenance = new HDRDatasetProvenance();
        }

        public class HDRDatasetInterMetadata
        {
            public HDRDatasetMetadata metadata { get; set; }
            public HDRDatasetInterMetadata() { }
            public HDRDatasetInterMetadata(Catalogue catalogue)
            {
                metadata = new HDRDatasetMetadata(catalogue);
            }
        }

        private class HDRDatasetLatestMetadata
        {
            public int id { get; set; }
            public HDRDatasetInterMetadata metadata { get; set; }

            public HDRDatasetLatestMetadata() { }
            public HDRDatasetLatestMetadata(Catalogue catalogue)
            {
                metadata = new HDRDatasetInterMetadata(catalogue);
            }

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
            public HDRDatasetMetadata metadata = new();
            public HDRDatasetCreationBody(Catalogue catalogue)
            {
                metadata =  new HDRDatasetMetadata(catalogue);
            }
        }

        public ExecuteCommandExportCataloguesToHDRGateway(IBasicActivateItems activator, string url, int team, string appID, string clientID)
        {
            _activator = activator;
            _url = url.TrimEnd('/');
            _team = team;
            _appID = appID;
            _clientID = clientID;
        }

        private bool CatalogueExists(Catalogue catalogue)
        {
            var url = $"{_url}{_datasetsEndpoint}?title={catalogue.Name}&team={_team}";
            var response = Task.Run(async () => await _client.GetAsync(url)).Result;
            var contentString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            var content = JsonConvert.DeserializeObject<HDRResponse>(contentString);
            if (!content.data.Any()) return false;
            if (content.data.Any(dataset => dataset.latest_metadata.metadata.metadata.summary.title == catalogue.Name)) return true;
            return false;
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
            var jsonString = "{\"metadata\":{\"identifier\":\"05ec5a13-3955-45a3-b449-8aba78622113\",\"issued\":\"2025-01-08T11:23:35.000Z\",\"modified\":\"2025-01-08T11:23:35.000Z\",\"revisions\":[{\"version\":\"1.0.0\",\"url\":\"http://www.example.com/\"}],\"version\":\"1.0.0\",\"summary\":{\"abstract\":\"Thisisatestdatasetabstract-update11\",\"contactPoint\":\"jfriel001@dundee.ac.uk\",\"keywords\":[],\"doiName\":null,\"title\":\"JFRIELTESTDataset\",\"dataCustodian\":{\"name\":\"HealthInformaticsCentre-UniversityofDundee\",\"identifier\":\"12\",\"contactPoint\":\"jfriel001@dundee.ac.uk\",\"logo\":null,\"description\":null,\"memberOf\":null},\"populationSize\":-1,\"alternateIdentifiers\":null},\"documentation\":{\"description\":\"TESTDescription3\",\"associatedMedia\":null,\"inPipeline\":\"Notavailable\"},\"coverage\":{\"pathway\":null,\"spatial\":\"Prussia,OttomanEmpire,GranColombia\",\"followUp\":null,\"datasetCompleteness\":null,\"materialType\":[\"Other\"],\"typicalAgeRangeMin\":0,\"typicalAgeRangeMax\":0},\"provenance\":{\"origin\":{\"purpose\":[],\"source\":[],\"collectionSource\":[],\"datasetType\":[\"Healthanddisease\"],\"datasetSubType\":[\"Notapplicable\"],\"imageContrast\":\"Notstated\"},\"temporal\":{\"endDate\":null,\"startDate\":\"2025-01-01T11:26:16.000Z\",\"timeLag\":\"Variable\",\"publishingFrequency\":\"Irregular\",\"distributionReleaseDate\":null}},\"accessibility\":{\"access\":{\"deliveryLeadTime\":\"Notapplicable\",\"jurisdiction\":[\"UK\",\"SC\"],\"dataController\":\"HealthInformaticsCentre-UniversityofDundee\",\"dataProcessor\":\"HealthInformaticsCentre-UniversityofDundee\",\"accessRights\":\"https://www.dundee.ac.uk/hic/governance-service\",\"accessService\":\"HIChasimplementedaremote-accessTrustedResearchEnvironmenttoprotectdataconfidentiality,satisfypublicconcernsaboutdatalossandreassureDataControllersaboutHIC\\u00e2\\u20ac\\u2122ssecuremanagementandprocessingoftheirdata.\\n\\nDataisnotreleasedexternallytodatausersforanalysisontheirowncomputersbutplacedonaserveratHIC,withinarestricted,secureITenvironment,wherethedatauserisgivensecureremoteaccesstocarryouttheiranalysis.\\n\\nFulldetailsareavailableviathefollowinglink:\\nhttps://www.dundee.ac.uk/hic/safe-haven\",\"accessRequestCost\":\"Quotationavailableonrequest\",\"accessServiceCategory\":null},\"usage\":{\"dataUseLimitation\":[\"Generalresearchuse\"],\"resourceCreator\":null,\"dataUseRequirements\":null},\"formatAndStandards\":{\"conformsTo\":[\"I2B2\"],\"vocabularyEncodingScheme\":[\"LOCAL\"],\"language\":[\"en\"],\"format\":[\"CSV\",\"Database\"]}},\"enrichmentAndLinkage\":{\"tools\":[],\"investigations\":[],\"publicationAboutDataset\":[],\"publicationUsingDataset\":[],\"derivedFrom\":null,\"isPartOf\":null,\"linkableDatasets\":null,\"similarToDatasets\":null},\"observations\":[],\"structuralMetadata\":{\"tables\":[],\"syntheticDataWebLink\":null},\"demographicFrequency\":null,\"omics\":null}}";//System.Text.Json.JsonSerializer.Serialize(body, serializeOptions);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = Task.Run(async () => await _client.PostAsync(url, httpContent)).Result;
            var contentString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            var content = JsonConvert.DeserializeObject<HDRResponse>(contentString);
            Console.WriteLine($"Created Dataset '{catalogue.Name}'");

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
                if (CatalogueExists(catalogue))
                {
                    //update
                    Console.WriteLine($"update {catalogue.Name}");
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
