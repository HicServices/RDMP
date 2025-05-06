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
}
