using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Unit
{
    class IMightBeReadOnlyTests : UnitTests
    {
        [Test]
        public void IsReadonly_AggregateFilterContainer()
        {
            //im probably an orphan
            var c = WhenIHaveA<AggregateFilterContainer>();
            Assert.IsFalse(c.ShouldBeReadOnly(out _));

            //now I am in a cic
            var cic = WhenIHaveA<CohortIdentificationConfiguration>();
            cic.Name = "fff";
            cic.CreateRootContainerIfNotExists();
            cic.RootCohortAggregateContainer.AddChild(c.GetAggregate(),0);
            
            Assert.IsFalse(c.ShouldBeReadOnly(out _));

            cic.Frozen = true;
            Assert.IsTrue(c.ShouldBeReadOnly(out string reason));
            
            Assert.AreEqual("fff is Frozen",reason);
        }
        
        [Test]
        public void IsReadonly_ExtractionFilterContainer()
        {
            var c = WhenIHaveA<FilterContainer>();
            Assert.IsFalse(c.ShouldBeReadOnly(out _));

            var ec = c.GetSelectedDataSetIfAny().ExtractionConfiguration;
            
            Assert.IsFalse(c.ShouldBeReadOnly(out _));

            ec.Name = "lll";
            ec.IsReleased = true;
            Assert.IsTrue(c.ShouldBeReadOnly(out string reason));
            
            Assert.AreEqual("lll has already been released",reason);
        }

        [Test]
        public void IsReadonly_SpontaneousContainer()
        {
            var memoryrepo = new MemoryCatalogueRepository();
            var c = new SpontaneouslyInventedFilterContainer(memoryrepo,null,null,FilterContainerOperation.AND);
            Assert.IsFalse(c.ShouldBeReadOnly(out _),"Spont containers should never be in UI but lets not tell the programmer they shouldn't be edited");
        }


        
        [Test]
        public void IsReadonly_AggregateFilter()
        {
            //im probably an orphan
            var f = WhenIHaveA<AggregateFilter>();
            Assert.IsFalse(f.ShouldBeReadOnly(out _));

            //now I am in a cic
            var cic = WhenIHaveA<CohortIdentificationConfiguration>();
            cic.Name = "fff";
            cic.CreateRootContainerIfNotExists();
            cic.RootCohortAggregateContainer.AddChild(f.GetAggregate(),0);
            
            Assert.IsFalse(f.ShouldBeReadOnly(out _));

            cic.Frozen = true;
            Assert.IsTrue(f.ShouldBeReadOnly(out string reason));
            
            Assert.AreEqual("fff is Frozen",reason);
        }
        
        [Test]
        public void IsReadonly_DeployedExtractionFilter()
        {
            var f = WhenIHaveA<DeployedExtractionFilter>();
            Assert.IsFalse(f.ShouldBeReadOnly(out _));

            var ec = ((FilterContainer) f.FilterContainer).GetSelectedDataSetIfAny().ExtractionConfiguration;
            ec.Name = "lll";
            ec.IsReleased = true;
            Assert.IsTrue(f.ShouldBeReadOnly(out string reason));
            
            Assert.AreEqual("lll has already been released",reason);
        }
    }
}
