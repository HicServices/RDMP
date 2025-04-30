using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR
{
    /// <summary>
    /// 
    /// </summary>
    public class HDRDataset : PluginDataset
    {
        public HDRDataset() : base() { }
        public HDRDataset(ICatalogueRepository catalogueRepository, string name) : base(catalogueRepository, name)
        {
        }

        public HDRDatasetItems.Data data { get; set; }

        public override string GetID()
        {
            return data.id.ToString(); 
        }
        public override string GetRemoteID()
        {
            return Url.Split('?')[0].Split('/').Last();
        }

        public string GetDOI()
        {
            var version = data?.versions?.FirstOrDefault();
            if (version != null) {
                return version.metadata?.metadata?.summary?.doiName?.ToString();
            }
            return null;
        }
    }
}
