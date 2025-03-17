using Amazon.S3.Model;
using Newtonsoft.Json;
using Rdmp.Core.Datasets.JiraItems;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets
{
    /// <summary>
    /// Mapping of Jira API Asset object to C#
    /// </summary>
    public class JiraDataset : PluginDataset
    {
        public JiraDataset()
        {

        }
        public JiraDataset(ICatalogueRepository catalogueRepository, string name) : base(catalogueRepository, name)
        {
        }

        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string label { get; set; }
        public string objectKey { get; set; }
        public ObjectType objectType { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public List<JiraItems.Attribute> attributes { get; set; }
        public ExtendedInfo extendedInfo { get; set; }
        public Links _links { get; set; }
        public string name { get; set; }

    }
}
