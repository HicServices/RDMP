using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Pure.PureDatasetItem
{
#nullable enable
    /// <summary>
    /// Link to a PURE object
    /// </summary>
    public class PureLink
    {

        public PureLink(int pureID, string url, string? alias, ENGBWrapper description, URITerm linkType)
        {
            PureID = pureID;
            Url = url;
            Alias = alias;
            Description = description;
            LinkType = linkType;
        }
        public int PureID { get; set; }
        public string Url { get; set; }

        public string? Alias { get; set; }
        public ENGBWrapper Description { get; set; }
        public URITerm LinkType { get; set; }

    }
}
