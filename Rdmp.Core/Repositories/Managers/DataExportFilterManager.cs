// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Repositories.Managers;

internal class DataExportFilterManager : IFilterManager
{
    private readonly DataExportRepository _dataExportRepository;

    public DataExportFilterManager(DataExportRepository dataExportRepository)
    {
        _dataExportRepository = dataExportRepository;
    }

    public IContainer GetParentContainerIfAny(IContainer container)
    {
        return _dataExportRepository.SelectAll<FilterContainer>(
            $"SELECT FilterContainer_ParentID FROM FilterContainerSubcontainers WHERE FilterContainerChildID={container.ID}",
            "FilterContainer_ParentID").SingleOrDefault();
    }


    /// <inheritdoc />
    public virtual IContainer[] GetSubContainers(IContainer parent)
    {
        var subcontainers = _dataExportRepository.SelectAll<FilterContainer>(
            $"SELECT FilterContainerChildID FROM FilterContainerSubcontainers WHERE FilterContainer_ParentID={parent.ID}",
            "FilterContainerChildID");

        return subcontainers.Cast<IContainer>().ToArray();
    }

    /// <inheritdoc />
    public virtual IFilter[] GetFilters(IContainer container)
    {
        var filters =
            _dataExportRepository.GetAllObjectsWhere<DeployedExtractionFilter>("FilterContainer_ID", container.ID);
        return filters.Cast<IFilter>().ToArray();
    }

    /// <inheritdoc />
    public void AddSubContainer(IContainer parent, IContainer child)
    {
        if (child is not FilterContainer)
            throw new NotSupportedException();

        _dataExportRepository.Insert(
            "INSERT INTO FilterContainerSubcontainers(FilterContainer_ParentID,FilterContainerChildID) VALUES (@FilterContainer_ParentID, @FilterContainerChildID)",
            new Dictionary<string, object>
            {
                { "FilterContainer_ParentID", parent.ID },
                { "FilterContainerChildID", child.ID }
            });
    }

    /// <inheritdoc />
    public void MakeIntoAnOrphan(IContainer container)
    {
        _dataExportRepository.Delete(
            "DELETE FROM FilterContainerSubcontainers where FilterContainerChildID = @FilterContainerChildID",
            new Dictionary<string, object>
            {
                { "FilterContainerChildID", container.ID }
            }, false);
    }

    public void AddChild(IContainer container, IFilter filter)
    {
        filter.FilterContainer_ID = container.ID;
        filter.SaveToDatabase();
    }
}