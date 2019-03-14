using CatalogueLibrary.Data;

namespace CatalogueLibrary.Repositories
{
    public interface IFilterContainerManager
    {
        IContainer[] GetSubContainers(IContainer container);
        void MakeIntoAnOrphan(IContainer container);
        IContainer GetParentContainerIfAny(IContainer container);
        void AddSubContainer(IContainer parent, IContainer child);
        IFilter[] GetFilters(IContainer container);
        void AddChild(IContainer container, IFilter filter);
    }
}