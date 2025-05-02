using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Pure.PureDatasetItem
{
# nullable enable
    /// <summary>
    /// Internal PURE system class
    /// </summary>
    public class PurePerson
    {
        public string? TypeDiscriminator { get; set; }
        public int? PureId { get; set; }

        public PureName? Name { get; set; }
        public URITerm? Role { get; set; }

        public List<PureSystem>? Organizations { get; set; }
    }
}
