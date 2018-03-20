using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;


namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Sometimes you need to limit which records are extracted as part of an ExtractionConfiguration (See DeployedExtractionFilter).  In order to assemble valid WHERE SQL for this use
    /// case each DeployedExtractionFilter must be in either an AND or an OR container.  These FilterContainers ensure that each subcontainer / filter beyond the first is seperated by
    /// the appropriate operator (AND or OR) and brackets/tab indents where appropriate.
    /// </summary>
    public class FilterContainer : DatabaseEntity, IContainer
    {
        #region Database Properties
        private FilterContainerOperation _operation;

        public FilterContainerOperation Operation
        {
            get { return _operation; }
            set { SetField(ref _operation, value); }
        }

        #endregion

        public FilterContainer(IDataExportRepository repository, FilterContainerOperation operation = FilterContainerOperation.AND)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Operation", operation}
            });
        }

        internal FilterContainer(IDataExportRepository repository, DbDataReader r) : base(repository, r)
        {
            FilterContainerOperation op;

            if (FilterContainerOperation.TryParse(r["Operation"].ToString(), out op))
                Operation = op;
        }

        public IContainer GetParentContainerIfAny()
        {
            return  Repository.SelectAll<FilterContainer>(
                "SELECT FilterContainer_ParentID FROM FilterContainerSubcontainers WHERE FilterContainerChildID=" + ID,
                "FilterContainer_ParentID").SingleOrDefault();
        }
        public IContainer[] GetSubContainers()
        {
            var subcontainers = Repository.SelectAll<FilterContainer>(
                "SELECT FilterContainerChildID FROM FilterContainerSubcontainers WHERE FilterContainer_ParentID=" + ID,
                "FilterContainerChildID");

            return subcontainers.Cast<IContainer>().ToArray();
        }

        public IFilter[] GetFilters()
        {
            var filters = Repository.GetAllObjects<DeployedExtractionFilter>("WHERE FilterContainer_ID=" + ID);
            return filters.Cast<IFilter>().ToArray();
        }
        

        public void AddChild(IContainer child)
        {
            if (!(child is FilterContainer))
                throw new NotSupportedException();

            Repository.Insert("INSERT INTO FilterContainerSubcontainers(FilterContainer_ParentID,FilterContainerChildID) VALUES (@FilterContainer_ParentID, @FilterContainerChildID)", new Dictionary<string, object>
            {
                {"FilterContainer_ParentID", ID},
                {"FilterContainerChildID", child.ID}
            });
        }

        public void AddChild(IFilter filter)
        {
            if (filter.FilterContainer_ID.HasValue)
                if (filter.FilterContainer_ID == ID)
                    return; //It's already a child of us
                else
                    throw new NotSupportedException("Filter " + filter + " is already a child of nother container (ID=" + filter.FilterContainer_ID + ")");

            filter.FilterContainer_ID = ID;
            filter.SaveToDatabase();
        }

        public void MakeIntoAnOrphan()
        {
            Repository.Delete("DELETE FROM FilterContainerSubcontainers where FilterContainerChildID = @FilterContainerChildID", new Dictionary<string, object>
            {
                {"FilterContainerChildID", ID}
            });
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
            var sel = GetSelectedDataSetIfAny();
            return  sel != null?(Catalogue)sel.ExtractableDataSet.Catalogue:null;
        }

        public List<IContainer> GetAllSubContainersRecursively()
        {
            return new ContainerHelper().GetAllSubContainersRecursively(this);
        }

        public override string ToString()
        {
            return Operation.ToString();
        }

        public FilterContainer GetFilterContainerFor(IExtractionConfiguration configuration, IExtractableDataSet dataSet)
        {
            var objects = Repository.SelectAllWhere<FilterContainer>(
                "SELECT RootFilterContainer_ID FROM SelectedDataSets WHERE ExtractionConfiguration_ID=@ExtractionConfiguration_ID AND ExtractableDataSet_ID=@ExtractableDataSet_ID",
                "RootFilterContainer_ID",
                new Dictionary<string, object>
                {
                    {"ExtractionConfiguration_ID", configuration.ID},
                    {"ExtractableDataSet_ID", dataSet.ID}
                }).ToList();

            return objects.Any() ? objects.Single() : null;
        }
        
        public int CreateNewEmptyFilterContainerInDatabase()
        {
            return Repository.InsertAndReturnID<FilterContainer>(new Dictionary<string, object>
            {
                {"Operation", FilterContainerOperation.AND}
            });
        }

        
        public FilterContainer GetFilterContainerWithID(int id)
        {
            return Repository.GetObjectByID<FilterContainer>(id);
        }

        public override void DeleteInDatabase()
        {
            //if deleting first set delete any relationships where this is a child
            Repository.Delete("DELETE FROM FilterContainerSubcontainers WHERE FilterContainerChildID=" + ID,null,false);
            
            //then delete any children it has itself
            foreach (FilterContainer subContainer in this.GetSubContainers())
                subContainer.DeleteInDatabase();

            // then delete the actual component
            base.DeleteInDatabase();
        }
        
        public FilterContainer DeepCloneEntireTreeRecursivelyIncludingFilters()
        {
            //clone ourselves
            var clonedFilterContainer = Repository.CloneObjectInTable(this);
            
            //clone our filters
            foreach (var deployedExtractionFilter in GetFilters())
            {
                //clone it
                var cloneFilter = Repository.CloneObjectInTable((DeployedExtractionFilter) deployedExtractionFilter);

                //clone parameters
                foreach (DeployedExtractionFilterParameter parameter in deployedExtractionFilter.GetAllParameters())
                {
                    var clonefilterParameter = Repository.CloneObjectInTable(parameter);
                    clonefilterParameter.ExtractionFilter_ID = cloneFilter.ID;
                    clonefilterParameter.SaveToDatabase();
                }

                //change the clone to belonging to the cloned container (instead of this - the original container)
                cloneFilter.FilterContainer_ID = clonedFilterContainer.ID;
                cloneFilter.SaveToDatabase();
            }

            //now clone all subcontainers
            foreach (FilterContainer toCloneSubcontainer in this.GetSubContainers())
            {
                //clone the subcontainer recursively
                FilterContainer clonedSubcontainer = toCloneSubcontainer.DeepCloneEntireTreeRecursivelyIncludingFilters();

                //get the returned filter subcontainer and assocaite it with the cloned version of this
                clonedFilterContainer.AddChild(clonedSubcontainer);
            }

            //return the cloned version
            return clonedFilterContainer;
        }

        public SelectedDataSets GetSelectedDataSetIfAny()
        {
            var root = GetRootContainerOrSelf();

            if (root == null)
                return null;

            return Repository.GetAllObjects<SelectedDataSets>("WHERE RootFilterContainer_ID=" + root.ID).SingleOrDefault();
        }

        public SelectedDataSets GetSelectedDatasetRecursively()
        {
            //if it is a root
            var dataset = GetSelectedDataSetIfAny();

            //return the root
            if (dataset != null)
                return dataset;

            //it's not a root but it might have a parent (it should do!)
            var parent = (FilterContainer)GetParentContainerIfAny();

            //it doesn't
            if (parent == null)
                return null; //boo hoo, we are an orphan somehow

            //our parent must be the root container maybe? recursive
            return parent.GetSelectedDatasetRecursively();

        }
    }
}
