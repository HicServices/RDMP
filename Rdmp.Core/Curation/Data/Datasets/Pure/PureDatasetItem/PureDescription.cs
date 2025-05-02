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
    public class PureDescription
    {
        public int? PureId { get; set; }
        public ENGBWrapper? Value { get; set; }

        public URITerm Term { get => new URITerm("/dk/atira/pure/dataset/descriptions/datasetdescription", new ENGBWrapper("Description")); }
    }
}
