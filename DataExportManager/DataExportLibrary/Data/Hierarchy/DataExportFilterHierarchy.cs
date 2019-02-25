// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.Hierarchy
{

    /// <summary>
    /// Provides a memory based efficient (in terms of the number of database queries sent) way of finding all containers and subcontainers and filters in the entire DataExportManager
    /// database at once rather than using the methods on IContainer and IFilter which send individual database queries for relevant subcontainers etc.
    /// </summary>
    public class DataExportFilterHierarchy
    {
        /// <summary>
        /// All AND/OR containers found during construction (in the data export database).  The Key is the ID of the container (for rapid random access)
        /// </summary>
        public readonly Dictionary<int, FilterContainer> AllContainers;
        private readonly DeployedExtractionFilter[] _allFilters;
        private readonly DeployedExtractionFilterParameter[] _allParameters;

        readonly Dictionary<int, List<FilterContainer>> _subcontainers = new Dictionary<int, List<FilterContainer>>();
        
        /// <summary>
        /// Fetches all containers and filters out of the <paramref name="repository"/> and sets the class up to provide
        /// fast access to them.
        /// </summary>
        /// <param name="repository"></param>
        public DataExportFilterHierarchy(IDataExportRepository repository)
        {
            var repo = (TableRepository) repository;

            AllContainers = GetAllObjects<FilterContainer>(repo).ToDictionary(o => o.ID, o => o);
            _allFilters = GetAllObjects<DeployedExtractionFilter>(repo);
            _allParameters = GetAllObjects<DeployedExtractionFilterParameter>(repo);
            
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

                    _subcontainers[parentId].Add(AllContainers[subcontainerId]);

                    
                }
                r.Close();
            }
        }

        private T[] GetAllObjects<T>(TableRepository repository)where T:IMapsDirectlyToDatabaseTable
        {
            return CatalogueChildProvider.UseCaching ? repository.GetAllObjectsCached<T>():repository.GetAllObjects<T>();
        }

        /// <summary>
        /// Returns all filters found in the data export database (cached during class construction)
        /// </summary>
        /// <param name="filterContainer"></param>
        /// <returns></returns>
        public IEnumerable<DeployedExtractionFilter> GetFilters(FilterContainer filterContainer)
        {
            return _allFilters.Where(f => f.FilterContainer_ID == filterContainer.ID);
        }


        /// <summary>
        /// Returns all subcontainers found in the <paramref name="filterContainer"/> (results are returned from the cache created during class construction)
        /// </summary>
        /// <param name="filterContainer"></param>
        /// <returns></returns>
        public IEnumerable<FilterContainer> GetSubcontainers(FilterContainer filterContainer)
        {
            if (!_subcontainers.ContainsKey(filterContainer.ID))
                return new FilterContainer[0];

            return _subcontainers[filterContainer.ID];
        }

        /// <summary>
        /// Returns all parameters declared against the <paramref name="filter"/> (results are returned from the cache created during class construction)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<DeployedExtractionFilterParameter> GetParameters(DeployedExtractionFilter filter)
        {
            return _allParameters.Where(p => p.ExtractionFilter_ID == filter.ID);
        }
        
    }
}
