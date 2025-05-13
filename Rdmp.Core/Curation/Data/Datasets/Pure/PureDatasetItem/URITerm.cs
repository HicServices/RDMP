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
    public class URITerm
    {
        public URITerm(string? uri, ENGBWrapper enGBWrapper)
        {
            URI = uri;
            Term = enGBWrapper;
        }
        public string? URI { get; set; }
        public ENGBWrapper Term { get; set; }
    }
}
