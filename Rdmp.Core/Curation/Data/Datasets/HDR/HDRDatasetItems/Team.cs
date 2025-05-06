using Rdmp.Core.Curation.Data.Datasets.HDR.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    /// <summary>
    /// Mapping from HDR API
    /// </summary>
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
}
