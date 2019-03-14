// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
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
    public class AggregateFilterContainer : ConcreteContainer, IDisableable
    {
        #region Database Properties
        
        private bool _isDisabled;
        private string _softwareVersion;
        

        /// <inheritdoc/>
        public bool IsDisabled
        {
            get { return _isDisabled; }
            set { SetField(ref _isDisabled, value); }
        }

        /// <summary>
        /// The version of RDMP that was running when the object was created
        /// </summary>
        [DoNotExtractProperty]
        public string SoftwareVersion
        {
            get { return _softwareVersion; }
            set { SetField(ref  _softwareVersion, value); }
        }

        #endregion

        /// <summary>
        /// Creates a new IContainer in the dtabase for use with an <see cref="AggregateConfiguration"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="operation"></param>
        public AggregateFilterContainer(ICatalogueRepository repository, FilterContainerOperation operation):base(repository.FilterContainerManager)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>(){{"Operation" ,operation}});
        }


        internal AggregateFilterContainer(ICatalogueRepository repository, DbDataReader r): base(repository.FilterContainerManager,repository, r)
        {
            SoftwareVersion = r["SoftwareVersion"].ToString();
            IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Operation.ToString();
        }


        /// <inheritdoc/>
        public override Catalogue GetCatalogueIfAny()
        {
            var agg = GetAggregate();
            return agg != null?agg.Catalogue:null;
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

        
        /// <summary>
        /// Returns the AggregateConfiguration for which this container is either the root container for or part of the root container subcontainer tree.
        /// Returns null if the container is somehow an orphan. 
        /// </summary>
        /// <returns></returns>
        public AggregateConfiguration GetAggregate()
        {
            var aggregateConfiguration = Repository.GetAllObjectsWhere<AggregateConfiguration>("RootFilterContainer_ID",ID).SingleOrDefault();

            if (aggregateConfiguration != null)
                return aggregateConfiguration;

            var parentContainer = GetParentContainerIfAny();

            if (parentContainer == null)
                return null;

            return ((AggregateFilterContainer)parentContainer).GetAggregate();
        }
    }
}
