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
    public class ObjectAttributeValue
    {
        public object value { get; set; }
        public object searchValue { get; set; }
        public bool referencedType { get; set; }
        public string displayValue { get; set; }
        public ReferencedObject referencedObject { get; set; }
    }
}
