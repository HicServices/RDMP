using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Jira.JiraDatasetObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class ReferencedObject
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string label { get; set; }
        public string objectKey { get; set; }
        public ObjectType objectType { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public bool hasAvatar { get; set; }
        public object timestamp { get; set; }
        public Links _links { get; set; }
        public string name { get; set; }
    }
}
