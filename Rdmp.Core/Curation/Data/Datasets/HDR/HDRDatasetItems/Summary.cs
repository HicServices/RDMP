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
    public class Summary
    {
        public string @abstract { get; set; }
        public string contactPoint { get; set; }
        public List<object> keywords { get; set; }
        public object doiName { get; set; }
        public string title { get; set; }
        public DataCustodian dataCustodian { get; set; }
        public int populationSize { get; set; }
        public object alternateIdentifiers { get; set; }
    }
}
