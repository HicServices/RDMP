using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    /// <summary>
    /// Mapping from HDR API
    /// </summary>
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
}
