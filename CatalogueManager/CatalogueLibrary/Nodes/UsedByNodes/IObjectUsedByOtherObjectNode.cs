using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes.UsedByNodes
{
    public interface IObjectUsedByOtherObjectNode:IMasqueradeAs
    {
        
    }

    public interface IObjectUsedByOtherObjectNode<out T> : IObjectUsedByOtherObjectNode
    {
        T User {get;}
    }

    public interface IObjectUsedByOtherObjectNode<T, out T2> : IObjectUsedByOtherObjectNode<T>
    {
        T2 ObjectBeingUsed { get; }
    }
}