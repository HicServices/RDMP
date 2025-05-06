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
    public class StructuralMetadata
    {
        public List<object> tables { get; set; }
        [JsonIgnore]
        public object syntheticDataWebLink { get; set; }
    }
}
