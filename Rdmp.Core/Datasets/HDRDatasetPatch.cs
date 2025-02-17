using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;
public class PatchMetadata
{

    public string schemaModel = "HDRUK";
    public string schemaVersion = "3.0.0";
    public PatchSubMetadata metadata { get; set; }


    public PatchMetadata() { }
    public PatchMetadata(Metadata existingMetadata)
    {

        metadata = existingMetadata.metadata!=null?new PatchSubMetadata(existingMetadata.metadata):null;
    }
}

public class PatchSubMetadata
{
    public object observations { get; set; }
    public object coverage { get; set; }
    public object structuralMetadata { get; set; }
    public object enrichmentAndLinkage { get; set; }
    public object accessibility { get; set; }

    public object provenance { get; set; }
    public object documentation { get; set; }
    public Summary summary { get; set; }
    public PatchSubMetadata(Metadata existingMetadata)
    {
        accessibility = existingMetadata.accessibility;
        observations = existingMetadata.observations;
        coverage = existingMetadata.coverage;
        structuralMetadata = existingMetadata.structuralMetadata;
        enrichmentAndLinkage = existingMetadata?.original_metadata?.enrichmentAndLinkage;
        provenance = existingMetadata.provenance;
        documentation = existingMetadata.original_metadata?.documentation;
        summary = existingMetadata.summary;
    }
}

public class HDRDatasetPatch
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
    public List<Collection> collections { get; set; }
    public Team team { get; set; }
    public PatchMetadata metadata { get; set; }


    public HDRDatasetPatch() { }
    public HDRDatasetPatch(HDRDataset existingDataset)
    {
        id = existingDataset.data.id;
        mongo_object_id = existingDataset.data.mongo_object_id;
        mongo_id = existingDataset.data.mongo_id;
        mongo_pid = existingDataset.data.mongo_pid;
        datasetid = existingDataset.data.datasetid;
        pid = existingDataset.data.pid;
        source = existingDataset.data.source;
        discourse_topic_id =existingDataset.data.discourse_topic_id;
        is_cohort_discovery = existingDataset.data.is_cohort_discovery;
        commercial_use = existingDataset.data.commercial_use;
        state_id = existingDataset.data.state_id;
        uploader_id = existingDataset.data.uploader_id;
        metadataquality_id =    existingDataset.data.metadataquality_id;
        user_id = existingDataset.data.user_id;
        team_id = existingDataset.data.team_id;
        views_count = existingDataset.data.views_count;
        views_prev_count = existingDataset.data.views_prev_count;
        has_technical_details = existingDataset.data.has_technical_details;
        created = existingDataset.data.created;
        updated = existingDataset.data.updated;
        submitted = existingDataset.data.submitted;
        published = existingDataset.data.published;
        created_at = existingDataset.data.created_at;
        updated_at = existingDataset.data.updated_at;
        deleted_at = existingDataset.data.deleted_at;
        create_origin = existingDataset.data.create_origin;
        status = existingDataset.data.status;
        durs_count = existingDataset.data.durs_count;
        publications_count = existingDataset.data.publications_count;
        tools_count = existingDataset.data.tools_count;
        collections_count = existingDataset.data.collections_count;
        spatialCoverage = existingDataset.data.spatialCoverage;
        durs = existingDataset.data.durs;
        publications = existingDataset.data.publications;
        named_entities = existingDataset.data.named_entities;
        collections = existingDataset.data.collections;
        team = existingDataset.data.team;
        metadata = new PatchMetadata(existingDataset.data.metadata);
    }
}

