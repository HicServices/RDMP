using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets.JiraItems
{
    /// <summary>
    /// 
    /// </summary>
    public class Attribute
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public ObjectTypeAttribute objectTypeAttribute { get; set; }
        public string objectTypeAttributeId { get; set; }
        public List<ObjectAttributeValue> objectAttributeValues { get; set; }
        public string objectId { get; set; }
    }
}
