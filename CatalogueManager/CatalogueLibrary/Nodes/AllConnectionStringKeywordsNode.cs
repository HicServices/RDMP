using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all currently configured <see cref="ConnectionStringKeyword"/>.  These allow you to use custom keywords in your
    /// connection strings (e.g. change Port).
    /// </summary>
    public class AllConnectionStringKeywordsNode:SingletonNode
    {
        public AllConnectionStringKeywordsNode() : base("Connection String Keywords")
        {
        }
    }
}
