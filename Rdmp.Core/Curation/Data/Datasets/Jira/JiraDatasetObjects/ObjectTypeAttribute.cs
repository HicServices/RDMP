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
    public class ObjectTypeAttribute
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public bool label { get; set; }
        public int type { get; set; }
        public DefaultType defaultType { get; set; }
        public bool editable { get; set; }
        public bool system { get; set; }
        public bool sortable { get; set; }
        public bool summable { get; set; }
        public bool indexed { get; set; }
        public int minimumCardinality { get; set; }
        public int maximumCardinality { get; set; }
        public bool removable { get; set; }
        public bool hidden { get; set; }
        public bool includeChildObjectTypes { get; set; }
        public bool uniqueAttribute { get; set; }
        public string options { get; set; }
        public int position { get; set; }
        public string description { get; set; }
        public ReferenceType referenceType { get; set; }
        public string referenceObjectTypeId { get; set; }
        public ReferenceObjectType referenceObjectType { get; set; }
        public string suffix { get; set; }
        public string regexValidation { get; set; }
        public string qlQuery { get; set; }
        public string iql { get; set; }
    }
}
