using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) implementation of IContainer.  
    /// 
    /// IContainers are collections of subcontainers and WHERE statements e.g. 
    /// (
    ///     --age is above 5
    ///     Age > 5
    /// AND
    ///    --name is bob
    ///     Name like 'Bob%'
    /// )
    /// 
    /// Most IContainers come from the DataCatalogue/DataExport Database and are a hierarchical list of filters the user wants to use to create a query.  But sometimes IN CODE,
    /// we want to create an impromptu container and ram some additional filters we have either also invented or have pulled out of the Catalogue into the container.  This 
    /// Class lets you do that, it creates a 'memory only' container which cannot be saved/deleted etc but can be used in query building by ISqlQueryBuilders.
    /// 
    /// See also SpontaneouslyInventedFilter
    /// </summary>
    public class SpontaneouslyInventedFilterContainer:SpontaneousObject,IContainer
    {
        List<IContainer> _subContainers = new List<IContainer>();
        List<IFilter> _filters = new List<IFilter>();

        public SpontaneouslyInventedFilterContainer(IContainer[] subContainersIfAny, IFilter[] filtersIfAny, FilterContainerOperation operation)
        {
            if (subContainersIfAny != null)
                _subContainers.AddRange(subContainersIfAny);

            if (filtersIfAny != null)
                _filters.AddRange(filtersIfAny);

            Operation = operation;
        }

        /// <inheritdoc/>
        public FilterContainerOperation Operation { get; set; }

        public IContainer GetParentContainerIfAny()
        {
            return null;
        }

        public IContainer[] GetSubContainers()
        {
            return _subContainers.ToArray();
        }

        public IFilter[] GetFilters()
        {
            return _filters.ToArray();
        }

        public void AddChild(IContainer child)
        {
            _subContainers.Add(child);
        }

        public void AddChild(IFilter filter)
        {
            _filters.Add(filter);
        }

        public void MakeIntoAnOrphan()
        {
            throw new NotSupportedException();
        }

        public IContainer GetRootContainerOrSelf()
        {
            return new ContainerHelper().GetRootContainerOrSelf(this);
        }

        public List<IFilter> GetAllFiltersIncludingInSubContainersRecursively()
        {
            return new ContainerHelper().GetAllFiltersIncludingInSubContainersRecursively(this);
        }

        public Catalogue GetCatalogueIfAny()
        {
            return null;
        }

        public List<IContainer> GetAllSubContainersRecursively()
        {
            return new ContainerHelper().GetAllSubContainersRecursively(this);
        }
    }
}