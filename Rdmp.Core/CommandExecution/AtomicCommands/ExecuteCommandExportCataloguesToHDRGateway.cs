
using MongoDB.Driver;
using Newtonsoft.Json;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private InputJSONDetails _inputJsonDetails = new();
        private class InputJSONDetails
        {
            /// <summary>
            /// {
            ///     "accessRights":"https://www.dundee.ac.uk/hic/governance-service",
            ///     "accessService":"some kind of access message- get in touch",
            ///     "accessServiceCategory":"TRE/SDE",
            ///     "conformsTo":["LOCAL"],
            ///     "vocabularyEncodingScheme":["LOCAL"],
            ///     "language":["en"],
            ///     "format":["CSV","Database"]
            ///     "dataUseLimitation":["General research use"],
            ///     "resourceCreator":"Please cite us!",
            ///     "dataUseRequirements":["Disclosure control"]
            /// }
            /// </summary>
            public string accessRights { get; set; }
            public string accessService { get; set; }
            public string accessRequestCost { get; set; }
            public string accessServiceCategory { get; set; }
            public List<string> conformsTo { get; set; }
            public List<string> vocabularyEncodingScheme { get; set; }
            public List<string> language { get; set; }
            public List<string> format { get; set; }
            public List<string> dataUseLimitation { get; set; }
            public string resourceCreator { get; set; }
            public List<string> dataUseRequirements { get; set; }
        }

        public class HDRDatasetDataCustodian
        {
            public HDRDatasetDataCustodian() { }
            public HDRDatasetDataCustodian(Catalogue catalogue)
            {
                name = catalogue.DataProcessor;
                contactPoint = catalogue.Administrative_contact_email;
            }
            public string name { get; set; }
            public int identifier { get; set; } = 0;
            public string contactPoint { get; set; }
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

            public void Update(Catalogue catalogue)
            {
                description = catalogue.Description;

            }
            public string description { get; set; }
        }
        private class HDRDatasetMetadataSummary
        {
            public HDRDatasetMetadataSummary(Catalogue catalogue)
            {
                Update(catalogue);
            }
            public void Update(Catalogue catalogue)
            {
                title = catalogue.Name;
                populationSize = 0;
                @abstract = catalogue.ShortDescription;
                contactPoint = catalogue.Administrative_contact_email;
                dataCustodian = new HDRDatasetDataCustodian(catalogue);
                keywords = catalogue.Search_keywords != null ? catalogue.Search_keywords.Split(',').ToList() : null;
                doiName = catalogue.Doi;
            }

            public HDRDatasetMetadataSummary() { }
            public string @abstract { get; set; }
            public string contactPoint { get; set; }
            public object keywords { get; set; }
            public object doiName { get; set; }
            public string title { get; set; }
            public HDRDatasetDataCustodian dataCustodian { get; set; } = new HDRDatasetDataCustodian();
            public int populationSize { get; set; }
            public object alternateIdentifiers { get; set; }
            public object controlledKeywords { get; set; }
        }
        private class HDRDatasetAccess
        {

            private static string MapUpdateLag(Catalogue.UpdateLagTimes time)
            {
                switch (time)
                {
                    case Catalogue.UpdateLagTimes.LessThanAWeek:
                        return "Less than 1 week";
                    case Catalogue.UpdateLagTimes.OneToTwoWeeks:
                        return "1-2 weeks";
                    case Catalogue.UpdateLagTimes.TwoToFourWeeks:
                        return "2-4 weeks";
                    case Catalogue.UpdateLagTimes.OneToTwoMonths:
                        return "1-2 months";
                    case Catalogue.UpdateLagTimes.TwoToSixMonths:
                        return "2-6 months";
                    case Catalogue.UpdateLagTimes.SixMonthsPlus:
                        return "More than 6 months";
                    case Catalogue.UpdateLagTimes.Variable:
                        return "Variable";
                    case Catalogue.UpdateLagTimes.NotApplicable:
                        return "Not applicable";
                    case Catalogue.UpdateLagTimes.Other:
                        return "Other";
                }
                return null;
            }

            public HDRDatasetAccess() { }
            public HDRDatasetAccess(Catalogue catalogue, InputJSONDetails details)
            {
                Update(catalogue, details);
            }

            public void Update(Catalogue catalogue, InputJSONDetails details)
            {
                jurisdiction = catalogue.Juristiction != null ? catalogue.Juristiction.Split(',').ToList() : null;
                deliveryLeadTime = MapUpdateLag(catalogue.UpdateLag);
                dataController = catalogue.DataController;
                dataProcessor = catalogue.DataProcessor;
                accessRights = details.accessRights;
                accessService = details.accessService;
                accessServiceCategory = details.accessServiceCategory;
            }

            public string deliveryLeadTime { get; set; }
            public object jurisdiction { get; set; }
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
                if (catalogue.StartDate != null) startDate = (DateTime)catalogue.StartDate;
                if (catalogue.EndDate != null) endDate = (DateTime)catalogue.EndDate;

            }

            public void Update(Catalogue catalogue)
            {
                timeLag = catalogue.UpdateLag.ToString();
                publishingFrequency = catalogue.Update_freq.ToString();
                if (catalogue.StartDate != null) startDate = (DateTime)catalogue.StartDate;
                if (catalogue.EndDate != null) endDate = (DateTime)catalogue.EndDate;
                if (catalogue.DatasetReleaseDate != null) distributionReleaseDate = (DateTime)catalogue.DatasetReleaseDate;
            }

            [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
            public DateTime? endDate { get; set; }

            [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
            public DateTime startDate { get; set; }
            public string timeLag { get; set; }
            public string publishingFrequency { get; set; }
            public object distributionReleaseDate { get; set; }
        }

        private class HDRDatasetFormatAndStandards
        {
            public HDRDatasetFormatAndStandards() { }
            public HDRDatasetFormatAndStandards(Catalogue catalogue, InputJSONDetails details)
            {
                Update(catalogue, details);
            }
            public void Update(Catalogue catalogue, InputJSONDetails details)
            {
                conformsTo = details.conformsTo;
                vocabularyEncodingScheme = details.vocabularyEncodingScheme;
                language = details.language;
                format = details.format;
            }
            public object conformsTo { get; set; }
            public object vocabularyEncodingScheme { get; set; }
            public object language { get; set; }
            public object format { get; set; }
        }

        public class HDRDatasetOrigin
        {


            public HDRDatasetOrigin() { }


            private static string MapDatasetType(string datasetType)
            {
                switch (datasetType)
                {
                    case "HealthcareAndDisease":
                        return "Health and disease";
                    case "TreatmentsAndInterventions":
                        return "Treatments/Interventions";
                    case "MeasurementsAndTests":
                        return "Measurements/Tests";
                    case "ImagingTypes":
                        return "Imaging Types";
                    case "Omics":
                        return "Omics";
                    case "Socioeconomic":
                        return "Socioeconomic";
                    case "Lifestyle":
                        return "Lifestyle";
                    case "Registry":
                        return "Registry";
                    case "EnvironmentalAndEnergy":
                        return "Environment and energy";
                    case "InformationAndCommunication":
                        return "Information and communication";
                    case "Politics":
                        return "Politics";
                }
                return "";
            }

            private static string MapDataSourceType(string dataSourceType)
            {
                switch (dataSourceType)
                {
                    case "Other":
                        return "Other";
                    case "EPR":
                        return "EPR";
                    case "ElectronicSurvey":
                        return "Electronic survey";
                    case "LIMS":
                        return "LIMS";
                    case "PaperBased":
                        return "Paper-based";
                    case "FreeTextNLP":
                        return "Free text NLP";
                    case "MachineLearning":
                        return "Machine generated";
                }
                return "";
            }

            private static string MapPurpose(Catalogue.DatasetPurpose purpose)
            {
                switch (purpose)
                {


                    case Catalogue.DatasetPurpose.ResearchCohort:
                        return "Research cohort";
                    case Catalogue.DatasetPurpose.Study:
                        return "Study";
                    case Catalogue.DatasetPurpose.DiseaseRegistry:
                        return "Disease registry";
                    case Catalogue.DatasetPurpose.Trial:
                        return "Trial";
                    case Catalogue.DatasetPurpose.Care:
                        return "Care";
                    case Catalogue.DatasetPurpose.Audit:
                        return "Audit";
                    case Catalogue.DatasetPurpose.Administrative:
                        return "Administrative";
                    case Catalogue.DatasetPurpose.Finantial:
                        return "Financial";
                    case Catalogue.DatasetPurpose.Statutory:
                        return "Statutory";
                    case Catalogue.DatasetPurpose.Other:
                        return "Other";
                }
                return null;
            }

            private class DataTypeObject
            {
                public string name;
                public List<string> subTypes = new();
            }

            private static string MapCollectionSource(string source)
            {
                switch (source)
                {
                    case "CohortStudyTrial":
                        return "Cohort, study, trial";
                    case "Clininc":
                        return "Clinic";
                    case "PrimaryCareReferrals":
                        return "Primary care - Referrals";
                    case "PrimaryCareClinic":
                        return "Primary care - Clinic";
                    case "PrimaryCareOutOfHours":
                        return "Primary care - Out of hours";
                    case "SecondaryCareAccidentAndEmergency":
                        return "Secondary care - Accident and Emergency";
                    case "SecondaryCareOutpatients":
                        return "Secondary care - Outpatients";
                    case "SecondaryCareInPateints":
                        return "Secondary care - In-patients";
                    case "SecondaryCareAmbulance":
                        return "Secondary care - Ambulance";
                    case "SecondaryCareICU":
                        return "Secondary care - ICU";
                    case "PrescribingCommunityPharmacy":
                        return "Prescribing - Community pharmacy";
                    case "PrescribingHospital":
                        return "Prescribing - Hospital";
                    case "PateintReportOutcome":
                        return "Patient report outcome";
                    case "Wearables":
                        return "Wearables";
                    case "LocalAuthority":
                        return "Local authority";
                    case "NationalGovernment":
                        return "National government";
                    case "Community":
                        return "Community";
                    case "Services":
                        return "Services";
                    case "Home":
                        return "Home";
                    case "Private":
                        return "Private";
                    case "SocialCareHealthcareAtHome":
                        return "Social care - Health care at home";
                    case "SocialCareOthersocialData":
                        return "Social care - Other social data";
                    case "Census":
                        return "Census";
                    case "Other":
                        return "Other";
                }
                return null;
            }

            public HDRDatasetOrigin(Catalogue catalogue)
            {
                Update(catalogue);
            }

            public void Update(Catalogue catalogue)
            {
                source = catalogue.DataSource != null ? catalogue.DataSource.Split(',').Select(s => MapDataSourceType(s)).Where(s => s != "") : null;
                purpose = new List<String>() { MapPurpose(catalogue.Purpose) };
                collectionSource = catalogue.DataSourceSetting != null ? catalogue.DataSourceSetting.Split(',').Select(source => MapCollectionSource(source)) : null;
                datasetType = catalogue.DataType != null ? catalogue.DataType.Split(',').Select(s => MapDatasetType(s)).Where(s => s != "").Select(s => new DataTypeObject() { name = s }) : null;

            }
            public object purpose { get; set; }
            public object source { get; set; }
            public object collectionSource { get; set; }
            public object datasetType { get; set; }
            public string datasetSubType { get; set; }
            public string imageContrast { get; set; }
        }

        private class HDRDatasetUsage
        {
            public HDRDatasetUsage() { }
            public HDRDatasetUsage(InputJSONDetails details)
            {
                dataUseLimitation = details.dataUseLimitation;
                resourceCreator = details.resourceCreator;
                dataUseRequirements = details.dataUseRequirements;
            }

            public object dataUseLimitation { get; set; }
            public object resourceCreator { get; set; }
            public List<string> dataUseRequirements { get; set; }
        }
        private class HDRDatasetAccessibility
        {
            public HDRDatasetAccessibility() { }

            public HDRDatasetAccessibility(Catalogue catalogue, InputJSONDetails details)
            {
                access = new HDRDatasetAccess(catalogue, details);
                formatAndStandards = new HDRDatasetFormatAndStandards(catalogue, details);
                usage = new HDRDatasetUsage(details);
            }

            public void Update(Catalogue catalogue, InputJSONDetails details)
            {
                access = new HDRDatasetAccess(catalogue, details);
                formatAndStandards.Update(catalogue, details);
                usage = new HDRDatasetUsage(details);
            }
            public object access { get; set; }
            public HDRDatasetUsage usage { get; set; }
            public HDRDatasetFormatAndStandards formatAndStandards { get; set; } = new HDRDatasetFormatAndStandards();
        }
        private class HDRDatasetProvenance
        {
            public HDRDatasetProvenance() { }

            public HDRDatasetProvenance(Catalogue catalogue)
            {
                temporal = new HDRDatasetTemporal(catalogue);
                origin = new HDRDatasetOrigin(catalogue);
            }

            public void Update(Catalogue catalogue)
            {
                temporal.Update(catalogue);
                if (origin != null) origin.Update(catalogue);
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

            public HDRDatasetCoverage(Catalogue catalogue)
            {
                spatial = catalogue.Geographical_coverage;
            }

            public void Update(Catalogue catalogue)
            {
                spatial = catalogue.Geographical_coverage;
            }
            public string spatial { get; set; }
        }

        private class HDRDatasetRequired
        {

            public HDRDatasetRequired() { }

            public string issued { get; set; }
            public string version { get; set; }
            public string modified { get; set; }
            public string gatewayId { get; set; }
            public string gatewayPid { get; set; }

            public List<HDRDatasetRevision> revisions { get; set; } = new();
        }

        private class HDRDatasetMetadata
        {

            public HDRDatasetMetadata(Catalogue catalogue, InputJSONDetails details)
            {
                summary = new HDRDatasetMetadataSummary(catalogue);
                documentation = new HDRDatasetDocumentation(catalogue);
                provenance = new HDRDatasetProvenance(catalogue);
                coverage = new HDRDatasetCoverage(catalogue);
                accessibility = new HDRDatasetAccessibility(catalogue, details);
            }

            public HDRDatasetMetadata() { }
            public string identifier = "";
            public string version = "1.0.0";
            public List<HDRDatasetRevision> revisions = new List<HDRDatasetRevision>();
            public string modified = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            public string issued = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            public HDRDatasetMetadataSummary summary = new HDRDatasetMetadataSummary();
            public HDRDatasetAccessibility accessibility = new HDRDatasetAccessibility();
            public List<object> observations = new List<object>();
            public HDRDatasetDocumentation documentation = new HDRDatasetDocumentation();
            public HDRDatasetCoverage coverage = new HDRDatasetCoverage();
            public HDRDatasetProvenance provenance = new HDRDatasetProvenance();
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
            public HDRDatasetCreationBody(Catalogue catalogue, InputJSONDetails details)
            {
                metadata = new HDRDatasetMetadata(catalogue, details);
            }
        }


        private class HDRDatasetUpdateMetadata
        {
            public string version = "1.0.0";
            public DateTime issued = DateTime.Now;
            public DateTime modified = DateTime.Now;
            public string identifier = Guid.NewGuid().ToString();
            public HDRDatasetMetadataSummary summary { get; set; }
            public HDRDatasetCoverage coverage { get; set; }
            public List<HDRDatasetRevision> revisions = new();
            public HDRDatasetProvenance provenance { get; set; }
            public List<int> observations = [];
            public HDRDatasetAccessibility accessibility { get; set; }

            public HDRDatasetDocumentation documentation { get; set; }

            public HDRDatasetUpdateMetadata() { }

            private string UpdateSemVer(string currentVersion)
            {
                var ver = currentVersion.Split('.');
                var newPatch = Int32.Parse(ver.Last()) + 1;
                ver[2] = newPatch.ToString();
                return String.Join('.', ver);

            }

            public HDRDatasetUpdateMetadata(HDRDatasetObject datasetObject)
            {
                summary = datasetObject.latest_metadata.metadata.metadata.summary;
                coverage = datasetObject.latest_metadata.metadata.metadata.coverage;
                provenance = datasetObject.latest_metadata.metadata.metadata.provenance;
                accessibility = datasetObject.latest_metadata.metadata.metadata.accessibility;
                documentation = datasetObject.latest_metadata.metadata.metadata.documentation;
                version = UpdateSemVer(datasetObject.latest_metadata.metadata.metadata.required.version);
            }
            public void Update(Catalogue catalogue, InputJSONDetails details)
            {
                summary.Update(catalogue);
                coverage.Update(catalogue);
                provenance.Update(catalogue);
                accessibility.Update(catalogue,details);
                documentation = new HDRDatasetDocumentation(catalogue);
            }
        }

        private class HDRDatasetUpdateInterMetadata
        {

            public HDRDatasetUpdateInterMetadata(HDRDatasetObject datasetObject)
            {

                metadata = new HDRDatasetUpdateMetadata(datasetObject);
            }

            public HDRDatasetUpdateMetadata metadata { get; set; }
            public string schemaModel = "HDRUK";
            public string schemaVersion = "4.0.0";
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

            public void Update(Catalogue catalogue, InputJSONDetails details)
            {
                metadata.metadata.Update(catalogue, details);
            }

        }

        public ExecuteCommandExportCataloguesToHDRGateway(IBasicActivateItems activator, string url, int team, string appID, string clientID, FileInfo configFile)
        {
            _activator = activator;
            _url = url.TrimEnd('/');
            _team = team;
            _appID = appID;
            _clientID = clientID;
            var x = File.ReadAllText(configFile.Name);
            _inputJsonDetails = JsonConvert.DeserializeObject<InputJSONDetails>(File.ReadAllText(configFile.Name));
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
            var url = $"{_url}/v2/teams/{_team}/datasets?input_schema=HDRUK&input_version=4.0.0?input_schema=HDRUK&input_version=4.0.0";
            HDRDatasetCreationBody body = new HDRDatasetCreationBody(catalogue,_inputJsonDetails);
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
            Console.WriteLine($"Created Dataset '{catalogue.Name}'");
        }

        private void UpdateDataset(Catalogue catalogue, HDRDatasetObject existingObject)
        {
            var url = $"{_url}/v2/teams/{_team}/datasets/{existingObject.id}?input_schema=HDRUK&input_version=4.0.0?input_schema=HDRUK&input_version=4.0.0";

            HDRDatasetUpdateBody body = new HDRDatasetUpdateBody(existingObject);
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IncludeFields = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            body.Update(catalogue,_inputJsonDetails);
            var jsonString = System.Text.Json.JsonSerializer.Serialize(body, serializeOptions);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = Task.Run(async () => await _client.PutAsync(url, httpContent)).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var contentString = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                Console.WriteLine($"unable to update Dataset '{catalogue.Name}': {contentString}. Will attempt to update in draft.");
                body.status = "DRAFT";
                jsonString = System.Text.Json.JsonSerializer.Serialize(body, serializeOptions);
                httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                response = Task.Run(async () => await _client.PutAsync(url, httpContent)).Result;
            }
            else
            {
                Console.WriteLine($"Updated Dataset '{catalogue.Name}'");
            }
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
                    UpdateDataset(catalogue, existingDataset);
                }
                else
                {
                    CreateDataset(catalogue);
                }
            }

        }

    }
}
