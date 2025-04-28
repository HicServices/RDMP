using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Jira.API
{
    public class MediaClientConfig
    {
        public string clientId { get; set; }
        public string issuer { get; set; }
        public string mediaBaseUrl { get; set; }
        public string mediaJwtToken { get; set; }
        public string fileId { get; set; }
        public int tokenLifespanInMinutes { get; set; }
    }
}
