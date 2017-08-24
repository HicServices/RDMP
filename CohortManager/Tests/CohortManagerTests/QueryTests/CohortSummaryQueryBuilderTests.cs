using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.FilterImporting;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Tests.Common;

namespace CohortManagerTests.QueryTests
{
    public class CohortSummaryQueryBuilderTests:DatabaseTests
    {
        private Catalogue c;
        private CatalogueItem ci;
        private CatalogueItem ci2;

        private TableInfo t;
        private ColumnInfo col;
        private ColumnInfo col2;

        private AggregateConfiguration acCohort;
        private AggregateConfiguration acDataset;
        private ExtractionInformation ei_Chi;
        
        private CohortIdentificationConfiguration cic;
        private CohortAggregateContainer container;
        private ExtractionInformation ei_Year;

        private ISqlParameter parama1;
        private ISqlParameter parama2;
        private AggregateFilterContainer container1;
        private AggregateFilterContainer container2;


        [SetUp]
        public void Setup()
        {
           c = new Catalogue(CatalogueRepository, "MyCata");
           ci = new CatalogueItem(CatalogueRepository, c, "MyCataItem");
           ci2 = new CatalogueItem(CatalogueRepository, c, "YearColumn");
           t = new TableInfo(CatalogueRepository, "MyTable");
           col = new ColumnInfo(CatalogueRepository, "mycol", "varchar(10)", t);
           col2 = new ColumnInfo(CatalogueRepository, "myOtherCol", "varchar(10)", t);

           
           acCohort = new AggregateConfiguration(CatalogueRepository, c, CohortIdentificationConfiguration.CICPrefix + "Agg1_Cohort");
           acDataset = new AggregateConfiguration(CatalogueRepository, c, "Agg2_Dataset");

           ei_Year = new ExtractionInformation(CatalogueRepository, ci2, col2, "Year");
           ei_Year.IsExtractionIdentifier = true;
           ei_Year.SaveToDatabase();
           acDataset.AddDimension(ei_Year);
           acDataset.CountSQL = "count(*)";
           acDataset.SaveToDatabase();


           ei_Chi = new ExtractionInformation(CatalogueRepository, ci, col, "CHI");
           ei_Chi.IsExtractionIdentifier = true;
           ei_Chi.SaveToDatabase();

           acCohort.AddDimension(ei_Chi);

           cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic");
           cic.CreateRootContainerIfNotExists();
           cic.RootCohortAggregateContainer.AddChild(acCohort,0);
        }
        #region Constructor Arguments
        [Test]
        public void ConstructorArguments_SameAggregateTwice()
        {
            var ex = Assert.Throws<ArgumentException>(()=>new CohortSummaryQueryBuilder(acCohort,acCohort));
            Assert.AreEqual("Summary and Cohort should be different aggregates.  Summary should be a graphable useful aggregate while cohort should return a list of private identifiers",ex.Message);
        }

        [Test]
        public void ConstructorArguments_Param1AccidentallyACohort()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CohortSummaryQueryBuilder(acCohort, acDataset));
            Assert.AreEqual("The first argument to constructor CohortSummaryQueryBuilder should be a basic AggregateConfiguration (i.e. not a cohort) but the argument you passed ('cic_Agg1_Cohort') was a cohort identification configuration aggregate", ex.Message);
        }
        [Test]
        public void ConstructorArguments_Param2AccidentallyAnAggregate()
        {
            //change it in memory so it doesn't look like a cohort aggregate anymore
            acCohort.Name = "RegularAggregate";
            var ex = Assert.Throws<ArgumentException>(() => new CohortSummaryQueryBuilder(acDataset,acCohort));
            Assert.AreEqual("The second argument to constructor CohortSummaryQueryBuilder should be a cohort identification aggregate (i.e. have a single AggregateDimension marked IsExtractionIdentifier and have a name starting with cic_) but the argument you passed ('RegularAggregate') was NOT a cohort identification configuration aggregate", ex.Message);
            acCohort.RevertToDatabaseState();
        }

        [Test]
        public void ConstructorArguments_DifferentDatasets()
        {
            acCohort.Catalogue_ID = -999999;
            var ex = Assert.Throws<ArgumentException>(() => new CohortSummaryQueryBuilder(acDataset, acCohort));

            Assert.IsTrue(ex.Message.StartsWith("Constructor arguments to CohortSummaryQueryBuilder must belong to the same dataset"));
            acCohort.RevertToDatabaseState();
        }

        [Test]
        public void ConstructorArguments_Normal()
        {
            Assert.DoesNotThrow(() => new CohortSummaryQueryBuilder(acDataset, acCohort));
        }
        #endregion


        [Test]
        public void QueryGeneration_BasicQuery()
        {
           string sql = acDataset.GetQueryBuilder().SQL;

           Assert.AreEqual(@"/*Agg2_Dataset*/
SELECT 
Year,
count(*)
FROM 
MyTable
group by 
Year
order by 
Year", sql);
        }

        [Test]
        public void QueryGeneration_WithLinkedCohort_WHERECHIIN()
        {
            var csqb = new CohortSummaryQueryBuilder(acDataset, acCohort);
            
            var ex = Assert.Throws<NotSupportedException>(() => csqb.GetAdjustedAggregateBuilder(CohortSummaryAdjustment.WhereExtractionIdentifiersIn));

            Assert.AreEqual("No Query Caching Server configured", ex.Message);
        }

        [Test]
        public void QueryGeneration_Parameters_DifferentValues_WHERECHIIN()
        {
            CreateParameters("'bob'","'fish'");

            try
            {
                var csqb = new CohortSummaryQueryBuilder(acDataset, acCohort);

                var ex = Assert.Throws<NotSupportedException>(() => csqb.GetAdjustedAggregateBuilder(CohortSummaryAdjustment.WhereExtractionIdentifiersIn));

                Assert.AreEqual("No Query Caching Server configured",ex.Message);
            }
            finally
            {
                DestroyParameters();
            }
        }



        [Test]
        public void QueryGeneration_NoCohortWhereLogic()
        {
            var csqb = new CohortSummaryQueryBuilder(acDataset, acCohort);

            var builder = csqb.GetAdjustedAggregateBuilder(CohortSummaryAdjustment.WhereRecordsIn);

            Assert.AreEqual(@"/*Agg2_Dataset*/
SELECT 
Year,
count(*)
FROM 
MyTable
group by 
Year
order by 
Year", builder.SQL);
        }

        [Test]
        public void QueryGeneration_BothHaveWHEREContainerAndParameters()
        {
            CreateParameters("'bob'", "'fish'");

            var global = new AnyTableSqlParameter(CatalogueRepository, cic, "DECLARE @bob AS VARCHAR(50)");
            global.Value = "'zomber'";
            global.SaveToDatabase();

            try
            {
                ((IDeleteable)parama1).DeleteInDatabase();
                var csqb = new CohortSummaryQueryBuilder(acDataset, acCohort);

                var builder = csqb.GetAdjustedAggregateBuilder(CohortSummaryAdjustment.WhereRecordsIn);

                Assert.AreEqual(@"DECLARE @bob AS VARCHAR(50);
SET @bob='zomber';
/*Agg2_Dataset*/
SELECT 
Year,
count(*)
FROM 
MyTable
WHERE
(
	(
	/*Filter2*/
	@bob = 'fish'
	)
AND
	(
	/*Filter1*/
	@bob = 'bob'
	)
)

group by 
Year
order by 
Year", builder.SQL);

            }
            finally
            {
                global.DeleteInDatabase();
                DestroyParameters();
            }
        }
        private void DestroyParameters()
        {
            container1.GetFilters()[0].DeleteInDatabase();
            container2.GetFilters()[0].DeleteInDatabase();

            container1.DeleteInDatabase();
            container2.DeleteInDatabase();
        }

        private void CreateParameters(string param1Value,string param2Value)
        {
            container1 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            acDataset.RootFilterContainer_ID = container1.ID;
            acDataset.SaveToDatabase();

            AggregateFilter filter1 = new AggregateFilter(CatalogueRepository, "Filter1", container1);
            filter1.WhereSQL = "@bob = 'bob'";
            filter1.SaveToDatabase();

            var paramCreator = new ParameterCreator(filter1.GetFilterFactory(), null, null);
            paramCreator.CreateAll(filter1, null);

            container2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            acCohort.RootFilterContainer_ID = container2.ID;
            acCohort.SaveToDatabase();

            AggregateFilter filter2 = new AggregateFilter(CatalogueRepository, "Filter2", container2);
            filter2.WhereSQL = "@bob = 'fish'";
            filter2.SaveToDatabase();

            paramCreator.CreateAll(filter2, null);

            parama1 = filter1.GetAllParameters()[0];
            parama1.Value = param1Value;
            parama1.SaveToDatabase();

            parama2 = filter2.GetAllParameters()[0];
            parama2.Value = param2Value;
            parama2.SaveToDatabase();
            
        }

        [TearDown]
        public void TearDown()
        {
            cic.DeleteInDatabase();

            acCohort.DeleteInDatabase();
            acDataset.DeleteInDatabase();

            t.DeleteInDatabase();
            c.DeleteInDatabase();
        }

    }
}
