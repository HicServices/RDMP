using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NPOI.HSSF.Util.HSSFColor;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    /// <summary>
    /// 
    /// </summary>
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
        public List<HDRDatasetItems.Version> versions { get; set; }
        public Team team { get; set; }
    }
}
