using Rdmp.Core.Datasets.HDRItems;
using Rdmp.Core.Repositories;
namespace Rdmp.Core.Datasets;

/// <summary>
/// HDR API Dataset Object mapping
/// </summary>
public class HDRDataset:PluginDataset
{

    public HDRDataset():base() { }
    public HDRDataset(ICatalogueRepository catalogueRepository, string name) : base(catalogueRepository, name)
    {
    }

    public Data data { get; set; }
}





