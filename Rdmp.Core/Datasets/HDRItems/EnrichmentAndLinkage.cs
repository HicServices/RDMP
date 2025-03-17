using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
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
