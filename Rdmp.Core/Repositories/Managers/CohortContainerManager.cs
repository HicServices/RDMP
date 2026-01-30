// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.EntityFramework;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Repositories.Managers;

internal class CohortContainerManager : ICohortContainerManager
{
    protected readonly RDMPDbContext RDMPDbContext;

    public CohortContainerManager(RDMPDbContext catalogueRepository)
    {
        RDMPDbContext = catalogueRepository;
    }

    public CohortAggregateContainer GetParent(AggregateConfiguration child) => null;
        //CatalogueDbContext.SelectAllWhere<CohortAggregateContainer>(
        //    "SELECT CohortAggregateContainer_ID FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID",
        //    "CohortAggregateContainer_ID",
        //    new Dictionary<string, object>
        //    {
        //        { "AggregateConfiguration_ID", child.ID }
        //    }).SingleOrDefault();

    public void Add(CohortAggregateContainer parent, AggregateConfiguration child, int order)
    {
        //CatalogueDbContext.Insert(
        //    "INSERT INTO CohortAggregateContainer_AggregateConfiguration (CohortAggregateContainer_ID, AggregateConfiguration_ID, [Order]) VALUES (@CohortAggregateContainer_ID, @AggregateConfiguration_ID, @Order)",
        //    new Dictionary<string, object>
        //    {
        //        { "CohortAggregateContainer_ID", parent.ID },
        //        { "AggregateConfiguration_ID", child.ID },
        //        { "Order", order }
        //    });
    }

    public void Remove(CohortAggregateContainer parent, AggregateConfiguration child)
    {
        //CatalogueDbContext.Delete(
        //    "DELETE FROM CohortAggregateContainer_AggregateConfiguration WHERE CohortAggregateContainer_ID = @CohortAggregateContainer_ID AND AggregateConfiguration_ID = @AggregateConfiguration_ID",
        //    new Dictionary<string, object>
        //    {
        //        { "CohortAggregateContainer_ID", parent.ID },
        //        { "AggregateConfiguration_ID", child.ID }
        //    });
    }


    public int? GetOrderIfExistsFor(AggregateConfiguration configuration)
    {
        //if (configuration.CatalogueDbContext != this)
        //    if ((configuration.CatalogueDbContext).ConnectionString !=
        //        CatalogueDbContext.ConnectionString)
        //        throw new NotSupportedException(
        //            "AggregateConfiguration is from a different repository than this with a different connection string");

        //using var con = CatalogueDbContext.GetConnection();
        //using var cmd = DatabaseCommandHelper.GetCommand(
        //    "SELECT [Order] FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID",
        //    con.Connection, con.Transaction);
        //cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@AggregateConfiguration_ID", cmd));
        //cmd.Parameters["@AggregateConfiguration_ID"].Value = configuration.ID;

        //return CatalogueDbContext.ObjectToNullableInt(cmd.ExecuteScalar());
        return null;
    }

    public virtual IOrderable[] GetChildren(CohortAggregateContainer parent)
    {
        //var containers = CatalogueDbContext.SelectAllWhere<CohortAggregateContainer>(
        //    "SELECT CohortAggregateContainer_ChildID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ParentID=@CohortAggregateContainer_ParentID",
        //    "CohortAggregateContainer_ChildID",
        //    new Dictionary<string, object>
        //    {
        //        { "CohortAggregateContainer_ParentID", parent.ID }
        //    }).ToArray();

        //var configs = CatalogueDbContext.SelectAll<AggregateConfiguration>(
        //        $"SELECT AggregateConfiguration_ID FROM CohortAggregateContainer_AggregateConfiguration where CohortAggregateContainer_ID={parent.ID}")
        //    .OrderBy(config => config.Order).ToArray();

        //return containers.Cast<IOrderable>().Union(configs).OrderBy(o => o.Order).ToArray();
        return null;
    }

    public CohortAggregateContainer GetParent(CohortAggregateContainer child) => null;
        //CatalogueDbContext.SelectAllWhere<CohortAggregateContainer>(
        //    "SELECT CohortAggregateContainer_ParentID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID=@CohortAggregateContainer_ChildID",
        //    "CohortAggregateContainer_ParentID",
        //    new Dictionary<string, object>
        //    {
        //        { "CohortAggregateContainer_ChildID", child.ID }
        //    }).SingleOrDefault();

    public void Remove(CohortAggregateContainer parent, CohortAggregateContainer child)
    {
        //CatalogueDbContext.Delete(
        //    "DELETE FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID = @CohortAggregateContainer_ChildID",
        //    new Dictionary<string, object>
        //    {
        //        { "CohortAggregateContainer_ChildID", child.ID }
        //    });
    }

    public void SetOrder(AggregateConfiguration child, int newOrder)
    {
        //CatalogueDbContext.Update(
        //    $"UPDATE CohortAggregateContainer_AggregateConfiguration SET [Order] = {newOrder} WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID",
        //    new Dictionary<string, object>
        //    {
        //        { "AggregateConfiguration_ID", child.ID }
        //    });
    }

    public void Add(CohortAggregateContainer parent, CohortAggregateContainer child)
    {
        //CatalogueDbContext.Insert(
        //    "INSERT INTO CohortAggregateSubContainer(CohortAggregateContainer_ParentID,CohortAggregateContainer_ChildID) VALUES (@CohortAggregateContainer_ParentID, @CohortAggregateContainer_ChildID)",
        //    new Dictionary<string, object>
        //    {
        //        { "CohortAggregateContainer_ParentID", parent.ID },
        //        { "CohortAggregateContainer_ChildID", child.ID }
        //    });
    }
}