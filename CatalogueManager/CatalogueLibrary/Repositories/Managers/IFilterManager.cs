using CatalogueLibrary.Data;

namespace CatalogueLibrary.Repositories.Managers
{
    public interface IFilterManager
    {
        IContainer[] GetSubContainers(IContainer container);
        void MakeIntoAnOrphan(IContainer container);
        IContainer GetParentContainerIfAny(IContainer container);
        void AddSubContainer(IContainer parent, IContainer child);
        IFilter[] GetFilters(IContainer container);
        void AddChild(IContainer container, IFilter filter);
    }
}