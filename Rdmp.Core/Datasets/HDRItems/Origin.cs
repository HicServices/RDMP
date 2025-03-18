using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
    /// </summary>
    public class Origin
    {
        public List<object> purpose { get; set; }
        public List<object> source { get; set; }
        public List<object> collectionSource { get; set; }
        public List<string> datasetType { get; set; }
        public List<string> datasetSubType { get; set; }
        public string imageContrast { get; set; }
    }
}
