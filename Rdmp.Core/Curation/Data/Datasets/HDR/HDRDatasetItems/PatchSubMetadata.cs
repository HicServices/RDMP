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
    /// 
    /// </summary>
    public class PatchSubMetadata
    {
        public object observations { get; set; }
        public Coverage coverage { get; set; }
        public object structuralMetadata { get; set; }
        public object enrichmentAndLinkage { get; set; }
        public Accessibility accessibility { get; set; }

        public string identifier { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]

        public DateTime issued { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]

        public DateTime modified { get; set; }

        public Provenance provenance { get; set; }
        public object documentation { get; set; }
        public Summary summary { get; set; }
        public List<Revision> revisions { get; set; }

        public string version { get; set; }

        public PatchSubMetadata(HDRDatasetItems.Metadata existingMetadata)
        {
            accessibility = existingMetadata.accessibility;
            observations = existingMetadata.observations;
            coverage = existingMetadata.coverage;
            structuralMetadata = existingMetadata.structuralMetadata;
            enrichmentAndLinkage = existingMetadata.enrichmentAndLinkage;
            provenance = existingMetadata.provenance;
            documentation = existingMetadata.documentation;
            summary = existingMetadata.summary;
            identifier = existingMetadata.identifier;
            issued = existingMetadata.issued;
            modified = existingMetadata.modified;
            revisions = existingMetadata.revisions;
            version = existingMetadata.version;
        }
    }
}
