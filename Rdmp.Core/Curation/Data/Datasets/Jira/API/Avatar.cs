using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Jira.API
{
    /// <summary>
    /// 
    /// </summary>
    public class Avatar
    {
        public string workspaceId { get; set; }
        public string url16 { get; set; }
        public string url48 { get; set; }
        public string url72 { get; set; }
        public string url144 { get; set; }
        public string url288 { get; set; }
        public string objectId { get; set; }
        public MediaClientConfig mediaClientConfig { get; set; }
    }
}
