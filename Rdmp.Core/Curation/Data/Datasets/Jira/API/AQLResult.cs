using Rdmp.Core.Curation.Data.Datasets.Jira.JiraDatasetObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Jira.API
{
    public class AQLResult
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public List<Value> values { get; set; }
        public List<ObjectTypeAttribute> objectTypeAttributes { get; set; }
        public bool hasMoreResults { get; set; }
        public bool last { get; set; }
        public bool isLast { get; set; }
    }
}
