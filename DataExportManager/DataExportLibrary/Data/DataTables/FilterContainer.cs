// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;


namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Sometimes you need to limit which records are extracted as part of an ExtractionConfiguration (See DeployedExtractionFilter).  In order to assemble valid WHERE SQL for this use
    /// case each DeployedExtractionFilter must be in either an AND or an OR container.  These FilterContainers ensure that each subcontainer / filter beyond the first is seperated by
    /// the appropriate operator (AND or OR) and brackets/tab indents where appropriate.
    /// </summary>
    public class FilterContainer : ConcreteContainer, IContainer
    {
        /// <summary>
        /// Creates a new instance with the given <paramref name="operation"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="operation"></param>
        public FilterContainer(IDataExportRepository repository, FilterContainerOperation operation = FilterContainerOperation.AND):base(repository.FilterManager)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Operation", operation}
            });
        }

        internal FilterContainer(IDataExportRepository repository, DbDataReader r) : base(repository.FilterManager,repository, r)
        {
        }

        /// <inheritdoc/>
        public override Catalogue GetCatalogueIfAny()
        {
            var sel = GetSelectedDataSetIfAny();
            return sel != null ? (Catalogue)sel.ExtractableDataSet.Catalogue : null;
        }
        
        /// <summary>
        /// Returns the <see cref="ConcreteContainer.Operation"/> "AND" or "OR"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Operation.ToString();
        }
        
        
        
        /// <summary>
        /// Creates a deep copy of the current container, all filters and subcontainers (recursively).  These objects will all have new IDs and be new objects
        /// in the repository database.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// If this container is a top level root container (as opposed to a subcontainer) this will return which <see cref="SelectedDataSets"/> (which dataset in which configuration)
        /// in which it applies.
        /// </summary>
        /// <returns></returns>
        public SelectedDataSets GetSelectedDataSetIfAny()
        {
            var root = GetRootContainerOrSelf();

            if (root == null)
                return null;

            return Repository.GetAllObjectsWhere<SelectedDataSets>("RootFilterContainer_ID", root.ID).SingleOrDefault();
        }


        /// <summary>
        /// /// <summary>
        /// Return which <see cref="SelectedDataSets"/> (which dataset in which configuration) this container resides in (or null if it is an orphan).  This
        /// involves multiple database queries as the container hierarchy is recursively traversed up.
        /// </summary>
        /// <returns></returns>
        /// </summary>
        /// <returns></returns>
        public SelectedDataSets GetSelectedDataSetsRecursively()
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
            return parent.GetSelectedDataSetsRecursively();

        }
    }
}
