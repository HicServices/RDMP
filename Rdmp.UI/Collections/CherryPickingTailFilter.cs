// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrightIdeasSoftware;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Tail filter that returns up to a maximum number of objects but priorities search matches
    /// </summary>
    internal class CherryPickingTailFilter : IListFilter
    {
        private readonly int _numberOfObjects;
        private readonly TextMatchFilterWithWhiteList _modelFilter;

        public CherryPickingTailFilter(int numberOfObjects,TextMatchFilterWithWhiteList modelFilter)
        {
            _numberOfObjects = numberOfObjects;
            _modelFilter = modelFilter;
        }

        public IEnumerable Filter(IEnumerable modelObjects)
        {
            int countReturned = _numberOfObjects;

            if (_modelFilter == null)
                return modelObjects.Cast<object>().Take(_numberOfObjects);

            bool hasSearchTokens = _modelFilter.HasComponents;
            bool hasWhitelist = _modelFilter.WhiteList != null && _modelFilter.WhiteList.Any();

            var available = modelObjects.Cast<object>().ToList();

            //We will return the whitelisted objects for sure
            var toReturn = hasWhitelist ? new HashSet<object>(available.Intersect(_modelFilter.WhiteList)) : new HashSet<object>();
            
            //but lets also take up to _numberOfObjects other objects that match the filter (if any)
            foreach (var a in available.Where(o => !hasSearchTokens || _modelFilter.Filter(o)))
            {
                countReturned--;
                toReturn.Add(a);

                if (countReturned < 0)
                    break;
            }

            return toReturn;
        }
    }
}