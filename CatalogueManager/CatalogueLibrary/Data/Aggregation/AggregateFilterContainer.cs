// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
    /// <para>FilterContainers are fully hierarchical and must be fetched from the database via recursion from the SubContainer table (AggregateFilterSubContainer). 
    /// The class deals with all this transparently via GetSubContainers.</para>
    /// </summary>
    public class AggregateFilterContainer : VersionedDatabaseEntity, IContainer, IDisableable
    {
        #region Database Properties
        private FilterContainerOperation _operation;
        private bool _isDisabled;


        /// <inheritdoc/>
        public FilterContainerOperation Operation
        {
            get { return _operation; }
            set { SetField(ref  _operation, value); }
        }

        /// <inheritdoc/>
        public bool IsDisabled
        {
            get { return _isDisabled; }
            set { SetField(ref _isDisabled, value); }
        }

        #endregion

        /// <summary>
        /// Creates a new IContainer in the dtabase for use with an <see cref="AggregateConfiguration"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="operation"></param>
        public AggregateFilterContainer(ICatalogueRepository repository, FilterContainerOperation operation)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>(){{"Operation" ,operation.ToString()}});
        }


        internal AggregateFilterContainer(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            FilterContainerOperation op;
            FilterContainerOperation.TryParse(r["Operation"].ToString(), out op);
            Operation = op;

            IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Operation.ToString();
        }

        /// <inheritdoc/>
        public IContainer GetParentContainerIfAny()
        {
            return Repository.SelectAll<AggregateFilterContainer>("SELECT AggregateFilterContainer_ParentID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID=" + ID,
                "AggregateFilterContainer_ParentID").SingleOrDefault();
        }

        /// <inheritdoc/>
        public IContainer[] GetSubContainers()
        {
            return Repository.SelectAll<AggregateFilterContainer>("SELECT AggregateFilterContainer_ChildID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ParentID=" + ID, 
                "AggregateFilterContainer_ChildID").ToArray();
        }
        
        /// <inheritdoc/>
        public IFilter[] GetFilters()
        {
            return Repository.GetAllObjects<AggregateFilter>("WHERE FilterContainer_ID="+ID).ToArray();
        }

        /// <inheritdoc/>
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



        /// <inheritdoc/>
        public void MakeIntoAnOrphan()
        {
            Repository.Delete("DELETE FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID = @AggregateFilterContainer_ChildID", new Dictionary<string, object>
            {
                {"AggregateFilterContainer_ChildID", ID}
            });
        }
        
        /// <inheritdoc/>
        public IContainer GetRootContainerOrSelf()
        {
            return new ContainerHelper().GetRootContainerOrSelf(this);
        }

        /// <inheritdoc/>
        public List<IFilter> GetAllFiltersIncludingInSubContainersRecursively()
        {
            return new ContainerHelper().GetAllFiltersIncludingInSubContainersRecursively(this);
        }

        /// <inheritdoc/>
        public Catalogue GetCatalogueIfAny()
        {
            var agg = GetAggregate();
            return agg != null?agg.Catalogue:null;
        }

        /// <inheritdoc/>
        public List<IContainer> GetAllSubContainersRecursively()
        {
            return new ContainerHelper().GetAllSubContainersRecursively(this);
        }
        
        /// <summary>
        /// Creates a copy of the current AggregateFilterContainer including new copies of all subcontainers, filters (including those in subcontainers) and paramaters of those 
        /// filters.  This is a recursive operation that will clone the entire tree no matter how deep.
        /// </summary>
        /// <returns></returns>
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

        /// <inheritdoc/>
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

        /// <summary>
        /// Returns the AggregateConfiguration for which this container is either the root container for or part of the root container subcontainer tree.
        /// Returns null if the container is somehow an orphan. 
        /// </summary>
        /// <returns></returns>
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
