using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets
{
    public class HDRDatasetPatch
    {
        public HDRDatasetPatch() { }

        public HDRDatasetPatch(HDRDataset dataset) {

            id = dataset.data.id;
            mongo_object_id = dataset.data.mongo_object_id;
            mongo_id = dataset.data.mongo_id;
            datasetid = dataset.data.datasetid;
            pid = dataset.data.pid;
            source = dataset.data.source;
            discourse_topic_id = dataset.data.discourse_topic_id;
            is_cohort_discovery = dataset.data.is_cohort_discovery;
            commercial_use = dataset.data.commercial_use;
            state_id = dataset.data.state_id;
            uploader_id = dataset.data.uploader_id;
            metadataquality_id = dataset.data.metadataquality_id;
            user_id = dataset.data.user_id;
            team_id = dataset.data.team_id;
            views_count = dataset.data.views_count;
            views_prev_count = dataset.data.views_prev_count;
            has_technical_details = dataset.data.has_technical_details;
            created = dataset.data.created;
            updated = dataset.data.updated;
            submitted = dataset.data.submitted;
            published = dataset.data.publications;
            created_at = dataset.data.created_at;
            updated_at = dataset.data.updated_at;
            deleted_at = dataset.data.deleted_at;
            create_origin = dataset.data.create_origin;
            status = dataset.data.status;
            durs_count = dataset.data.durs_count;
            publications_count  = dataset.data.publications_count;
            tools_count = dataset.data.tools_count;
            collections_count = dataset.data.collections_count;
            spatialCoverage = dataset.data.spatialCoverage;
            durs = dataset.data.durs;
            publications = dataset.data.publications;
            named_entities = dataset.data.named_entities;
            collections =   dataset.data.collections;
            team   = dataset.data.team;
            metadata = new PatchMetadata(dataset.data.versions.First().metadata);
        }

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
        public Team team { get; set; }

        public PatchMetadata metadata { get; set; }
    }

    public class PatchMetadataMetadata
    {
        public Accessibility accessibility { get; set; }

        public PatchMetadataMetadata(Metadata _metadata)
        {
            accessibility = _metadata.accessibility;
        }

    }

    public class PatchMetadata
    {
        public PatchMetadataMetadata metadata { get; set; }

        public PatchMetadata(Metadata _metadata) {
        
            metadata = new PatchMetadataMetadata(_metadata);
        }
    }
}
