using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Dynamically created collection of 'servers' produced from all currently configured <see cref="TableInfo"/>.
    /// </summary>
    public class AllServersNode:SingletonNode
    {
        public AllServersNode():base("Data Repository Servers")
        {
            
        }
    }
}