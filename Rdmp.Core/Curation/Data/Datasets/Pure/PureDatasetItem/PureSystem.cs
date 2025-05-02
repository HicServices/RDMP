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
    public class PureSystem
    {
        public PureSystem(string? uuid, string? systemName)
        {
            UUID = uuid;
            SystemName = systemName;
        }
        public string? SystemName { get; set; }
        public string? UUID { get; set; }
    }
}
