// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) implementation of IContainer.  
    /// 
    /// <para>IContainers are collections of subcontainers and WHERE statements e.g. 
    /// (
    ///     --age is above 5
    ///     Age > 5
    /// AND
    ///    --name is bob
    ///     Name like 'Bob%'
    /// )</para>
    /// 
    /// <para>Most IContainers come from the DataCatalogue/DataExport Database and are a hierarchical list of filters the user wants to use to create a query.  But sometimes IN CODE,
    /// we want to create an impromptu container and ram some additional filters we have either also invented or have pulled out of the Catalogue into the container.  This 
    /// Class lets you do that, it creates a 'memory only' container which cannot be saved/deleted etc but can be used in query building by ISqlQueryBuilders.</para>
    /// 
    /// <para>See also SpontaneouslyInventedFilter</para>
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
