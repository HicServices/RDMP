using CatalogueLibrary.Data.Remoting;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all <see cref="RemoteRDMP"/>s you have configured.
    /// </summary>
    public class AllRDMPRemotesNode : SingletonNode
    {
        public AllRDMPRemotesNode()
            : base("Remote RDMP Instances")
        {
        }
    }
}