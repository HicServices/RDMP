using System.Collections.Generic;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes which logical keyword to use to interspace IFilters (and sub IContainers) within an IContainer.  If you have an IContainer with only one IFilter in it then
    /// it makes no difference which FilterContainerOperation you specify.  Once an IContainer has more than one IFilter they will be seperated with the 
    /// FilterContainerOperation (AND / OR See SqlQueryBuilderHelper)
    /// </summary>
    public enum FilterContainerOperation
    {
        AND,
        OR
    }

    /// <summary>
    /// Interface for grouping IFilters (lines of WHERE Sql) into an AND/OR tree e.g. WHERE ('Hb is Tayside' OR 'Record is older than 5 months') AND 
    /// ('result is clinically significant').  Each subcontainer / IFilter are seperated with the Operation (See FilterContainerOperation) when building SQL
    /// (See SqlQueryBuilderHelper).
    /// </summary>
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
