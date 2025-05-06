using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Pure.PureDatasetItem
{
#nullable enable
    /// <summary>
    /// Internal PURE system class
    /// </summary>
    public class Geolocation
    {
        public ENGBWrapper? GeographicalCoverage { get; set; }
        public string? Point { get; set; }

        public string? Polygon { get; set; }
    }
}
