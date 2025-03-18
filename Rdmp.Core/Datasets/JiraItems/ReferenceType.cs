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
    public class ReferenceType
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string color { get; set; }
        public string url16 { get; set; }
        public bool removable { get; set; }
    }
}
