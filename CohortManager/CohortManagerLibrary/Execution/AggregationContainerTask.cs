using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CohortManagerLibrary.Execution
{
    /// <summary>
    /// The runtime/compile time wrapper for CohortAggregateContainer. UNION,EXCEPT,INTERSECT containers with 0 or more AggregateConfigurations within
    /// them - also optionally with other sub containers.
    /// </summary>
    public class AggregationContainerTask : Compileable,IOrderable
    {
        public CohortAggregateContainer Container { get; set; }
        
        public CohortAggregateContainer[] SubContainers { get; set; }
        public AggregateConfiguration[] ContainedConfigurations { get; set; }

        public AggregationContainerTask(CohortAggregateContainer container, CohortCompiler compiler):base(compiler)
        {
            Container = container;

            SubContainers = Container.GetSubContainers();
            ContainedConfigurations = Container.GetAggregateConfigurations();

        }

        public override string GetCatalogueName()
        {
            return "";
        }

        public override IMapsDirectlyToDatabaseTable Child { get { return Container; } }
        
        public override IDataAccessPoint[] GetDataAccessPoints()
        {
            var cataIDs = Container.GetAggregateConfigurations().Select(c => c.Catalogue_ID).Distinct().ToList();

            //if this container does not have any configurations
            if (!cataIDs.Any())//try looking at the subcontainers
            {
                var subcontainers =  Container.GetSubContainers().FirstOrDefault(subcontainer => subcontainer.GetAggregateConfigurations().Any());
                if(subcontainers != null)
                    cataIDs = subcontainers.GetAggregateConfigurations().Select(c => c.Catalogue_ID).Distinct().ToList();
            }

            //none of the subcontainers have any catalogues either!
            if(!cataIDs.Any())
                throw new Exception("Aggregate Container " + Container.ID + " does not have any datasets in it and neither does an of its direct subcontainers have any, how far down the tree do you expect me to look!");

            var catas = Container.Repository.GetAllObjectsInIDList<Catalogue>(cataIDs);

            return catas.SelectMany(c => c.GetTableInfoList(false)).ToArray();
        }

        public string DescribeOperation()
        {

            switch (((CohortAggregateContainer)Child).Operation)
            {
                case SetOperation.UNION:
                    return 
@"Includes ALL patients which appear in any of the sets in this container.  If there are subcontainers
(i.e. other UNION/INTERSECT/EXCEPT blocks under this one) then the UNION operation will be applied to the
result of the subcontainer.";
                case SetOperation.INTERSECT:
                    return
@"Only includes patients which appear in EVERY set in this container.  This means that for a patient to
returned by this operation they must be in all the sets under this (including the results of any subcontainers)";
                case SetOperation.EXCEPT:
                    return
@"Includes ALL patients in the FIRST set (or subcontainer) under this container but ONLY if they DO NOT
APPEAR in any of the sets that come after the FIRST.  This means that you get everyone in the first set
EXCEPT anyone appearing in any of the other sets that follow the FIRST.";
                default : throw new ArgumentOutOfRangeException("Did not know what tool tip to return for set operation " + ToString());

            }
        }

    }
}
 