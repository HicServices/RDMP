// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Helper for finding all the IFilter (lines of WHERE Sql) within a given IContainer (AND / OR),recursively finding all the subcontainers of an IContainer etc.
    /// </summary>
    public class ContainerHelper
    {
        /// <summary>
        /// Returns all IContainers that are declared as below the current IContainer (e.g. children).  This includes children of children etc down the tree.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public List<IContainer> GetAllSubContainersRecursively(IContainer current)
        {
            List<IContainer> toReturn = new List<IContainer>();

            var currentSubs = current.GetSubContainers();
            toReturn.AddRange(currentSubs);

            foreach (IContainer sub in currentSubs)
                toReturn.AddRange(GetAllSubContainersRecursively(sub));

            return toReturn;
        }

        /// <summary>
        /// Returns the absolute top level root IContainer of the hierarchy that the container is a part of.  If the specified container is already a root level container
        /// or it is an orphan or part of it's hierarchy going upwards is an orphan then the same container reference will be returned.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public IContainer GetRootContainerOrSelf(IContainer container)
        {
            var parent = container.GetParentContainerIfAny();
            if (parent != null)
                return GetRootContainerOrSelf(parent);

            return container;
        }

        /// <summary>
        /// Returns all IFilters that are declared in the current container or any of it's subcontainers (recursively).  This includes children of children 
        /// etc down the tree.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public List<IFilter> GetAllFiltersIncludingInSubContainersRecursively(IContainer container)
        {
            List<IFilter> toReturn = new List<IFilter>();

            toReturn.AddRange(container.GetFilters());

            IContainer[] subs = container.GetSubContainers();

            if (subs != null)
                foreach (IContainer sub in subs)
                    toReturn.AddRange(GetAllFiltersIncludingInSubContainersRecursively(sub));

            return toReturn;
        }
    }
}