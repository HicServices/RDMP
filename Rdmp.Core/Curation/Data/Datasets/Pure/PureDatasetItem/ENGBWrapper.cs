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
    public class ENGBWrapper
    {
        public ENGBWrapper(string? text) { En_GB = text; }
        public string? En_GB { get; set; }
    }
}
