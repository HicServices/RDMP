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
    public class Coverage
    {
        public object pathway { get; set; }
        public string spatial { get; set; }
        public object followUp { get; set; }
        public object datasetCompleteness { get; set; }
        public List<string> materialType { get; set; }
        public int typicalAgeRangeMin { get; set; }
        public int typicalAgeRangeMax { get; set; }
    }
}
