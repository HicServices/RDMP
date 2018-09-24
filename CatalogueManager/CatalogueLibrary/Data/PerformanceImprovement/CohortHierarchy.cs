using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;


namespace CatalogueLibrary.Data.PerformanceImprovement
{
    /// <summary>
    /// Performance class that builds the hierarchy of CohortIdentificationConfiguration children.  This includes containers (CohortAggregateContainer) and subcontainers
    /// and thier contained cohort sets ( AggregateConfiguration).  This is done in memory by fetching all the relevant relationship records with two queries and then
    /// sorting out the already fetched objects in CatalogueChildProvider into the relevant hierarchy.
    /// 
    /// <para>This allows you to use GetSubContainers and GetAggregateConfigurations in bulk without having to use the method on IContainer directly (which goes back to the database).</para>
    /// </summary>
    class CohortHierarchy
    {
        private readonly CatalogueChildProvider _childProvider;
        public CohortAggregateContainer[] AllContainers { get; private set; }
        public JoinableCohortAggregateConfiguration[] AllJoinables { get; private set; }
        public JoinableCohortAggregateConfigurationUse[] AllJoinUses { get; private set; }

        private Dictionary<int, List<CohortAggregateContainer>> _subContainers;
        private Dictionary<int, List<AggregateConfiguration>> _aggregatesInContainers;

        public CohortHierarchy(CatalogueRepository repository,CatalogueChildProvider childProvider)
        {
            _childProvider = childProvider;
            AllContainers = repository.GetAllObjects<CohortAggregateContainer>();
            AllJoinables = repository.GetAllObjects<JoinableCohortAggregateConfiguration>();
            AllJoinUses = repository.GetAllObjects<JoinableCohortAggregateConfigurationUse>();
            
            _subContainers = new Dictionary<int, List<CohortAggregateContainer>>();
            _aggregatesInContainers = new Dictionary<int, List<AggregateConfiguration>>();

            using (var con = repository.GetConnection())
            {

                //find all the cohort SET operation subcontainers e.g. UNION Ag1,Ag2,(Agg3 INTERSECT Agg4) would have 2 CohortAggregateContainers (the UNION and the INTERSECT) in which the INTERSECT was the child container of the UNION
                var r = repository.DiscoveredServer.GetCommand("SELECT [CohortAggregateContainer_ParentID],[CohortAggregateContainer_ChildID] FROM [CohortAggregateSubContainer] ORDER BY CohortAggregateContainer_ParentID", con).ExecuteReader();

                int lastParentId = -1;
                while (r.Read())
                {
                    var currentParentId = Convert.ToInt32(r["CohortAggregateContainer_ParentID"]);
                    var currentChildId = Convert.ToInt32(r["CohortAggregateContainer_ChildID"]);

                    if (currentParentId != lastParentId)
                    {
                        _subContainers.Add(currentParentId,new List<CohortAggregateContainer>());
                        lastParentId = currentParentId;
                    }

                    _subContainers[currentParentId].Add(AllContainers.Single(c=>c.ID == currentChildId));
                }
                r.Close();

                //now find all the Agg configurations within the containers too, (in the above example we will find Agg1 in the UNION container at order 1 and Agg2 at order 2 and then we find Agg3 and Agg4 in the INTERSECT container)
                r = repository.DiscoveredServer.GetCommand(@"SELECT [CohortAggregateContainer_ID], [AggregateConfiguration_ID],[Order] FROM [CohortAggregateContainer_AggregateConfiguration] ORDER BY CohortAggregateContainer_ID", con).ExecuteReader();
                
                lastParentId = -1;
                while (r.Read())
                {
                    var currentParentId = Convert.ToInt32(r["CohortAggregateContainer_ID"]);
                    var currentChildId = Convert.ToInt32(r["AggregateConfiguration_ID"]);
                    var currentOrder = Convert.ToInt32(r["Order"]);

                    if (currentParentId != lastParentId)
                    {
                        _aggregatesInContainers.Add(currentParentId, new List<AggregateConfiguration>());
                        lastParentId = currentParentId;
                    }

                    AggregateConfiguration config;

                    try
                    {
                        
                        config = _childProvider.AllAggregateConfigurations.Single(a => a.ID == currentChildId);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Error occured trying to find AggregateConfiguration with ID " + currentChildId + " which is allegedly a child of CohortAggregateContainer " + currentParentId);
                    }

                    config.SetKnownOrder(currentOrder);

                    _aggregatesInContainers[currentParentId].Add(config);
                
                }
            }
        }

        public List<CohortAggregateContainer> GetSubContainers(CohortAggregateContainer container)
        {
            if (_subContainers.ContainsKey(container.ID))
                return _subContainers[container.ID];
            
            return new List<CohortAggregateContainer>();

        }

        public List<AggregateConfiguration> GetAggregateConfigurations(CohortAggregateContainer container)
        {
            if (_aggregatesInContainers.ContainsKey(container.ID))
                return _aggregatesInContainers[container.ID];

            return new List<AggregateConfiguration>();
        }
    }
}
