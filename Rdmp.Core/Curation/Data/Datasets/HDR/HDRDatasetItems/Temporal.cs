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
    public class Temporal
    {
        [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]

        public DateTime? endDate { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverterThreeMilliseconds))]
        public DateTime startDate { get; set; }
        public string timeLag { get; set; }
        public string publishingFrequency { get; set; }
        public object distributionReleaseDate { get; set; }
    }
}
