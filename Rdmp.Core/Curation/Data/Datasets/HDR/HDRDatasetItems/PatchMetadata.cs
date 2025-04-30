using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    public class PatchMetadata
    {

        public string schemaModel { get; set; }
        public string schemaVersion { get; set; }
        public PatchSubMetadata metadata { get; set; }


        public PatchMetadata() { }
        public PatchMetadata(HDRDatasetItems.Metadata existingMetadata)
        {

            metadata = existingMetadata.metadata != null ? new PatchSubMetadata(existingMetadata.metadata) : null;
            schemaModel = "HDRUK";
            schemaVersion = "3.0.0";
        }
    }
}
