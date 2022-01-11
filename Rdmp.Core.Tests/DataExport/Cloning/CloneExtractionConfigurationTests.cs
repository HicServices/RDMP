// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Cloning
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

            Assert.IsEmpty(_configuration.ReleaseLog);

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

                ExtractDatasetCommand request = new ExtractDatasetCommand(_configuration,new ExtractableDatasetBundle(_extractableDataSet));
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
[{0}ScratchArea].[dbo].[TestTable].[Name],
[{0}ScratchArea].[dbo].[TestTable].[DateOfBirth]
FROM 
[{0}ScratchArea].[dbo].[TestTable] INNER JOIN [{0}CohortDatabase]..[Cohort] ON [{0}ScratchArea].[dbo].[TestTable].[PrivateID]=[{0}CohortDatabase]..[Cohort].[PrivateID]

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
                    ExtractDatasetCommand request2 = new ExtractDatasetCommand(deepClone, new ExtractableDatasetBundle(_extractableDataSet));
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


        [Test]
        public void CloneWithExtractionProgress()
        {
            var sds = _configuration.SelectedDataSets[0];
            var ci = sds.GetCatalogue().CatalogueItems.First();
            var origProgress = new ExtractionProgress(DataExportRepository, sds, null, DateTime.Now, 10, "fff drrr", ci.ID);
            origProgress.ProgressDate = new DateTime(2001, 01, 01);
            origProgress.SaveToDatabase();

            ExtractionConfiguration deepClone = _configuration.DeepCloneWithNewIDs();
            Assert.AreEqual(deepClone.Cohort_ID, _configuration.Cohort_ID);
            Assert.AreNotEqual(deepClone.ID, _configuration.ID);

            var clonedSds = deepClone.SelectedDataSets.Single(s => s.ExtractableDataSet_ID == sds.ExtractableDataSet_ID);

            var clonedProgress = clonedSds.ExtractionProgressIfAny;

            Assert.IsNotNull(clonedProgress);
            Assert.IsNull(clonedProgress.StartDate);
            Assert.IsNull(clonedProgress.ProgressDate, "Cloning a ExtractionProgress should reset its ProgressDate back to null in anticipation of it being extracted again");
            
            Assert.AreEqual(clonedProgress.EndDate, origProgress.EndDate);
            Assert.AreEqual(clonedProgress.NumberOfDaysPerBatch, origProgress.NumberOfDaysPerBatch);
            Assert.AreEqual(clonedProgress.Name, origProgress.Name);
            Assert.AreEqual(clonedProgress.ExtractionInformation_ID, origProgress.ExtractionInformation_ID);


            deepClone.DeleteInDatabase();

            // remove the progress so that it doesn't trip other tests
            origProgress.DeleteInDatabase();
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
