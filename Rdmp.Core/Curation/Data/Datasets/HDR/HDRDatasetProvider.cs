using Org.BouncyCastle.Asn1.Cms;
using Rdmp.Core.CommandExecution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rdmp.Core.Curation.Data.Datasets.HDR.Helpers;
using Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems;
using MongoDB.Bson;

namespace Rdmp.Core.Curation.Data.Datasets.HDR
{
    /// <summary>
    /// Provider for connecting to the HDR Gateway
    /// </summary>
    public class HDRDatasetProvider : PluginDatasetProvider
    {
        private HttpClient _client;
        private DatasetProviderConfiguration _configuration;
        public HDRDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration, HttpClient client = null) : base(activator, configuration)
        {
            _client = new HttpClient();
            var credentials = Repository.GetAllObjectsWhere<DataAccessCredentials>("ID", Configuration.DataAccessCredentials_ID).First();
            var apiKey = credentials.GetDecryptedPassword();
            _client.DefaultRequestHeaders.Add("x-application-id", credentials.Username);
            _client.DefaultRequestHeaders.Add("x-client-id", apiKey);
            _configuration = configuration;
        }
        public override Dataset AddExistingDatasetWithReturn(string name, string url)
        {
            HDRDataset hdrDataset = (HDRDataset)FetchDatasetByID(int.Parse(url));
            var datasetName = string.IsNullOrWhiteSpace(name) ? hdrDataset?.data.versions?.First().metadata.metadata.summary.title : name;
            var dataset = new Dataset(Repository, datasetName)
            {
                Url = url,
                Type = ToString(),
                Provider_ID = Configuration.ID,
                //DigitalObjectIdentifier = hdrDataset.data.versions.First().metadata.metadata.summary.doiName.ToString(),
                Folder = $"\\{Configuration.Name}",
            };
            dataset.SaveToDatabase();
            Activator.Publish(dataset);
            return dataset;
        }

        public override void AddExistingDataset(string name, string url)
        {
            AddExistingDatasetWithReturn(name, url);
        }

        private class CreateDatasetResponse
        {
            public int data { get; set; }
        }

        public override Dataset Create(Catalogue catalogue)
        {
            var url = Configuration.Url + "/v1/integrations/datasets?input_schema=HDRUK&input_version=3.0.0";
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IncludeFields = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            using var stream = new MemoryStream();
            var ds = new HDRDatasetPost(catalogue);

            var jsonString = System.Text.Json.JsonSerializer.Serialize(ds, serializeOptions);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = Task.Run(async () => await _client.PostAsync(url, httpContent)).Result;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                var responseJson = JsonConvert.DeserializeObject<CreateDatasetResponse>(content);
                var dataset = FetchDatasetByID(responseJson.data) as HDRDataset;
                //(dataset, catalogue);//todo wll have to test this
                return dataset;
            }
            else
            {
                var content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                throw new Exception(content);
            }

        }

        public HDRDataset FetchHDRDataset(Dataset dataset)
        {
            var response = Task.Run(async () => await _client.GetAsync($"{dataset.Url}?schema_model=HDRUK&schema_version=3.0.0")).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                Console.WriteLine(detailsString);
                HDRDataset hdrDataset = JsonConvert.DeserializeObject<HDRDataset>(detailsString);
                return hdrDataset;
            }
            throw new Exception("Unable to fetch HDR dataset");
        }

        public override Dataset FetchDatasetByID(int id)
        {
            var url = Configuration.Url + "/v1/datasets/" + id;
            var response = Task.Run(async () => await _client.GetAsync($"{url}?schema_model=HDRUK&schema_version=3.0.0")).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var detailsString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                Console.WriteLine(detailsString);
                HDRDataset hdrDataset = JsonConvert.DeserializeObject<HDRDataset>(detailsString);
                return hdrDataset;
            }
            throw new Exception("Unable to fetch HDR dataset");
        }

        public override void Update(string uuid, PluginDataset datasetUpdates)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            serializeOptions.Converters.Add(new CustomDateTimeConverter());
            serializeOptions.Converters.Add(new CustomDateTimeConverterThreeMilliseconds());

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            var update = (HDRDataset)datasetUpdates;
            var updateObj = new HDRUpdateObject()
            {
                metadata = new HDRDatasetPatch((HDRDataset)datasetUpdates).metadata.metadata
            };
            System.Text.Json.JsonSerializer.Serialize<PatchSubMetadata>(stream, updateObj.metadata, serializeOptions);
            var jsonString = "{\"metadata\":{\"metadata\":" + Encoding.UTF8.GetString(stream.ToArray()) + "}}";
            var uri = $"{Configuration.Url}/v1/integrations/datasets/{uuid}?input_schema=HDRUK&input_version=3.0.0";
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = Task.Run(async () => await _client.PutAsync(uri, httpContent)).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var x = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                throw new Exception(x);
            }
        }

        private string MapDataTypeToHDRDataType(string dt)
        {
            switch (dt)
            {
                case "HealthcareAndDisease":
                    return "Healthcare and disease";
                case "TreatmentsAndInterventions":
                    return "Treatments/Interventions";
                case "MeasurementsAndTests":
                    return "Measurements/Tests";
                case "ImagingTypes":
                    return "Imaging types";
                case "ImagingAreaOfTheBody":
                    return "Imaging area of the body";
                case "Omics":
                    return "Omics";
                case "Socioeconomic":
                    return "Socioeconomic";
                case "Lifestyle":
                    return "Lifestyle";
                case "Registry":
                    return "Registry";
                case "EnvironmentalAndEnergy":
                    return "Environmental and energy";
                case "InformationAndCommunication":
                    return "Information and communication";
                case "Politics":
                    return "Politics";
                default:
                    return dt;
            }
        }

        private string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) ||
                        preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        private string MapDataSubTypeToHDR(string dst)
        {
            if (dst == "ResearchDiseaseRegistry") return "Disease registry (research)";
            return AddSpacesToSentence(dst, true);
        }

        private string MapTimeLagToHDR(string tl)
        {
            switch (tl)
            {
                case "LessThanAWeek":
                    return "Less than 1 week";
                case "OneToTwoWeeks":
                    return "1-2 weeks";
                case "TwoToFourWeeks":
                    return "2-4 weeks";
                case "OneToTwoMonths":
                    return "1-2 months";
                case "TwoToSixMonths":
                    return "2-6 months";
                case "SixMonthsPlus":
                    return "More than 6 months";
                case "NotApplicable":
                    return "Not applicable";
                default:
                    return tl;
            }
        }

        public override void UpdateUsingCatalogue(Dataset dataset, Catalogue catalogue)
        {
            var hdrDataset = (HDRDataset)FetchDatasetByID(int.Parse(dataset.Url));
            hdrDataset.data.versions.First().metadata.metadata.summary.title = catalogue.Name;
            hdrDataset.data.versions.First().metadata.metadata.summary.@abstract = catalogue.ShortDescription.Length <5? catalogue.ShortDescription.PadRight(5): catalogue.ShortDescription;
            hdrDataset.data.versions.First().metadata.metadata.summary.contactPoint = catalogue.Administrative_contact_email;
            hdrDataset.data.versions.First().metadata.metadata.summary.keywords = (catalogue.Search_keywords ?? "").Split(',').Cast<string>().Where(k => k != "").Cast<object>().ToList();
            hdrDataset.data.versions.First().metadata.metadata.summary.doiName = catalogue.Doi;

            hdrDataset.data.versions.First().metadata.metadata.documentation.description = catalogue.Description;
            hdrDataset.data.versions.First().metadata.metadata.documentation.associatedMedia = catalogue.AssociatedMedia;

            hdrDataset.data.versions.First().metadata.metadata.coverage.spatial = catalogue.Geographical_coverage;

            if(catalogue.DataType != null)
                hdrDataset.data.versions.First().metadata.metadata.provenance.origin.datasetType = catalogue.DataType.Split(",").Select(MapDataTypeToHDRDataType).ToList();
            if (catalogue.DataSubType != null)
                hdrDataset.data.versions.First().metadata.metadata.provenance.origin.datasetSubType = catalogue.DataSubType.Split(",").Select(MapDataSubTypeToHDR).ToList();
            hdrDataset.data.versions.First().metadata.metadata.provenance.temporal.endDate = catalogue.EndDate != null ? catalogue.EndDate.Value : null;
            if (catalogue.StartDate != null)
                hdrDataset.data.versions.First().metadata.metadata.provenance.temporal.startDate = catalogue.StartDate.Value;
            hdrDataset.data.versions.First().metadata.metadata.provenance.temporal.timeLag = MapTimeLagToHDR(catalogue.UpdateLag.ToString());
            hdrDataset.data.versions.First().metadata.metadata.provenance.temporal.publishingFrequency = catalogue.Update_freq.ToString();

            hdrDataset.data.versions.First().metadata.metadata.accessibility.access.jurisdiction = (catalogue.Juristiction ?? "").Split(",").Where(k => k != "").ToList();
            hdrDataset.data.versions.First().metadata.metadata.accessibility.access.dataController = catalogue.DataController;
            hdrDataset.data.versions.First().metadata.metadata.accessibility.access.dataProcessor = catalogue.DataProcessor;

            hdrDataset.data.versions.First().metadata.metadata.identifier = "05ec5a13-3955-45a3-b449-8aba78622113";// hdrDataset.data.versions.First().metadata.identifier;

            Update(hdrDataset.data.id.ToString(), hdrDataset);
        }

        private class HDRUpdateObject
        {
            public int team_id { get; set; }
            public int user_id { get; set; }
            public string create_origin { get; set; }
            public PatchSubMetadata metadata { get; set; }
        }
    }
}
