using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.PerformanceImprovement
{

    /// <summary>
    /// Provides a memory based efficient (in terms of the number of database queries sent) way of finding all Catalogue filters and parameters as well as those used in
    /// AggregateConfigurations 
    /// 
    /// </summary>
    public class CatalogueFilterHierarchy
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

            AllAggregateContainers = repository.GetAllObjects<AggregateFilterContainer>().ToDictionary(o=>o.ID,o2=>o2);
            _allAggregateFilters = repository.GetAllObjects<AggregateFilter>();
            _allAggregateFilterParameters = repository.GetAllObjects<AggregateFilterParameter>();

            _allCatalogueFilters = repository.GetAllObjects<ExtractionFilter>();
            _allCatalogueParameters = repository.GetAllObjects<ExtractionFilterParameter>();
            _allCatalogueValueSets = repository.GetAllObjects<ExtractionFilterParameterSet>();
            _allCatalogueValueSetValues = repository.GetAllObjects<ExtractionFilterParameterSetValue>();
            
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
