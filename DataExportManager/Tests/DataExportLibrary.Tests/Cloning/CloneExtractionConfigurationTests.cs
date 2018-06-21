using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using DataExportLibrary.Tests.DataExtraction;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.UserPicks;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using Tests.Common;


namespace DataExportLibrary.Tests.Cloning
{
    public class CloneExtractionConfigurationTests:TestsRequiringAnExtractionConfiguration
    {

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CloneWithFilters(bool introduceOrphanExtractionInformation)
        {
            if (introduceOrphanExtractionInformation)
                IntroduceOrphan();

            var filter = new ExtractionFilter(CatalogueRepository, "FilterByFish", _extractionInformations[0]);
            try
            {
                //setup a filter with a parameter
                filter.WhereSQL = "Fish = @fish";

                new ParameterCreator(new ExtractionFilterFactory(_extractionInformations[0]), null, null).CreateAll(filter,null);
                filter.SaveToDatabase();

                Assert.IsTrue(filter.ExtractionFilterParameters.Count()==1);

                //create a root container
                var container =  new FilterContainer(DataExportRepository);
                _selectedDataSet.RootFilterContainer_ID = container.ID;
                _selectedDataSet.SaveToDatabase();

                //create a deployed filter
                var importer = new FilterImporter(new DeployedExtractionFilterFactory(DataExportRepository), null);
                var deployedFilter = (DeployedExtractionFilter)importer.ImportFilter(filter, null);
                deployedFilter.FilterContainer_ID = container.ID;
                deployedFilter.Name = "FilterByFishDeployed";
                deployedFilter.SaveToDatabase();

                var param = deployedFilter.ExtractionFilterParameters[0];
                param.Value = "'jormungander'";
                param.SaveToDatabase();

                ExtractDatasetCommand request = new ExtractDatasetCommand(RepositoryLocator,_configuration,new ExtractableDatasetBundle(_extractableDataSet));
                request.GenerateQueryBuilder();
                Assert.AreEqual(
                    CollapseWhitespace(
string.Format(
@"DECLARE @fish AS varchar(50);
SET @fish='jormungander';
/*The ID of the cohort in [{0}CohortDatabase]..[Cohort]*/
DECLARE @CohortDefinitionID AS int;
SET @CohortDefinitionID=-599;
/*The project number of project {0}ExtractionConfiguration*/
DECLARE @ProjectNumber AS int;
SET @ProjectNumber=1;

SELECT DISTINCT 
[{0}CohortDatabase]..[Cohort].[ReleaseID] AS ReleaseID,
[{0}ScratchArea]..[TestTable].[Name],
[{0}ScratchArea]..[TestTable].[DateOfBirth]
FROM 
[{0}ScratchArea]..[TestTable] INNER JOIN [{0}CohortDatabase]..[Cohort] ON [{0}ScratchArea]..[TestTable].[PrivateID]=[{0}CohortDatabase]..[Cohort].[PrivateID] collate Latin1_General_BIN

WHERE
(
/*FilterByFishDeployed*/
Fish = @fish
)
AND
[{0}CohortDatabase]..[Cohort].[cohortDefinition_id]=-599
"
  , TestDatabaseNames.Prefix))
  ,CollapseWhitespace(request.QueryBuilder.SQL));

                ExtractionConfiguration deepClone = _configuration.DeepCloneWithNewIDs();
                Assert.AreEqual(deepClone.Cohort_ID,_configuration.Cohort_ID);
                Assert.AreNotEqual(deepClone.ID,_configuration.ID);
                try
                {
                    ExtractDatasetCommand request2 = new ExtractDatasetCommand(RepositoryLocator,deepClone, new ExtractableDatasetBundle(_extractableDataSet));
                    request2.GenerateQueryBuilder();
                
                    Assert.AreEqual(request.QueryBuilder.SQL,request2.QueryBuilder.SQL);

                }
                finally
                {
                    deepClone.DeleteInDatabase();
                }
            }
            finally 
            {
                
                filter.DeleteInDatabase();
            }
        }

        public void IntroduceOrphan()
        {
            var cols = _configuration.GetAllExtractableColumnsFor(_extractableDataSet).Cast<ExtractableColumn>().ToArray();

            var name = cols.Single(c => c.GetRuntimeName().Equals("Name"));

            using (var con = DataExportRepository.GetConnection())
            {
                DataExportRepository.DiscoveredServer.GetCommand(
                    "UPDATE ExtractableColumn set CatalogueExtractionInformation_ID = " + int.MaxValue + " where ID = " +
                    name.ID, con).ExecuteNonQuery();
            }

        }
    }
}
