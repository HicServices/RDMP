using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Repositories
{
    class DataExportFilterContainerManager : IFilterContainerManager
    {
        private readonly IDataExportRepository _dataExportRepository;

        public DataExportFilterContainerManager(IDataExportRepository dataExportRepository)
        {
            _dataExportRepository = dataExportRepository;
        }

        public IContainer GetParentContainerIfAny(IContainer container)
        {
            return _dataExportRepository.SelectAll<FilterContainer>(
                "SELECT FilterContainer_ParentID FROM FilterContainerSubcontainers WHERE FilterContainerChildID=" + container.ID,
                "FilterContainer_ParentID").SingleOrDefault();
        }


        /// <inheritdoc/>
        public IContainer[] GetSubContainers(IContainer parent)
        {
            var subcontainers = _dataExportRepository.SelectAll<FilterContainer>(
                "SELECT FilterContainerChildID FROM FilterContainerSubcontainers WHERE FilterContainer_ParentID=" + parent.ID,
                "FilterContainerChildID");

            return subcontainers.Cast<IContainer>().ToArray();
        }

        /// <inheritdoc/>
        public IFilter[] GetFilters(IContainer container)
        {
            var filters = _dataExportRepository.GetAllObjectsWhere<DeployedExtractionFilter>("FilterContainer_ID" , container.ID);
            return filters.Cast<IFilter>().ToArray();
        }

        /// <inheritdoc/>
        public void AddSubContainer(IContainer parent, IContainer child)
        {
            if (!(child is FilterContainer))
                throw new NotSupportedException();

            _dataExportRepository.Insert("INSERT INTO FilterContainerSubcontainers(FilterContainer_ParentID,FilterContainerChildID) VALUES (@FilterContainer_ParentID, @FilterContainerChildID)", new Dictionary<string, object>
            {
                {"FilterContainer_ParentID", parent.ID},
                {"FilterContainerChildID", child.ID}
            });
        }

        /// <inheritdoc/>
        public void MakeIntoAnOrphan(IContainer container)
        {
            _dataExportRepository.Delete("DELETE FROM FilterContainerSubcontainers where FilterContainerChildID = @FilterContainerChildID", new Dictionary<string, object>
            {
                {"FilterContainerChildID", container.ID}
            });
        }
        public void AddChild(IContainer container, IFilter filter)
        {
            filter.FilterContainer_ID = container.ID;
            filter.SaveToDatabase();
        }
    }
}