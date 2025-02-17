using Amazon.S3.Model;
using Rdmp.Core.Datasets.HDRItems;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets
{
    public class HDRDataset: PluginDataset
    {
        //public List<HDRSummary> Versions { get; set; }
        //public string Id { get; set; }
        //public string mongo_object_id { get; set; }

        public string message { get; set; }
        public Data data { get; set; }

        public HDRDataset() { }
        public HDRDataset(ICatalogueRepository catalogueRepository, string name) : base(catalogueRepository, name) { }
        public HDRDataset(ICatalogueRepository repository, DbDataReader r) : base(repository, r) { }

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Access
    {
        public string accessRights { get; set; }
        public object jurisdiction { get; set; } //string or list
        public string accessService { get; set; }
        public object dataProcessor { get; set; }
        public string dataController { get; set; }
        public string deliveryLeadTime { get; set; }
        public object accessRequestCost { get; set; }
        public string accessServiceCategory { get; set; }
    }

    //they've borked the schema, so access object is different in versions and original metadata...

    public class Accessibility
    {
        public Usage usage { get; set; }
        public Access access { get; set; }
        public FormatAndStandards formatAndStandards { get; set; }
    }

    public class Collection
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image_link { get; set; }
        public bool enabled { get; set; }
        public int @public { get; set; }
        public int counter { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public string mongo_object_id { get; set; }
        public string mongo_id { get; set; }
        public object updated_on { get; set; }
        public int team_id { get; set; }
        public string status { get; set; }
        public List<int> dataset_version_ids { get; set; }
    }

    public class Column
    {
        public string name { get; set; }
        public object values { get; set; }
        public string dataType { get; set; }
        public bool sensitive { get; set; }
        public string description { get; set; }
    }

    public class Coverage
    {
        public object pathway { get; set; }
        public string spatial { get; set; }
        public string followUp { get; set; }
        public string typicalAgeRange { get; set; }
        public object datasetCompleteness { get; set; }
        public List<string> materialType { get; set; }
        public int typicalAgeRangeMax { get; set; }
        public int typicalAgeRangeMin { get; set; }
    }

    public class Data
    {
        public int id { get; set; }
        public string mongo_object_id { get; set; }
        public string mongo_id { get; set; }
        public string mongo_pid { get; set; }
        public string datasetid { get; set; }
        public string pid { get; set; }
        public object source { get; set; }
        public int discourse_topic_id { get; set; }
        public bool is_cohort_discovery { get; set; }
        public int commercial_use { get; set; }
        public int state_id { get; set; }
        public int uploader_id { get; set; }
        public int metadataquality_id { get; set; }
        public int user_id { get; set; }
        public int team_id { get; set; }
        public int views_count { get; set; }
        public int views_prev_count { get; set; }
        public int has_technical_details { get; set; }
        public string created { get; set; }
        public string updated { get; set; }
        public string submitted { get; set; }
        public object published { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public string create_origin { get; set; }
        public string status { get; set; }
        public int durs_count { get; set; }
        public int publications_count { get; set; }
        public int tools_count { get; set; }
        public int collections_count { get; set; }
        public List<SpatialCoverage> spatialCoverage { get; set; }
        public List<object> durs { get; set; }
        public List<object> publications { get; set; }
        public List<NamedEntity> named_entities { get; set; }
        public List<Collection> collections { get; set; }
        public List<Version> versions { get; set; }
        public Team team { get; set; }

        public Metadata metadata { get; set; }
    }

    public class DataCustodian
    {
        public object logo { get; set; }
        public string name { get; set; }
        public string memberOf { get; set; }
        public string identifier { get; set; }
        public object description { get; set; }
        public object contactPoint { get; set; }
    }

    public class DatasetLinkage
    {
        public object isPartOf { get; set; }
        public object isMemberOf { get; set; }
        public object isDerivedFrom { get; set; }
        public object linkedDatasets { get; set; }
    }

    public class Documentation
    {
        public string inPipeline { get; set; }
        public string description { get; set; }
        public object associatedMedia { get; set; }
    }

    public class EnrichmentAndLinkage
    {
        public object tools { get; set; }
        public object isPartOf { get; set; }
        public object derivedFrom { get; set; }
        public object investigations { get; set; }
        public object linkableDatasets { get; set; }
        public object similarToDatasets { get; set; }
        public object publicationAboutDataset { get; set; }
        public object publicationUsingDataset { get; set; }
    }

    public class FormatAndStandards
    {
        public string formats { get; set; }
        public string languages { get; set; }
        public object conformsTo { get; set; }
        public string vocabularyEncodingSchemes { get; set; }
        public List<string> format { get; set; }
        public List<string> language { get; set; }
        public List<string> vocabularyEncodingScheme { get; set; }
    }

    public class Linkage
    {
        public object tools { get; set; }
        public object dataUses { get; set; }
        public object isReferenceIn { get; set; }
        public DatasetLinkage datasetLinkage { get; set; }
        public object investigations { get; set; }
        public object associatedMedia { get; set; }
        public object isGeneratedUsing { get; set; }
        public object syntheticDataWebLink { get; set; }
        public object publicationAboutDataset { get; set; }
        public object publicationUsingDataset { get; set; }
    }

    public class Metadata
    {
        public Metadata metadata { get; set; }
        public string gwdmVersion { get; set; }
        public OriginalMetadata original_metadata { get; set; }
        public object omics { get; set; }
        public Linkage linkage { get; set; }
        public Summary summary { get; set; }
        public Coverage coverage { get; set; }
        public Required required { get; set; }
        public Provenance provenance { get; set; }
        public List<Observation> observations { get; set; }
        public Accessibility accessibility { get; set; }
        public List<StructuralMetadata> structuralMetadata { get; set; }
        public object demographicFrequency { get; set; }
        public List<TissuesSampleCollection> tissuesSampleCollection { get; set; }
    }

    public class NamedEntity
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public List<int> dataset_version_ids { get; set; }
    }

    public class Observation
    {
        public string observedNode { get; set; }
        public int measuredValue { get; set; }
        public string observationDate { get; set; }
        public string measuredProperty { get; set; }
        public string disambiguatingDescription { get; set; }
    }

    public class Origin
    {
        public object source { get; set; } //string or list
        public object purpose { get; set; } //string or list
        public string imageContrast { get; set; }
        public string collectionSituation { get; set; }
        public List<string> datasetType { get; set; }
        public List<string> datasetSubType { get; set; }
        public List<string> collectionSource { get; set; }
    }

    public class OriginalMetadata
    {
        public object omics { get; set; }
        public DateTime issued { get; set; }
        public Summary summary { get; set; }
        public string version { get; set; }
        public Coverage coverage { get; set; }
        public DateTime modified { get; set; }
        public List<object> revisions { get; set; }
        public string identifier { get; set; }
        public Provenance provenance { get; set; }
        public List<Observation> observations { get; set; }
        public Accessibility accessibility { get; set; }
        public Documentation documentation { get; set; }
        public StructuralMetadata structuralMetadata { get; set; }
        public object demographicFrequency { get; set; }
        public EnrichmentAndLinkage enrichmentAndLinkage { get; set; }
    }

    public class Provenance
    {
        public Origin origin { get; set; }
        public Temporal temporal { get; set; }
    }

    public class Publisher
    {
        public string name { get; set; }
        public string gatewayId { get; set; }
    }

    public class Required
    {
        public DateTime issued { get; set; }
        public string version { get; set; }
        public DateTime modified { get; set; }
        public string gatewayId { get; set; }
        public List<Revision> revisions { get; set; }
        public string gatewayPid { get; set; }
    }

    public class ResourceCreator
    {
        public string name { get; set; }
        public object rorId { get; set; }
        public object gatewayId { get; set; }
    }

    public class Revision
    {
        public string url { get; set; }
        public string version { get; set; }
    }

    public class Root
    {
        public string message { get; set; }
        public Data data { get; set; }
    }

    public class SpatialCoverage
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string region { get; set; }
        public bool enabled { get; set; }
        public List<int> dataset_version_ids { get; set; }
    }

    public class StructuralMetadata
    {
        public List<Table> tables { get; set; }
        public object syntheticDataWebLink { get; set; }
        public string name { get; set; }
        public List<Column> columns { get; set; }
        public string description { get; set; }
    }

    public class Summary
    {
        public string title { get; set; }
        public object doiName { get; set; }
        public string @abstract { get; set; }
        //public string keywords { get; set; }
        public Publisher publisher { get; set; }
        public string inPipeline { get; set; }
        public string shortTitle { get; set; }
        public string datasetType { get; set; }
        public string description { get; set; }
        public string contactPoint { get; set; }
        public string datasetSubType { get; set; }
        public int populationSize { get; set; }
        public object controlledKeywords { get; set; }
        public DataCustodian dataCustodian { get; set; }
        public object alternateIdentifiers { get; set; }
    }

    public class Table
    {
        public string name { get; set; }
        public List<Column> columns { get; set; }
        public string description { get; set; }
    }

    public class Team
    {
        public int id { get; set; }
        public string pid { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public string name { get; set; }
        public bool enabled { get; set; }
        public bool allows_messaging { get; set; }
        public bool workflow_enabled { get; set; }
        public bool access_requests_management { get; set; }
        public bool uses_5_safes { get; set; }
        public bool is_admin { get; set; }
        public string team_logo { get; set; }
        public string member_of { get; set; }
        public object contact_point { get; set; }
        public string application_form_updated_by { get; set; }
        public string application_form_updated_on { get; set; }
        public string mongo_object_id { get; set; }
        public bool notification_status { get; set; }
        public bool is_question_bank { get; set; }
        public bool is_provider { get; set; }
        public object url { get; set; }
        public object introduction { get; set; }
        public object dar_modal_header { get; set; }
        public object dar_modal_content { get; set; }
        public object dar_modal_footer { get; set; }
        public bool is_dar { get; set; }
        public object service { get; set; }
    }

    public class Temporal
    {
        public string endDate { get; set; }
        public string timeLag { get; set; }
        public string startDate { get; set; }
        public string accrualPeriodicity { get; set; }
        public string distributionReleaseDate { get; set; }
        public string publishingFrequency { get; set; }
    }

    public class TissuesSampleCollection
    {
        public object id { get; set; }
        public object disease { get; set; }
        public string materialType { get; set; }
        public object collectionType { get; set; }
        public object dataCategories { get; set; }
        public object sampleAgeRange { get; set; }
        public object accessConditions { get; set; }
        public object storageTemperature { get; set; }
        public object tissueSampleMetadata { get; set; }
    }

    public class Usage
    {
        public ResourceCreator resourceCreator { get; set; }
        public object dataUseLimitation { get; set; } //string or list<string>
        public string dataUseRequirement { get; set; }
        public List<string> dataUseRequirements { get; set; }
    }

    public class Version
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public int dataset_id { get; set; }
        public Metadata metadata { get; set; }
        public int version { get; set; }
        public object provider_team_id { get; set; }
        public object application_type { get; set; }
        public List<object> linked_dataset_versions { get; set; }
    }


}
