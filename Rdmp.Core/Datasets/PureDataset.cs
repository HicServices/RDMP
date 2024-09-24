using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;

public class PureDataset: PluginDataset
{
    public int PureID { get; set; }
    public string UUID { get; set; }
    public string CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }

    public string PortalURL { get; set; }

    public string Version { get; set; }



    public PureDataset() { }
}
