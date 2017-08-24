using System.Collections.Generic;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    public enum FilterContainerOperation
    {
        AND,
        OR
    }


    public interface IContainer:IDeleteable,ISaveable,IMapsDirectlyToDatabaseTable
    {
        FilterContainerOperation Operation { get; set; }
        IContainer GetParentContainerIfAny();
        IContainer[] GetSubContainers();
        IFilter[] GetFilters();

        void AddChild(IContainer child);
        void AddChild(IFilter filter);
        void MakeIntoAnOrphan();

        //ContainerHelper implements these if you are writting a sane IContainer you can instantiate the helper and use it's methods
        IContainer GetRootContainerOrSelf();
        List<IContainer> GetAllSubContainersRecursively();
        List<IFilter> GetAllFiltersIncludingInSubContainersRecursively();

        Catalogue GetCatalogueIfAny();
    }
}
