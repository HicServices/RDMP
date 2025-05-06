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
    public class Provenance
    {
        public Origin origin { get; set; }
        public Temporal temporal { get; set; }
    }
}
