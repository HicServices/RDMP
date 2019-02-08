// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.PerformanceImprovement
{

    /// <summary>
    /// Provides a memory based efficient (in terms of the number of database queries sent) way of finding all Catalogue filters and parameters as well as those used in
    /// AggregateConfigurations 
    /// 
    /// </summary>
    class CatalogueFilterHierarchy
    {
        //Filters for Aggregates (includes filter containers (AND/OR)
        public Dictionary<int,AggregateFilterContainer> AllAggregateContainers;
        private AggregateFilter[] _allAggregateFilters;
        private AggregateFilterParameter[] _allAggregateFilterParameters;

        //Catalogue master filters (does not include any support for filter containers (AND/OR)
        private ExtractionFilter[] _allCatalogueFilters;
        public ExtractionFilterParameter[] _allCatalogueParameters;
        public ExtractionFilterParameterSet[] _allCatalogueValueSets;
        public ExtractionFilterParameterSetValue[] _allCatalogueValueSetValues;

        /// <summary>
        /// Where ID key is the ID of the parent and the Value List is all the subcontainers.  If there is no key there are no subcontainers.
        /// </summary>
        readonly Dictionary<int, List<AggregateFilterContainer>> _subcontainers = new Dictionary<int, List<AggregateFilterContainer>>();

        public CatalogueFilterHierarchy(CatalogueRepository repository)
        {
            AllAggregateContainers = GetAllObjects<AggregateFilterContainer>(repository).ToDictionary(o=>o.ID,o2=>o2);
            _allAggregateFilters = GetAllObjects<AggregateFilter>(repository);
            _allAggregateFilterParameters = GetAllObjects<AggregateFilterParameter>(repository);

            _allCatalogueFilters = GetAllObjects<ExtractionFilter>(repository);
            _allCatalogueParameters = GetAllObjects<ExtractionFilterParameter>(repository);
            _allCatalogueValueSets = GetAllObjects<ExtractionFilterParameterSet>(repository);
            _allCatalogueValueSetValues = GetAllObjects<ExtractionFilterParameterSetValue>(repository);
            
            var server = repository.DiscoveredServer;
            using (var con = repository.GetConnection())
            {
                var r = server.GetCommand("SELECT [AggregateFilterContainer_ParentID],[AggregateFilterContainer_ChildID]  FROM [AggregateFilterSubContainer]", con).ExecuteReader();
                while(r.Read())
                {

                    var parentId = Convert.ToInt32(r["AggregateFilterContainer_ParentID"]);
                    var subcontainerId = Convert.ToInt32(r["AggregateFilterContainer_ChildID"]);

                    if(!_subcontainers.ContainsKey(parentId))
                        _subcontainers.Add(parentId,new List<AggregateFilterContainer>());

                    _subcontainers[parentId].Add(AllAggregateContainers[subcontainerId]);
                }
                r.Close();
            }
        }

        private T[] GetAllObjects<T>(CatalogueRepository repository) where T: IMapsDirectlyToDatabaseTable
        {
            return CatalogueChildProvider.UseCaching ? repository.GetAllObjectsCached<T>() : repository.GetAllObjects<T>();
        }

        //Aggregates
        public IEnumerable<AggregateFilter> GetFilters(AggregateFilterContainer filterContainer)
        {
            return _allAggregateFilters.Where(f => f.FilterContainer_ID == filterContainer.ID);
        }
        

        public IEnumerable<AggregateFilterContainer> GetSubcontainers(AggregateFilterContainer filterContainer)
        {
            if (!_subcontainers.ContainsKey(filterContainer.ID))
                return new AggregateFilterContainer[0];

            return _subcontainers[filterContainer.ID];
        }

        public IEnumerable<AggregateFilterParameter> GetParameters(AggregateFilter filter)
        {
            return _allAggregateFilterParameters.Where(p => p.AggregateFilter_ID == filter.ID);
        }


        //Catalogue Maters
        public IEnumerable<ExtractionFilter> GetFilters(ExtractionInformation extractionInformation)
        {
            return _allCatalogueFilters.Where(f => f.ExtractionInformation_ID == extractionInformation.ID);
        }
        public IEnumerable<ExtractionFilterParameter> GetParameters(ExtractionFilter filter)
        {
            return _allCatalogueParameters.Where(p => p.ExtractionFilter_ID == filter.ID);
        }

        public IEnumerable<ExtractionFilterParameterSet> GetValueSets(ExtractionFilter filter)
        {
            return _allCatalogueValueSets.Where(vs => vs.ExtractionFilter_ID == filter.ID);
        }

        public IEnumerable<ExtractionFilterParameterSetValue> GetValueSetValues(ExtractionFilterParameterSet parameterSet)
        {
            return _allCatalogueValueSetValues.Where(v => v.ExtractionFilterParameterSet_ID == parameterSet.ID);
        }

    }
}
