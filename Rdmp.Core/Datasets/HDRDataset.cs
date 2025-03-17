// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Text.Json;
using System.Globalization;
namespace Rdmp.Core.Datasets;
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Access
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

public class Accessibility
{
    public Access access { get; set; }
    public Usage usage { get; set; }
    public FormatAndStandards formatAndStandards { get; set; }
}

public class Coverage
{
    public object pathway { get; set; }
    public string spatial { get; set; }
    public object followUp { get; set; }
    public object datasetCompleteness { get; set; }
    public List<string> materialType { get; set; }
    public int typicalAgeRangeMin { get; set; }
    public int typicalAgeRangeMax { get; set; }
}

public class Data
{
    public int id { get; set; }
    public object mongo_object_id { get; set; }
    public object mongo_id { get; set; }
    public object mongo_pid { get; set; }
    public object datasetid { get; set; }
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
    public List<object> collections { get; set; }
    public List<Version> versions { get; set; }
    public Team team { get; set; }
}

public class DataCustodian
{
    public string name { get; set; }
    public string identifier { get; set; }
    public string contactPoint { get; set; }
    public object logo { get; set; }
    public object description { get; set; }
    public object memberOf { get; set; }
}

public class Documentation
{
    public string description { get; set; }
    public object associatedMedia { get; set; }
    public string inPipeline { get; set; }
}

public class EnrichmentAndLinkage
{
    public List<object> tools { get; set; }
    public List<object> investigations { get; set; }
    public List<object> publicationAboutDataset { get; set; }
    public List<object> publicationUsingDataset { get; set; }

    [JsonIgnore]
    public object derivedFrom { get; set; }

    [JsonIgnore]
    public object isPartOf { get; set; }

    [JsonIgnore]
    public object linkableDatasets { get; set; }

    [JsonIgnore]
    public object similarToDatasets { get; set; }
}

public class FormatAndStandards
{
    public List<string> conformsTo { get; set; }
    public List<string> vocabularyEncodingScheme { get; set; }
    public List<string> language { get; set; }
    public List<string> format { get; set; }
}

public class Metadata
{
    public Metadata metadata { get; set; }
    public string identifier { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
    public DateTime issued { get; set; }
    
    [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
    public DateTime modified { get; set; }
    public List<Revision> revisions { get; set; }
    public string version { get; set; }
    public Summary summary { get; set; }
    public Documentation documentation { get; set; }
    public Coverage coverage { get; set; }
    public Provenance provenance { get; set; }
    public Accessibility accessibility { get; set; }
    public EnrichmentAndLinkage enrichmentAndLinkage { get; set; }
    public List<object> observations { get; set; }
    public StructuralMetadata structuralMetadata { get; set; }
    public object demographicFrequency { get; set; }
    public object omics { get; set; }
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

public class Origin
{
    public List<object> purpose { get; set; }
    public List<object> source { get; set; }
    public List<object> collectionSource { get; set; }
    public List<string> datasetType { get; set; }
    public List<string> datasetSubType { get; set; }
    public string imageContrast { get; set; }
}

public class Provenance
{
    public Origin origin { get; set; }
    public Temporal temporal { get; set; }
}

public class Revision
{
    public string version { get; set; }
    public string url { get; set; }
}

public class HDRDataset:PluginDataset
{
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
    public List<object> tables { get; set; }
    [JsonIgnore]
    public object syntheticDataWebLink { get; set; }
}

public class Summary
{
    public string @abstract { get; set; }
    public string contactPoint { get; set; }
    public List<object> keywords { get; set; }
    public object doiName { get; set; }
    public string title { get; set; }
    public DataCustodian dataCustodian { get; set; }
    public int populationSize { get; set; }
    public object alternateIdentifiers { get; set; }
}

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
       return  DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.000000Z", CultureInfo.InvariantCulture));
    }
}

public class CustomDateTimeConverterThreeMilliseconds : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.InvariantCulture));
    }
}

public class Team
{
    public int id { get; set; }
    public string pid { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverter))]
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
    [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]

    public DateTime? endDate { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
    public DateTime startDate { get; set; }
    public string timeLag { get; set; }
    public string publishingFrequency { get; set; }
    public object distributionReleaseDate { get; set; }
}

public class Usage
{
    public List<string> dataUseLimitation { get; set; }
    public object resourceCreator { get; set; }
    public List<string> dataUseRequirements { get; set; } = [];
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
    public List<object> reduced_linked_dataset_versions { get; set; }
}

