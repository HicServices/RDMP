using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Jira.API
{
    public class Entry
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public int objectCount { get; set; }
        public string objectSchemaId { get; set; }
        public bool inherited { get; set; }
        public bool abstractObjectType { get; set; }
        public bool parentObjectTypeInherited { get; set; }
        public string description { get; set; }
    }
}
