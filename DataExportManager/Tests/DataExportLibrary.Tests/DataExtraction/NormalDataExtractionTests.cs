using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using NUnit.Framework;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class NormalDataExtractionTests:TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void ExtractNormally()
        {
            ExtractionPipelineUseCase execute;
            IExecuteDatasetExtractionDestination result;

            _catalogue.Name = "TestTable";
            _catalogue.SaveToDatabase();
            _request.DatasetBundle.DataSet.RevertToDatabaseState();

            Assert.AreEqual(1, _request.ColumnsToExtract.Count(c => c.IsExtractionIdentifier));
            
            base.Execute(out execute,out result);

            var r = (ExecuteDatasetExtractionFlatFileDestination)result;

            //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
            Assert.AreEqual(@"ReleaseID,Result
" + _cohortKeysGenerated[_cohortKeysGenerated.Keys.First()] + @",1", File.ReadAllText(r.OutputFile).Trim()); 

            Assert.AreEqual(1, _request.QueryBuilder.SelectColumns.Count(c => c.IColumn is ReleaseIdentifierSubstitution));
            File.Delete(r.OutputFile);
        }


        [Test]
        public void DodgyCharactersInCatalogueName()
        {
            string beforeName = _catalogue.Name;
            try
            {
                _catalogue.Name = "Fish;#:::FishFish";
                Assert.IsFalse(Catalogue.IsAcceptableName(_catalogue.Name));
                _catalogue.SaveToDatabase();
                _extractableDataSet.RevertToDatabaseState();

                var extractionDirectory = new ExtractionDirectory(@"c:\temp", _configuration);

            
                var ex = Assert.Throws<NotSupportedException>(() => {var dir = extractionDirectory.GetDirectoryForDataset(_extractableDataSet); });

                Assert.AreEqual("Cannot extract dataset Fish;#:::FishFish because it points at Catalogue with an invalid name, name is invalid because:The following invalid characters were found:'#'", ex.Message);
            }
            finally
            {
                _catalogue.Name = beforeName;
                _catalogue.SaveToDatabase();
                _extractableDataSet.RevertToDatabaseState();
            }
        }
    }
}
