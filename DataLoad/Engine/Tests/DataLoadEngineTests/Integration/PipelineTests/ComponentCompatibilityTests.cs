using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration.PipelineTests
{
    public class ComponentCompatibilityTests :DatabaseTests
    {
        [Test]
        public void GetComponentsCompatibleWithBulkInsertContext()
        {
            Type[] array = CatalogueRepository.MEF.GetTypes<IDataFlowComponent<DataTable>>().ToArray();

            Assert.Greater(array.Count(),0);
        }

        [Test]
        public void HowDoesMEFHandleTypeNames()
        {

            string expected = "CatalogueLibrary.DataFlowPipeline.IDataFlowSource(System.Data.DataTable)";

            Assert.AreEqual(expected, MEF.GetMEFNameForType(typeof(IDataFlowSource<DataTable>)));
        }

      
    }
}

