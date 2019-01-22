namespace CatalogueLibrary.Nodes.SharingNodes
{
    /// <summary>
    /// Collection of all ObjectExport and ObjectImport references (for sharing RDMP objects with other remote deployment instances of RDMP).
    /// </summary>
    public class AllObjectSharingNode:SingletonNode
    {
        public AllObjectSharingNode() : base("Object Sharing")
        {
        }
    }
}
