// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Providers
{
    /// <summary>
    /// Thread safe (ish) Collection which models the relationship between a parents.ID (e.g. <see cref="Catalogue.ID"/> and a childs child.parent_ID (e.g. <see cref="CatalogueItem.Catalogue_ID"/>.
    /// Class exists to provide rapid repeated lookups of this relationship in memory e.g. in <see cref="ICoreChildProvider"/>.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class From1ToM<T, T1>:ConcurrentDictionary<int,ConcurrentBag<T1>> where T:IMapsDirectlyToDatabaseTable where T1:IMapsDirectlyToDatabaseTable
    {
        public From1ToM(Func<T1,int> idSelector, IEnumerable<T1> collection)
        {
            Parallel.ForEach(collection, (o) =>
                {
                    int id = idSelector(o);

                    if (!Keys.Contains(id))
                        AddOrUpdate(id, new ConcurrentBag<T1>(),(i, set) => set);

                    this[id].Add(o);
                }
            );
        }

        public IEnumerable<T1> this[T parent]
        {
            get
            {
                if(this.ContainsKey(parent.ID))
                    return this[parent.ID];

                return Enumerable.Empty<T1>();
            }
        }
    }
}
