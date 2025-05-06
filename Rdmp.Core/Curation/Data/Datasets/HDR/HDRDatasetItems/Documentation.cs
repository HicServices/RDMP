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
    public class Documentation
    {
        public string description { get; set; }
        public object associatedMedia { get; set; }
        public string inPipeline { get; set; }
    }
}
