using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
    /// </summary>
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
}
