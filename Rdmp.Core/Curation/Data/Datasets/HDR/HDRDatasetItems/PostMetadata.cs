using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    /// <summary>
    /// Used to map create to HDR API
    /// </summary>
    public class PostMetadata
    {

        public PostMetadata() { }
        public string identifier = "";
        public string version = "1.0.0";
        public List<Revision> revisions = new List<Revision>();
        public string modified;
        public string issued;
        public Summary summary = new Summary();
        public Accessibility accessibility = new Accessibility();
        public List<object> observations = new List<object>();
        public Provenance provenance = new Provenance();
    }
}
