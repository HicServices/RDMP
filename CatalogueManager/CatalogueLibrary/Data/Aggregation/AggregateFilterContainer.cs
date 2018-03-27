using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// All AggregateFilters must be contained within an AggregateFilterContainer at Query Generation time.  This tells QueryBuilder how to use brackets and whether to AND / OR 
    /// the various filter lines.  The AggregateFilterContainer serves the same purpose as the FilterContainer in Data Export Manager but for AggregateConfigurations (GROUP BY queries)
    /// 
    /// FilterContainers are fully hierarchical and must be fetched from the database via recursion from the SubContainer table (AggregateFilterSubContainer). 
    /// The class deals with all this transparently via GetSubContainers.
    /// </summary>
    public class AggregateFilterContainer: VersionedDatabaseEntity, IContainer
    {
        #region Database Properties
        private FilterContainerOperation _operation;

        public FilterContainerOperation Operation
        {
            get { return _operation; }
            set { SetField(ref  _operation, value); }
        }

        #endregion


        public AggregateFilterContainer(ICatalogueRepository repository, FilterContainerOperation operation)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>(){{"Operation" ,operation.ToString()}});
        }


        internal AggregateFilterContainer(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            FilterContainerOperation op;
            FilterContainerOperation.TryParse(r["Operation"].ToString(), out op);
            Operation = op;
        }

        public override string ToString()
        {
            return Operation.ToString();
        }
        public IContainer GetParentContainerIfAny()
        {
            return Repository.SelectAll<AggregateFilterContainer>("SELECT AggregateFilterContainer_ParentID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID=" + ID,
                "AggregateFilterContainer_ParentID").SingleOrDefault();
        }
        
        public IContainer[] GetSubContainers()
        {
            return Repository.SelectAll<AggregateFilterContainer>("SELECT AggregateFilterContainer_ChildID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ParentID=" + ID, 
                "AggregateFilterContainer_ChildID").ToArray();
        }
        public IFilter[] GetFilters()
        {
            return Repository.GetAllObjects<AggregateFilter>("WHERE FilterContainer_ID="+ID).ToArray();
        }

        public void AddChild(IContainer child)
        {
            AddChild((AggregateFilterContainer)child);
        }

        private void AddChild(AggregateFilterContainer child)
        {
            Repository.Insert(
                "INSERT INTO AggregateFilterSubContainer(AggregateFilterContainer_ParentID,AggregateFilterContainer_ChildID) VALUES (@AggregateFilterContainer_ParentID,@AggregateFilterContainer_ChildID)",
            new Dictionary<string, object>
            {
                {"AggregateFilterContainer_ParentID", ID},
                {"AggregateFilterContainer_ChildID", child.ID}
            });
        }

        


        public void MakeIntoAnOrphan()
        {
            Repository.Delete("DELETE FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID = @AggregateFilterContainer_ChildID", new Dictionary<string, object>
            {
                {"AggregateFilterContainer_ChildID", ID}
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
            var agg = GetAggregate();
            return agg != null?agg.Catalogue:null;
        }

        public List<IContainer> GetAllSubContainersRecursively()
        {
            return new ContainerHelper().GetAllSubContainersRecursively(this);
        }
        
        
        public AggregateFilterContainer DeepCloneEntireTreeRecursivelyIncludingFilters()
        {
            //clone ourselves
            AggregateFilterContainer clone = Repository.CloneObjectInTable(this);
            
            //clone our filters
            foreach (AggregateFilter filterToClone in GetFilters())
            {
                //clone it
                AggregateFilter cloneFilter = Repository.CloneObjectInTable(filterToClone);

                //clone parameters
                foreach (AggregateFilterParameter parameterToClone in filterToClone.GetAllParameters())
                {
                    AggregateFilterParameter clonefilterParameter = Repository.CloneObjectInTable(parameterToClone);

                    //change the cloned parameter to belong to the cloned filter
                    clonefilterParameter.AggregateFilter_ID = cloneFilter.ID;
                    clonefilterParameter.SaveToDatabase();
                }

                //change the clone to belonging to the cloned container (instead of this - the original container)
                cloneFilter.FilterContainer_ID = clone.ID;
                cloneFilter.SaveToDatabase();
            }

            //now clone all subcontainers
            foreach (AggregateFilterContainer toCloneSubcontainer in GetSubContainers())
            {
                //clone the subcontainer recursively
                AggregateFilterContainer clonedSubcontainer =
                    toCloneSubcontainer.DeepCloneEntireTreeRecursivelyIncludingFilters();

                //get the returned filter subcontainer and assocaite it with the cloned version of this
                clone.AddChild(clonedSubcontainer);
            }

            //return the cloned version
            return clone;
        }

        public void AddChild(IFilter filter)
        {
            if(filter.FilterContainer_ID.HasValue)
                if (filter.FilterContainer_ID == ID)
                    return; //It's already a child of us
                else
                    throw new NotSupportedException("Filter " + filter + " is already a child of nother container (ID=" + filter.FilterContainer_ID + ")");

            filter.FilterContainer_ID = ID;
            filter.SaveToDatabase();
        }

        public AggregateConfiguration GetAggregate()
        {
            var aggregateConfiguration = Repository.GetAllObjects<AggregateConfiguration>("WHERE RootFilterContainer_ID = " + ID).SingleOrDefault();

            if (aggregateConfiguration != null)
                return aggregateConfiguration;

            var parentContainer = GetParentContainerIfAny();

            if (parentContainer == null)
                return null;

            return ((AggregateFilterContainer)parentContainer).GetAggregate();
        }
    }
}
