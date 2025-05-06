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
    public class Workflow
    {
        public string? Step { get; set; }
        public ENGBWrapper? Description { get; set; }
    }
}
