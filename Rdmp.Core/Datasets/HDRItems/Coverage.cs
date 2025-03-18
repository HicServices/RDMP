using System.Collections.Generic;

namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
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
