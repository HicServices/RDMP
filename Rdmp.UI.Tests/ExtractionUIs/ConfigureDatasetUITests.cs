using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ProjectUI.Datasets;

namespace Rdmp.UI.Tests.ExtractionUIs
{
    class ConfigureDatasetUITests : UITests
    {
        [Test,UITimeout(50000)]
        public void Test_RemoveAllColumns_Only1Publish()
        {
            var sds = WhenIHaveA<SelectedDataSets>();
            var ui = AndLaunch<ConfigureDatasetUI>(sds);
            
            AssertNoErrors();

            int publishCount = 0;

            //should be at least 2 in the config for this test to be sensible
            var cols = sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet);
            Assert.GreaterOrEqual(cols.Length,2);

            ItemActivator.RefreshBus.BeforePublish += (s,e)=>publishCount++;

            Assert.AreEqual(0, publishCount);

            ui.ExcludeAll();

            Assert.AreEqual(1,publishCount);

            AssertNoErrors();
            
            //should now be no columns in the extraction configuration
            Assert.IsEmpty(sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet));

            ui.IncludeAll();

            //should now be another publish event
            Assert.AreEqual(2, publishCount);

            //and the columns should be back in the configuration
            cols = sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet);
            Assert.GreaterOrEqual(cols.Length, 2);

            //multiple includes shouldnt change the number of columns
            ui.IncludeAll();
            ui.IncludeAll();
            ui.IncludeAll();

            Assert.AreEqual(cols.Length, sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet).Length);
        }
    }
}
