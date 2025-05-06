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
    public class Visibility
    {
        public Visibility(string? key, ENGBWrapper description)
        {
            Key = key;
            Description = description;
        }
        public string? Key { get; set; }
        public ENGBWrapper Description { get; set; }
    }
}
