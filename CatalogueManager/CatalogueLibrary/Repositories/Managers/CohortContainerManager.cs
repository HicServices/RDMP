// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using ReusableLibraryCode;

namespace CatalogueLibrary.Repositories.Managers
{
    class CohortContainerManager : ICohortContainerManager
    {
        private readonly CatalogueRepository _catalogueRepository;

        public CohortContainerManager(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public CohortAggregateContainer GetParent(AggregateConfiguration child)
        {
            return
                _catalogueRepository.SelectAllWhere<CohortAggregateContainer>(
                    "SELECT CohortAggregateContainer_ID FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID",
                    "CohortAggregateContainer_ID",
                    new Dictionary<string, object>
                    {
                        {"AggregateConfiguration_ID", child.ID}
                    }).SingleOrDefault();
        }

        public void Add(CohortAggregateContainer parent, AggregateConfiguration child,int order)
        {

            _catalogueRepository.Insert(
                "INSERT INTO CohortAggregateContainer_AggregateConfiguration (CohortAggregateContainer_ID, AggregateConfiguration_ID, [Order]) VALUES (@CohortAggregateContainer_ID, @AggregateConfiguration_ID, @Order)",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ID", parent.ID},
                    {"AggregateConfiguration_ID", child.ID},
                    {"Order", order}
                });

        }

        public void Remove(CohortAggregateContainer parent,AggregateConfiguration child)
        {
            _catalogueRepository.Delete("DELETE FROM CohortAggregateContainer_AggregateConfiguration WHERE CohortAggregateContainer_ID = @CohortAggregateContainer_ID AND AggregateConfiguration_ID = @AggregateConfiguration_ID", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ID", parent.ID},
                {"AggregateConfiguration_ID", child.ID}
            });
        }

        
        public int? GetOrderIfExistsFor(AggregateConfiguration configuration)
        {
            if (configuration.Repository != this)
                if (((CatalogueRepository)configuration.Repository).ConnectionString != _catalogueRepository.ConnectionString)
                    throw new NotSupportedException("AggregateConfiguration is from a different repository than this with a different connection string");

            using (var con = _catalogueRepository.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand("SELECT [Order] FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID", con.Connection, con.Transaction);

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@AggregateConfiguration_ID", cmd));
                cmd.Parameters["@AggregateConfiguration_ID"].Value = configuration.ID;

                return _catalogueRepository.ObjectToNullableInt(cmd.ExecuteScalar());
            }
        }

        public IOrderable[] GetChildren(CohortAggregateContainer parent)
        {
            var containers = _catalogueRepository.SelectAllWhere<CohortAggregateContainer>("SELECT CohortAggregateContainer_ChildID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ParentID=@CohortAggregateContainer_ParentID",
                "CohortAggregateContainer_ChildID",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ParentID", parent.ID}
                }).ToArray();

            var configs = _catalogueRepository.SelectAll<AggregateConfiguration>("SELECT AggregateConfiguration_ID FROM CohortAggregateContainer_AggregateConfiguration where CohortAggregateContainer_ID=" + parent.ID).OrderBy(config => config.Order).ToArray();

            return containers.Cast<IOrderable>().Union(configs).OrderBy(o => o.Order).ToArray();
        }

        public CohortAggregateContainer GetParent(CohortAggregateContainer child)
        {
            return _catalogueRepository.SelectAllWhere<CohortAggregateContainer>("SELECT CohortAggregateContainer_ParentID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID=@CohortAggregateContainer_ChildID",
                "CohortAggregateContainer_ParentID",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ChildID", child.ID}
                }).SingleOrDefault();
        }

        public void Remove(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            _catalogueRepository.Delete("DELETE FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID = @CohortAggregateContainer_ChildID", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ChildID", child.ID}
            });
        }

        public void SetOrder(AggregateConfiguration child, int newOrder)
        {
            _catalogueRepository.Update("UPDATE CohortAggregateContainer_AggregateConfiguration SET [Order] = " + newOrder + " WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID", new Dictionary<string, object>
            {
                {"AggregateConfiguration_ID", child.ID}
            });
        }

        public void Add(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            _catalogueRepository.Insert("INSERT INTO CohortAggregateSubContainer(CohortAggregateContainer_ParentID,CohortAggregateContainer_ChildID) VALUES (@CohortAggregateContainer_ParentID, @CohortAggregateContainer_ChildID)", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ParentID", parent.ID},
                {"CohortAggregateContainer_ChildID", child.ID}
            });
        }
    }
}