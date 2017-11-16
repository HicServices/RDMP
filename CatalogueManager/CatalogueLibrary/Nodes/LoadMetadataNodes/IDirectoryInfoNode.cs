using System.IO;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public interface IDirectoryInfoNode
    {
        DirectoryInfo GetDirectoryInfoIfAny();
    }
}