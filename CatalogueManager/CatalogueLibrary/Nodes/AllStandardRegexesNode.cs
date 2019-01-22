using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all centrally defined <see cref="StandardRegex"/>.  These are documented regular expressions which
    /// can be shared and reused between components (e.g. PipelineComponents).
    /// </summary>
    public class AllStandardRegexesNode:SingletonNode
    {
        public AllStandardRegexesNode() : base("Standard Regexes")
        {
        }
    }
}
