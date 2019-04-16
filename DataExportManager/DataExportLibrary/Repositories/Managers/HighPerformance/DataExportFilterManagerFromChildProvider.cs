// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Providers;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Repositories.Managers.HighPerformance
{
    /// <summary>
    /// Provides a memory based efficient (in terms of the number of database queries sent) way of finding all containers and subcontainers and filters in the entire DataExportManager
    /// database at once rather than using the methods on IContainer and IFilter which send individual database queries for relevant subcontainers etc.
    /// </summary>
    class DataExportFilterManagerFromChildProvider : DataExportFilterManager
    {
        readonly Dictionary<int, List<FilterContainer>> _subcontainers = new Dictionary<int, List<FilterContainer>>();
        private DeployedExtractionFilter[] _allFilters;

        /// <summary>
        /// Fetches all containers and filters out of the <paramref name="repository"/> and sets the class up to provide
        /// fast access to them.
        /// </summary>
        /// <param name="repository"></param>
        public DataExportFilterManagerFromChildProvider(DataExportRepository repository, DataExportChildProvider childProvider): base(repository)
        {
            _allFilters = childProvider.AllDeployedExtractionFilters;

            var server = repository.DiscoveredServer;
            using (var con = repository.GetConnection())
            {
                var r = server.GetCommand("SELECT *  FROM FilterContainerSubcontainers", con).ExecuteReader();
                while(r.Read())
                {

                    var parentId = Convert.ToInt32(r["FilterContainer_ParentID"]);
                    var subcontainerId = Convert.ToInt32(r["FilterContainerChildID"]);

                    if(!_subcontainers.ContainsKey(parentId))
                        _subcontainers.Add(parentId,new List<FilterContainer>());

                    _subcontainers[parentId].Add(childProvider.AllContainers[subcontainerId]);
                }
                r.Close();
            }
        }
        
        /// <summary>
        /// Returns all subcontainers found in the <paramref name="parent"/> (results are returned from the cache created during class construction)
        /// </summary>
        public override IContainer[] GetSubContainers(IContainer parent)
        {
            if (!_subcontainers.ContainsKey(parent.ID))
                return new FilterContainer[0];

            return _subcontainers[parent.ID].ToArray();
        }

        public override IFilter[] GetFilters(IContainer container)
        {
            return  _allFilters.Where(f=>f.FilterContainer_ID == container.ID).ToArray();
        }
    }
}
