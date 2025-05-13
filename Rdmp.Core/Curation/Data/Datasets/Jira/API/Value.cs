using Rdmp.Core.Curation.Data.Datasets.Jira.JiraDatasetObjects;
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
    public class Value
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string label { get; set; }
        public string objectKey { get; set; }
        public Avatar avatar { get; set; }
        public ObjectType objectType { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public bool hasAvatar { get; set; }
        public long timestamp { get; set; }
        public List<JiraDatasetObjects.Attribute> attributes { get; set; }
        public Links _links { get; set; }
        public string name { get; set; }
    }

}
