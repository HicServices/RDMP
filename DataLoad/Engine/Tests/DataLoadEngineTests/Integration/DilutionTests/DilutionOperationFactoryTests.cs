using System;
using ANOStore.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using LoadModules.Generic.Mutilators.Dilution;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.DilutionTests
{
    public class DilutionOperationFactoryTests : DatabaseTests
    {
        [Test]
        public void NullColumn_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DilutionOperationFactory(null));
        }

        [Test]
        public void NullOperation_Throws()
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();
            col.Stub(p => p.Repository).Return(CatalogueRepository);

            var factory = new DilutionOperationFactory(col);
            Assert.Throws<ArgumentNullException>(()=>factory.Create(null));
        }

        [Test]
        public void UnexpectedType_Throws()
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();
            col.Stub(p => p.Repository).Return(CatalogueRepository);

            var factory = new DilutionOperationFactory(col);
            Assert.Throws<ArgumentException>(()=>factory.Create(typeof(Catalogue)));
        }

        [Test]
        public void ExpectedType_Created()
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();
            col.Stub(p => p.Repository).Return(CatalogueRepository);

            var factory = new DilutionOperationFactory(col);
            var i = factory.Create(typeof(ExcludeRight3OfUKPostcodes));
            Assert.IsNotNull(i);
            Assert.IsInstanceOf<IDilutionOperation>(i);
        }
    }
}
