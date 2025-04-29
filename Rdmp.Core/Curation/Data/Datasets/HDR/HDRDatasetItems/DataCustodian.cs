using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    /// <summary>
    /// 
    /// </summary>
    public class DataCustodian
    {
        public string name { get; set; }
        public string identifier { get; set; }
        public string contactPoint { get; set; }
        public object logo { get; set; }
        public object description { get; set; }
        public object memberOf { get; set; }
    }
}
