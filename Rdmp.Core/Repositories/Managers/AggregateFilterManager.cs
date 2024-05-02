// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.Repositories.Managers;

internal class AggregateFilterManager : IFilterManager
{
    private readonly CatalogueRepository _catalogueRepository;

    public AggregateFilterManager(CatalogueRepository catalogueRepository)
    {
        _catalogueRepository = catalogueRepository;
    }

    public virtual IContainer[] GetSubContainers(IContainer container)
    {
        return _catalogueRepository.SelectAll<AggregateFilterContainer>(
            $"SELECT AggregateFilterContainer_ChildID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ParentID={container.ID}",
            "AggregateFilterContainer_ChildID").ToArray();
    }

    public void MakeIntoAnOrphan(IContainer container)
    {
        _catalogueRepository.Delete(
            "DELETE FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID = @AggregateFilterContainer_ChildID",
            new Dictionary<string, object>
            {
                { "AggregateFilterContainer_ChildID", container.ID }
            }, false);
    }

    public IContainer GetParentContainerIfAny(IContainer container)
    {
        return _catalogueRepository.SelectAll<AggregateFilterContainer>(
            $"SELECT AggregateFilterContainer_ParentID FROM AggregateFilterSubContainer WHERE AggregateFilterContainer_ChildID={container.ID}",
            "AggregateFilterContainer_ParentID").SingleOrDefault();
    }

    public virtual IFilter[] GetFilters(IContainer container)
    {
        return _catalogueRepository
            .GetAllObjectsWhere<AggregateFilter>("FilterContainer_ID", container.ID).ToArray();
    }

    public void AddSubContainer(IContainer parent, IContainer child)
    {
        _catalogueRepository.Insert(
            "INSERT INTO AggregateFilterSubContainer(AggregateFilterContainer_ParentID,AggregateFilterContainer_ChildID) VALUES (@AggregateFilterContainer_ParentID,@AggregateFilterContainer_ChildID)",
            new Dictionary<string, object>
            {
                { "AggregateFilterContainer_ParentID", parent.ID },
                { "AggregateFilterContainer_ChildID", child.ID }
            });
    }

    public void AddChild(IContainer container, IFilter filter)
    {
        filter.FilterContainer_ID = container.ID;
        filter.SaveToDatabase();
    }
}