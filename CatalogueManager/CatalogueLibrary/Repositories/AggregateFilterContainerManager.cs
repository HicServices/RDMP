using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;

namespace CatalogueLibrary.Repositories
{
    class AggregateFilterContainerManager : IFilterContainerManager
    {
        private readonly CatalogueRepository _catalogueRepository;

        public AggregateFilterContainerManager(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public IContainer[] GetSubContainers(IContainer container)
        {
            return 
                _catalogueRepository.SelectAll<AggregateFilterContainer>("SELECT AggregateFilterContainer_ChildID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ParentID=" + container.ID,
                "AggregateFilterContainer_ChildID").ToArray();
        }

        public void MakeIntoAnOrphan(IContainer container)
        {
            _catalogueRepository.Delete("DELETE FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID = @AggregateFilterContainer_ChildID", new Dictionary<string, object>
            {
                {"AggregateFilterContainer_ChildID", container.ID}
            });
        }

        public IContainer GetParentContainerIfAny(IContainer container)
        {
            return _catalogueRepository.SelectAll<AggregateFilterContainer>("SELECT AggregateFilterContainer_ParentID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID=" + container.ID,
                "AggregateFilterContainer_ParentID").SingleOrDefault();
        }

        public IFilter[] GetFilters(IContainer container)
        {
            return _catalogueRepository.GetAllObjectsWhere<AggregateFilter>("FilterContainer_ID", container.ID).ToArray();
        }

        public void AddSubContainer(IContainer parent, IContainer child)
        {
            _catalogueRepository.Insert(
                "INSERT INTO AggregateFilterSubContainer(AggregateFilterContainer_ParentID,AggregateFilterContainer_ChildID) VALUES (@AggregateFilterContainer_ParentID,@AggregateFilterContainer_ChildID)",
                new Dictionary<string, object>
                {
                    {"AggregateFilterContainer_ParentID", parent.ID},
                    {"AggregateFilterContainer_ChildID", child.ID}
                });
        }
        
        public void AddChild(IContainer container, IFilter filter)
        {
            filter.FilterContainer_ID = container.ID;
            filter.SaveToDatabase();
        }
    }

    
}